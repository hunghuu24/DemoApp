param([switch]$ExerciseRecovery, [string]$AdminEmail, [string]$AdminPassword)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReMarketLocal.Common.ps1')
$qaDirectory = Join-Path $script:RmProjectRoot 'docs/qa/local-connectivity'
$null = New-Item -ItemType Directory -Path $qaDirectory -Force
$checks = [Collections.Generic.List[object]]::new()
$accessToken = $null
function Record-Check([string]$Name, [bool]$Passed, $Detail) {
    $checks.Add([pscustomobject]@{ name=$Name; passed=$Passed; detail=$Detail })
    Write-Output ($Name + ': ' + $(if ($Passed) { 'PASS' } else { 'FAIL' }))
    if (-not $Passed) { throw ('Check failed: ' + $Name) }
}
function Invoke-PhoneHttp([string]$Path) {
    $request = "GET $Path HTTP/1.0`r`nHost: 127.0.0.1:5080`r`nConnection: close`r`n`r`n"
    $encoded = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($request))
    # Keep stdin open briefly: adb reverse does not provide TCP half-close semantics.
    $remote = @('(',('echo '+$encoded+' | toybox base64 -d'),'sleep 3',') | toybox nc -w 5 -W 8 127.0.0.1 5080') -join "`n"
    $response = Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'shell','-T',$remote)
    return $response.text
}
function Stop-VerifiedTestBackend {
    $record = Get-Content -LiteralPath (Join-Path $script:RmRuntimeRoot 'api-process.json') -Raw -Encoding UTF8 | ConvertFrom-Json
    $server = Get-CimInstance Win32_Process -Filter ('ProcessId=' + [int]$record.pid)
    $listener = Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction Stop
    $sameStart = $server -and [Math]::Abs(($server.CreationDate.ToUniversalTime() - [DateTime]::Parse($record.startedUtc).ToUniversalTime()).TotalSeconds) -lt 5
    if (-not $sameStart -or $server.CommandLine -notlike '*ReMarket.Api.dll*' -or $server.ProcessId -notin $listener.OwningProcess -or $null -eq (Get-ReMarketState)) {
        throw 'Refusing to stop an unverified process. Only the local helper-owned ReMarket host can be used for recovery tests.'
    }
    Stop-Process -Id $server.ProcessId
    return $server.ProcessId
}
function Invoke-TestGradle([string]$LogName) {
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & (Join-Path $script:RmProjectRoot 'Android/gradlew.bat') --project-dir (Join-Path $script:RmProjectRoot 'Android') --daemon --max-workers=1 `
        '-Dorg.gradle.jvmargs=-Xmx1536m -Dfile.encoding=UTF-8' '-Pkotlin.compiler.execution.strategy=in-process' :app:assembleDebug *> (Join-Path $qaDirectory $LogName)
    $buildExit = $LASTEXITCODE
    $ErrorActionPreference = $previousPreference
    if ($buildExit -ne 0) { throw ('Gradle failed; see ' + $LogName) }
}
try {
    & (Join-Path $PSScriptRoot 'Start-ReMarketLocal.ps1') -AutoConnect
    $adb = Get-ReMarketAdb
    $phones = @(Connect-ReMarketDevices $adb | Where-Object { $_.apiForwarded -and $_.serial -notlike 'emulator-*' })
    Record-Check 'One authorized physical phone' ($phones.Count -eq 1) $phones.Count
    $script:testSerial = $phones[0].serial
    $physicalId = (Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'shell','getprop','ro.serialno')).text.Trim()
    $state = Get-ReMarketState
    Record-Check 'Backend + SQL + migrations ready' ($state.coSoDuLieuSanSang -eq $true) $state.tenUngDung
    $workerBefore = (Get-ReMarketWatcher).ProcessId
    & (Join-Path $PSScriptRoot 'Start-ReMarketLocal.ps1') -AutoConnect
    Record-Check 'Repeated startup reuses one watcher' ((Get-ReMarketWatcher).ProcessId -eq $workerBefore) $workerBefore
    if ($AdminEmail -and $AdminPassword) {
        $body = @{ taiKhoan=$AdminEmail; matKhau=$AdminPassword } | ConvertTo-Json -Compress
        $login = Invoke-RestMethod -Method Post -Uri ($script:RmUrl+'/api/taikhoan/dangnhap') -ContentType 'application/json' -Body $body
        $accessToken = $login.accessToken
        Record-Check 'Real Admin login' ([bool]$accessToken -and 'ADMIN' -in $login.hoSo.vaiTro) 'JWT received; value not logged'
        Remove-Variable login, body
    }
    if ($ExerciseRecovery) {
        & (Join-Path $PSScriptRoot 'Stop-ReMarketWatcher.ps1')
        $coldPid = Stop-VerifiedTestBackend
        Record-Check 'Cold startup precondition' ($null -eq (Get-ReMarketState)) 'Backend and watcher stopped; SQL and phone left running'
        Invoke-TestGradle 'android-cold-start-build.txt'
        Record-Check 'Debug build starts API and watcher automatically' ([bool](Get-ReMarketState).coSoDuLieuSanSang -and $null -ne (Get-ReMarketWatcher)) $coldPid
        if ($accessToken) {
            $profile = Invoke-RestMethod -Uri ($script:RmUrl+'/api/taikhoan/toi') -Headers @{ Authorization=('Bearer '+$accessToken) }
            Record-Check 'JWT survives Backend restart' ('ADMIN' -in $profile.vaiTro) 'Existing signing key and database session preserved'
        }
        Invoke-TestGradle 'android-warm-start-build.txt'
        $warmLog = [IO.File]::ReadAllText((Join-Path $qaDirectory 'android-warm-start-build.txt'))
        Record-Check 'Cached Debug build still prepares API' ($warmLog.Contains('Task :app:prepareReMarketLocalApi') -and $warmLog.Contains('API connected:')) 'Task runs even when APK tasks are UP-TO-DATE'
        for ($round=1; $round -le 3; $round++) {
            $removed = Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'reverse','--remove','tcp:5080')
            if ($removed.code -ne 0) { throw 'Could not simulate missing API forwarding.' }
            $timer = [Diagnostics.Stopwatch]::StartNew(); $restored = $false
            while ($timer.Elapsed.TotalSeconds -lt 15) {
                $mapping = Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'reverse','--list')
                if ($mapping.text -match '(?m)^\S+\s+tcp:5080\s+tcp:5080\s*$') { $restored=$true; break }
                Start-Sleep -Milliseconds 250
            }
            Record-Check ('Lost forwarding recovers '+$round) $restored ([Math]::Round($timer.Elapsed.TotalSeconds,2))
        }
        if ($script:testSerial -match '_adb-tls-connect|:\d+$') {
            $kicked = Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'reconnect')
            Record-Check 'Wireless reconnect request accepted' ($kicked.code -eq 0) 'Only the selected phone transport; no adb kill-server'
            $timer = [Diagnostics.Stopwatch]::StartNew(); $restored=$false
            while ($timer.Elapsed.TotalSeconds -lt 30) {
                $available = Invoke-ReMarketAdb $adb @('devices')
                if ($available.text -notmatch ('(?m)^'+[regex]::Escape($script:testSerial)+'\s+device\b')) {
                    $candidates = @($available.text -split "`r?`n" | ForEach-Object { if ($_ -match '^(\S+)\s+device\b' -and $Matches[1] -notlike 'emulator-*') { $Matches[1] } })
                    foreach ($candidate in $candidates) {
                        $candidateId = (Invoke-ReMarketAdb $adb @('-s',$candidate,'shell','getprop','ro.serialno')).text.Trim()
                        if ($candidateId -eq $physicalId) { $script:testSerial=$candidate; break }
                    }
                }
                if ($available.text -match ('(?m)^'+[regex]::Escape($script:testSerial)+'\s+device\b')) {
                    $mapping = Invoke-ReMarketAdb $adb @('-s',$script:testSerial,'reverse','--list')
                    if ($mapping.text -match '(?m)^\S+\s+tcp:5080\s+tcp:5080\s*$') { $restored=$true; break }
                }
                Start-Sleep -Milliseconds 500
            }
            Record-Check 'Wireless reconnect restores API automatically' $restored ([Math]::Round($timer.Elapsed.TotalSeconds,2))
        }
        $crashedPid = Stop-VerifiedTestBackend
        $timer = [Diagnostics.Stopwatch]::StartNew(); $recovered=$false
        while ($timer.Elapsed.TotalSeconds -lt 60) {
            $current = Get-ReMarketState
            if ($current -and $current.coSoDuLieuSanSang) { $recovered=$true; break }
            Start-Sleep -Milliseconds 500
        }
        Record-Check 'Stopped Backend recovers automatically' $recovered ([Math]::Round($timer.Elapsed.TotalSeconds,2))
    }
    $phoneState = Invoke-PhoneHttp '/api/hethong/trangthai'
    Record-Check 'Physical phone HTTP -> Backend -> SQL' ($phoneState -match 'HTTP/1\.[01] 200' -and $phoneState -match '"coSoDuLieuSanSang":true') 'Real request through Wireless ADB port 5080'
    $phoneHome = Invoke-PhoneHttp '/api/tindang/trangchu'
    Record-Check 'Physical phone reads marketplace API' ($phoneHome -match 'HTTP/1\.[01] 200' -and $phoneHome -match '"noiBat"') 'Actual API data; no fake repository'
    if ($accessToken) {
        $report = Invoke-WebRequest -Uri ($script:RmUrl+'/api/admin/bao-cao') -Headers @{ Authorization=('Bearer '+$accessToken) } -UseBasicParsing
        Record-Check 'Admin JWT still authorized' ([int]$report.StatusCode -eq 200) 'Admin report API backed by SQL'
    }
} finally {
    # Keep the user's local host/phone ready, even if a deliberate recovery test failed.
    & (Join-Path $PSScriptRoot 'Start-ReMarketLocal.ps1') -AutoConnect
    if ($accessToken) {
        $null = Invoke-WebRequest -Method Post -Uri ($script:RmUrl+'/api/taikhoan/dangxuat') -Headers @{ Authorization=('Bearer '+$accessToken) } -UseBasicParsing
        try { $null=Invoke-WebRequest -Uri ($script:RmUrl+'/api/taikhoan/toi') -Headers @{ Authorization=('Bearer '+$accessToken) } -UseBasicParsing; $revoked=$false }
        catch { $revoked=[int]$_.Exception.Response.StatusCode -eq 401 }
        $checks.Add([pscustomobject]@{name='Test JWT revoked after logout'; passed=$revoked; detail='Only this test login was logged out'})
    }
    $result = [pscustomobject]@{ checkedUtc=[DateTime]::UtcNow.ToString('o'); recoveryExercised=[bool]$ExerciseRecovery; windowsRebootPerformed=$false; checks=@($checks.ToArray()) }
    $result | ConvertTo-Json -Depth 7 | Set-Content -LiteralPath (Join-Path $qaDirectory 'result.json') -Encoding UTF8
}
