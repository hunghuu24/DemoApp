$script:RmProjectRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$script:RmBackendRoot = Join-Path $script:RmProjectRoot 'Backend'
$script:RmApiRoot = Join-Path $script:RmBackendRoot 'src/ReMarket.Api'
$script:RmRuntimeRoot = Join-Path $script:RmBackendRoot '.local/runtime'
$script:RmUrl = 'http://127.0.0.1:5080'
$hashProvider = [Security.Cryptography.SHA256]::Create()
try { $workspaceHash = [BitConverter]::ToString($hashProvider.ComputeHash([Text.Encoding]::UTF8.GetBytes($script:RmProjectRoot.ToLowerInvariant()))).Replace('-','').Substring(0,16) }
finally { $hashProvider.Dispose() }
$script:RmMutexName = 'Local\ReMarketLocal-' + $workspaceHash

function Get-ReMarketState {
    try {
        $state = Invoke-RestMethod ($script:RmUrl + '/api/hethong/trangthai') -TimeoutSec 2
        if ($state.tenUngDung -eq 'ReMarket') { return $state }
    } catch { }
    return $null
}

function Ensure-ReMarketBackend {
    # Serialize cold starts from Gradle, the launcher and the reconnect worker.
    $guard = New-Object Threading.Mutex($false, ($script:RmMutexName + '-Backend'))
    $locked = $false
    try {
        try { $locked = $guard.WaitOne(60000) } catch [Threading.AbandonedMutexException] { $locked = $true }
        if (-not $locked) { throw 'Backend startup is still in progress. Retry Run after it finishes.' }
        $state = Get-ReMarketState
        if ($null -eq $state) {
            if (Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue) {
                throw 'Port 5080 is occupied by an unrecognized or unresponsive host; it was not terminated.'
            }
            . (Join-Path $script:RmBackendRoot 'scripts/Use-DotNet8.ps1')
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
            & (Join-Path $script:RmBackendRoot 'scripts/Initialize-Authentication.ps1') | Out-Host
            $buildArguments = @('build', (Join-Path $script:RmApiRoot 'ReMarket.Api.csproj'), '--configuration', 'Release', '--nologo', '--verbosity', 'minimal')
            $projects = @(Get-ChildItem -Path (Join-Path $script:RmBackendRoot 'src/*/*.csproj'))
            $restoreRequired = $projects.Count -eq 0
            $centralInputs = @(Get-ChildItem -Path (Join-Path $script:RmBackendRoot '*.props'),(Join-Path $script:RmBackendRoot 'NuGet.config') -ErrorAction SilentlyContinue)
            foreach ($projectFile in $projects) {
                $assets = Join-Path $projectFile.DirectoryName 'obj/project.assets.json'
                if (-not (Test-Path -LiteralPath $assets)) { $restoreRequired = $true; continue }
                $restoredAt = (Get-Item -LiteralPath $assets).LastWriteTimeUtc
                if ($projectFile.LastWriteTimeUtc -gt $restoredAt -or @($centralInputs | Where-Object { $_.LastWriteTimeUtc -gt $restoredAt }).Count) { $restoreRequired = $true }
            }
            if (-not $restoreRequired) { $buildArguments += '--no-restore' }
            Push-Location $script:RmBackendRoot
            try {
                & dotnet @buildArguments | Out-Host
                if ($LASTEXITCODE -ne 0) { throw 'Backend build failed. See the output above.' }
            } finally { Pop-Location }
            $null = New-Item -ItemType Directory -Path $script:RmRuntimeRoot -Force
            $server = Start-Process -FilePath (Get-Command dotnet.exe).Source -WorkingDirectory $script:RmApiRoot `
                -ArgumentList @('bin/Release/net8.0/ReMarket.Api.dll','--urls',$script:RmUrl) -WindowStyle Hidden `
                -RedirectStandardOutput (Join-Path $script:RmRuntimeRoot 'api.stdout.log') `
                -RedirectStandardError (Join-Path $script:RmRuntimeRoot 'api.stderr.log') -PassThru
            [pscustomobject]@{ pid=$server.Id; startedUtc=$server.StartTime.ToUniversalTime().ToString('o'); url=$script:RmUrl } |
                ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:RmRuntimeRoot 'api-process.json') -Encoding UTF8
        }
        $deadline = [DateTime]::UtcNow.AddSeconds(40)
        do {
            $state = Get-ReMarketState
            if ($null -ne $state -and $state.coSoDuLieuSanSang) { return }
            Start-Sleep -Milliseconds 500
        } while ([DateTime]::UtcNow -lt $deadline)
        throw "Backend/SQL is not ready. Check SQL Server and logs in $script:RmRuntimeRoot. No migration or seed was run."
    } finally {
        if ($locked) { $guard.ReleaseMutex() }
        $guard.Dispose()
    }
}

function Get-ReMarketAdb {
    # Match Android Studio's SDK first; a different adb on PATH can restart its server.
    $properties = Join-Path $script:RmProjectRoot 'Android/local.properties'
    if (Test-Path -LiteralPath $properties) {
        $sdkLine = Get-Content -LiteralPath $properties | Where-Object { $_ -match '^sdk\.dir=' } | Select-Object -First 1
        if ($sdkLine) {
            $sdkDirectory = $sdkLine.Substring(8).Replace('\\','\').Replace('\:',':').Replace('\ ',' ')
            $candidate = Join-Path $sdkDirectory 'platform-tools/adb.exe'
            if (Test-Path -LiteralPath $candidate) { return $candidate }
        }
    }
    foreach ($sdkDirectory in @($env:ANDROID_HOME,$env:ANDROID_SDK_ROOT)) {
        if ($sdkDirectory) {
            $candidate = Join-Path $sdkDirectory 'platform-tools/adb.exe'
            if (Test-Path -LiteralPath $candidate) { return $candidate }
        }
    }
    $command = Get-Command adb.exe -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    throw 'Cannot find adb. Set sdk.dir in Android/local.properties to the Android Studio SDK.'
}

function Invoke-ReMarketAdb([string]$Adb, [string[]]$Arguments) {
    # An offline transport can write stderr without being a fatal PowerShell error.
    $ErrorActionPreference = 'Continue'
    $output = @(& $Adb @Arguments 2>&1)
    return [pscustomobject]@{ code=$LASTEXITCODE; text=($output -join "`n") }
}

function Connect-ReMarketDevicePorts([string]$Adb, [string]$DeviceSerial) {
    $listing = Invoke-ReMarketAdb $Adb @('devices')
    if ($listing.code -ne 0) { throw ('ADB devices failed: ' + $listing.text) }
    foreach ($line in ($listing.text -split "`r?`n")) {
        if ($line -notmatch '^(\S+)\s+(device|offline|unauthorized)\b') { continue }
        $serial = $Matches[1]; $deviceState = $Matches[2]
        if ($DeviceSerial -and $serial -ne $DeviceSerial) { continue }
        $forwarded = $false; $message = $null
        if ($deviceState -eq 'device') {
            $mapping = Invoke-ReMarketAdb $Adb @('-s',$serial,'reverse','--list')
            $forwarded = $mapping.code -eq 0 -and $mapping.text -match '(?m)^\S+\s+tcp:5080\s+tcp:5080\s*$'
            if (-not $forwarded) {
                $created = Invoke-ReMarketAdb $Adb @('-s',$serial,'reverse','tcp:5080','tcp:5080')
                if ($created.code -eq 0) {
                    $verified = Invoke-ReMarketAdb $Adb @('-s',$serial,'reverse','--list')
                    $forwarded = $verified.code -eq 0 -and $verified.text -match '(?m)^\S+\s+tcp:5080\s+tcp:5080\s*$'
                } else { $message = $created.text }
            }
        }
        [pscustomobject]@{ serial=$serial; state=$deviceState; apiForwarded=[bool]$forwarded; error=$message }
    }
}

function Connect-ReMarketWirelessEndpoint([string]$Adb, [string]$Endpoint) {
    $client = New-Object Diagnostics.Process
    $client.StartInfo = New-Object Diagnostics.ProcessStartInfo
    $client.StartInfo.FileName = $Adb
    $client.StartInfo.Arguments = 'connect ' + $Endpoint
    $client.StartInfo.UseShellExecute = $false
    $client.StartInfo.CreateNoWindow = $true
    $client.StartInfo.RedirectStandardOutput = $true
    $client.StartInfo.RedirectStandardError = $true
    try {
        $null = $client.Start()
        if (-not $client.WaitForExit(8000)) { $client.Kill(); $client.WaitForExit(); return }
        # Read bounded adb output without printing device/authentication details.
        $null = $client.StandardOutput.ReadToEnd()
        $null = $client.StandardError.ReadToEnd()
    } finally { $client.Dispose() }
}

function Connect-ReMarketDevices([string]$Adb, [string]$DeviceSerial) {
    $guard = New-Object Threading.Mutex($false, ($script:RmMutexName + '-Adb'))
    $locked = $false
    try {
        try { $locked = $guard.WaitOne(15000) } catch [Threading.AbandonedMutexException] { $locked = $true }
        if (-not $locked) { throw 'ADB reconnect is in progress. Retry shortly.' }
        $listing = Invoke-ReMarketAdb $Adb @('devices')
        if ($listing.code -ne 0) { throw ('ADB devices failed: ' + $listing.text) }
        $online = @($listing.text -split "`r?`n" | ForEach-Object { if ($_ -match '^(\S+)\s+device\b') { $Matches[1] } })
        $discovery = Invoke-ReMarketAdb $Adb @('mdns','services')
        $services = @($discovery.text -split "`r?`n" | ForEach-Object {
            if ($_ -match '^(\S+)\s+_adb-tls-connect\._tcp\.?\s+(\S+:\d+)\s*$') {
                $name=$Matches[1]; $endpoint=$Matches[2]
                [pscustomobject]@{ name=$name; endpoint=$endpoint; key=($name -replace '-[^-]+$',''); serial=($name+'._adb-tls-connect._tcp') }
            }
        })
        $knownPath = Join-Path $script:RmRuntimeRoot 'known-wireless.json'
        [string[]]$known = @()
        $knownChanged = $false
        if (Test-Path -LiteralPath $knownPath) {
            $saved = Get-Content -LiteralPath $knownPath -Raw -Encoding UTF8 | ConvertFrom-Json
            foreach ($identity in $saved.devices) {
                if ($identity -is [string] -and $identity -match '^adb-.+') { $known += [string]$identity }
            }
        }
        # Learn identities only from an already-authorized ADB transport, not arbitrary LAN advertisements.
        foreach ($service in $services) {
            if ($service.serial -in $online -or $service.endpoint -in $online -or $service.key.Substring(4) -in $online) {
                if ($service.key -notin $known) { $known += [string]$service.key; $knownChanged=$true }
            }
        }
        if ($knownChanged -and $known.Count) {
            $null = New-Item -ItemType Directory -Path $script:RmRuntimeRoot -Force
            [pscustomobject]@{ devices=[string[]]$known } | ConvertTo-Json | Set-Content -LiteralPath $knownPath -Encoding UTF8
        }
        foreach ($service in $services) {
            if ($service.key -in $known -and $service.serial -notin $online -and $service.endpoint -notin $online) {
                # TLS pairing is still enforced by adb; never pair, scan IP ranges or reuse an old IP/port.
                # Keep the mDNS transport name so Studio does not get a duplicate IP transport.
                Connect-ReMarketWirelessEndpoint $Adb $service.serial
            }
        }
        Connect-ReMarketDevicePorts $Adb $DeviceSerial
    } finally {
        if ($locked) { $guard.ReleaseMutex() }
        $guard.Dispose()
    }
}

function Get-ReMarketWatcher {
    $recordPath = Join-Path $script:RmRuntimeRoot 'watcher-process.json'
    if (-not (Test-Path -LiteralPath $recordPath)) { return $null }
    try {
        $record = Get-Content -LiteralPath $recordPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $process = Get-CimInstance Win32_Process -Filter ('ProcessId=' + [int]$record.pid)
        $expectedScript = Join-Path $script:RmProjectRoot 'tools/Watch-ReMarketLocal.ps1'
        if ($process -and $process.CommandLine -and $process.CommandLine.IndexOf($expectedScript,[StringComparison]::OrdinalIgnoreCase) -ge 0 `
            -and [Math]::Abs(($process.CreationDate.ToUniversalTime() - [DateTime]::Parse($record.startedUtc).ToUniversalTime()).TotalSeconds) -lt 5) {
            return $process
        }
    } catch { }
    return $null
}
