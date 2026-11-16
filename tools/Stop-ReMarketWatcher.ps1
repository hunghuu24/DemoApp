$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ReMarketLocal.Common.ps1')
$worker = Get-ReMarketWatcher
if ($worker) {
    Stop-Process -Id $worker.ProcessId
    Write-Output 'Stopped this workspace reconnect worker. Backend remains running.'
} else { Write-Output 'This workspace reconnect worker is not running.' }
