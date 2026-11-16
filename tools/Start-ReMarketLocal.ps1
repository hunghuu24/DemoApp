param([string]$DeviceSerial, [switch]$BackendOnly, [switch]$AutoConnect)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReMarketLocal.Common.ps1')
if ($BackendOnly -or -not $AutoConnect) { Ensure-ReMarketBackend }
if ($BackendOnly) { Write-Output ('Backend + SQL ready: ' + $script:RmUrl + '/swagger'); return }
if ($AutoConnect) {
    $guard = New-Object Threading.Mutex($false, ($script:RmMutexName + '-WatcherStart'))
    $locked = $false
    try {
        try { $locked = $guard.WaitOne(15000) } catch [Threading.AbandonedMutexException] { $locked = $true }
        if (-not $locked) { throw 'Another reconnect worker is starting. Retry Run.' }
        $worker = Get-ReMarketWatcher
        if ($null -eq $worker) {
            $null = New-Item -ItemType Directory -Path $script:RmRuntimeRoot -Force
            $workerScript = Join-Path $script:RmProjectRoot 'tools/Watch-ReMarketLocal.ps1'
            $arguments = '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' + $workerScript + '"'
            # ShellExecute creates a separate hidden console. Redirected/inherited pipes
            # would keep Gradle's Exec task waiting for a long-running descendant.
            $worker = Start-Process -FilePath (Join-Path $PSHOME 'powershell.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru
            [pscustomobject]@{ pid=$worker.Id; startedUtc=$worker.StartTime.ToUniversalTime().ToString('o') } |
                ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:RmRuntimeRoot 'watcher-process.json') -Encoding UTF8
            $workerPid = $worker.Id
        } else { $workerPid = $worker.ProcessId }
        Write-Output ('Local API reconnect worker active (PID ' + $workerPid + ').')
    } finally {
        if ($locked) { $guard.ReleaseMutex() }
        $guard.Dispose()
    }
    $deadline = [DateTime]::UtcNow.AddSeconds(60)
    do {
        $state = Get-ReMarketState
        if ($state -and $state.coSoDuLieuSanSang) { break }
        Start-Sleep -Milliseconds 500
    } while ([DateTime]::UtcNow -lt $deadline)
    if (-not $state -or -not $state.coSoDuLieuSanSang) {
        throw 'Backend/SQL not ready. See Backend/.local/runtime/watcher-transcript.log and local-status.json.'
    }
}
Write-Output ('Backend + SQL ready: ' + $script:RmUrl + '/swagger')
$devices = @(Connect-ReMarketDevices (Get-ReMarketAdb) $DeviceSerial)
foreach ($device in $devices) {
    if ($device.apiForwarded) { Write-Output ('API connected: ' + $device.serial) }
    else { Write-Warning ('Device ' + $device.serial + ': ' + $device.state + '. Authorize/reconnect Wireless debugging in Android Studio.') }
}
if ($AutoConnect -and -not @($devices | Where-Object { $_.apiForwarded }).Count) {
    Write-Warning 'Waiting for an authorized phone/emulator. API port 5080 will be connected automatically when it appears.'
} elseif (-not $AutoConnect -and -not @($devices | Where-Object { $_.apiForwarded }).Count) {
    throw 'No API connection to an authorized device. Connect the phone or use -AutoConnect to wait in the background.'
}
