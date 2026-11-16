$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReMarketLocal.Common.ps1')
$guard = New-Object Threading.Mutex($false, ($script:RmMutexName + '-Watcher'))
$locked = $false
try {
    try { $locked = $guard.WaitOne(0) } catch [Threading.AbandonedMutexException] { $locked = $true }
    if (-not $locked) { exit 0 }
    $null = Start-Transcript -Path (Join-Path $script:RmRuntimeRoot 'watcher-transcript.log') -Append
    $lastSummary = ''
    while ($true) {
        $devices = @(); $failure = $null; $ready = $false
        try {
            Ensure-ReMarketBackend
            $ready = $true
            $devices = @(Connect-ReMarketDevices (Get-ReMarketAdb))
        } catch { $failure = $_.Exception.Message }
        $status = [pscustomobject]@{ watcherPid=$PID; heartbeatUtc=[DateTime]::UtcNow.ToString('o'); url=$script:RmUrl; backendSqlReady=$ready; devices=$devices; error=$failure }
        $status | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $script:RmRuntimeRoot 'local-status.json') -Encoding UTF8
        $summary = ([ordered]@{ ready=$ready; devices=$devices; error=$failure } | ConvertTo-Json -Depth 5 -Compress)
        if ($summary -ne $lastSummary) { Write-Output ([DateTime]::UtcNow.ToString('o') + ' ' + $summary); $lastSummary = $summary }
        Start-Sleep -Seconds 3
    }
} finally {
    if ($locked) { $guard.ReleaseMutex() }
    if ($locked) { $null = Stop-Transcript }
    $guard.Dispose()
}
