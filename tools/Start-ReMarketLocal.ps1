param([string]$DeviceSerial, [switch]$BackendOnly)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$backendRoot = Join-Path $projectRoot 'Backend'
$apiRoot = Join-Path $backendRoot 'src/ReMarket.Api'
$runtimeRoot = Join-Path $backendRoot '.local/runtime'
. (Join-Path $backendRoot 'scripts/Use-DotNet8.ps1')

# Reuse an existing ReMarket host; never terminate another service on this port.
$running = $false
try {
    $state = Invoke-RestMethod 'http://127.0.0.1:5080/api/hethong/trangthai' -TimeoutSec 3
    $running = $state.tenUngDung -eq 'ReMarket'
} catch { }
if (-not $running) {
    if (Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue) {
        throw 'Port 5080 is occupied. Check the existing process before starting ReMarket.'
    }
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    & (Join-Path $backendRoot 'scripts/Initialize-Authentication.ps1')
    & dotnet build (Join-Path $apiRoot 'ReMarket.Api.csproj') --configuration Release --nologo --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw 'Backend build failed.' }
    $null = New-Item -ItemType Directory -Path $runtimeRoot -Force
    $dotnetPath = (Get-Command dotnet.exe).Source
    $serverProcess = Start-Process -FilePath $dotnetPath -WorkingDirectory $apiRoot `
        -ArgumentList @('bin/Release/net8.0/ReMarket.Api.dll','--urls','http://127.0.0.1:5080') `
        -WindowStyle Hidden -RedirectStandardOutput (Join-Path $runtimeRoot 'api.stdout.log') `
        -RedirectStandardError (Join-Path $runtimeRoot 'api.stderr.log') -PassThru
    [pscustomobject]@{ pid=$serverProcess.Id; startedUtc=[DateTime]::UtcNow.ToString('o'); url='http://127.0.0.1:5080' } |
        ConvertTo-Json | Set-Content (Join-Path $runtimeRoot 'api-process.json') -Encoding UTF8
}
$ready = $false
for ($attempt = 0; $attempt -lt 20; $attempt++) {
    try {
        $null = Invoke-WebRequest 'http://127.0.0.1:5080/health/ready' -UseBasicParsing -TimeoutSec 2
        $ready = $true; break
    } catch { Start-Sleep -Milliseconds 500 }
}
if (-not $ready) { throw "Backend/SQL is not ready. See $runtimeRoot and README SQL configuration." }
Write-Output 'Backend + SQL ready: http://127.0.0.1:5080/swagger'
if ($BackendOnly) { return }

$adbCommand = Get-Command adb.exe -ErrorAction SilentlyContinue
$adbPath = if ($adbCommand) { $adbCommand.Source } else { $null }
if (-not $adbPath) {
    $localProperties = Join-Path $projectRoot 'Android/local.properties'
    if (Test-Path -LiteralPath $localProperties) {
        $sdkLine = Get-Content -LiteralPath $localProperties | Where-Object { $_ -match '^sdk\.dir=' } | Select-Object -First 1
        if ($sdkLine) {
            $sdkPath = $sdkLine.Substring(8).Replace('\\','\').Replace('\:',':').Replace('\ ',' ')
            $candidate = Join-Path $sdkPath 'platform-tools/adb.exe'
            if (Test-Path -LiteralPath $candidate) { $adbPath = $candidate }
        }
    }
}
if (-not $adbPath) { throw 'Backend is ready. Add Android SDK platform-tools to PATH, then rerun to connect the phone.' }
$devices = @(& $adbPath devices | ForEach-Object { if ($_ -match '^(\S+)\s+device\s*$') { $Matches[1] } })
if ($DeviceSerial) { $devices = @($devices | Where-Object { $_ -eq $DeviceSerial }) }
if ($devices.Count -ne 1) { throw 'Connect one authorized device, or select it with -DeviceSerial SERIAL, then rerun. Backend remains running.' }
& $adbPath -s $devices[0] reverse tcp:5080 tcp:5080
if ($LASTEXITCODE -ne 0) { throw 'ADB reverse failed. Check the device connection.' }
Write-Output ('Connected device: '+$devices[0])
Write-Output 'Run the Android app with API URL http://127.0.0.1:5080/ (configured in Android/gradle.properties).'
Write-Output 'Rerun this script after ADB disconnect/reconnect. Starting Android Studio alone does not start the Backend.'
