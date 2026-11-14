param([string]$Server = 'localhost')
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Use-DotNet8.ps1')
Push-Location (Split-Path $PSScriptRoot -Parent)
$previousConnection = $env:ConnectionStrings__ReMarket
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$testDbName = 'ReMarketTest_' + [Guid]::NewGuid().ToString('N')
if ($testDbName -notmatch '^ReMarketTest_[a-f0-9]{32}$') { throw 'Unexpected database name' }
function Invoke-CheckedNative {
    param([string]$Executable, [string[]]$NativeArgs)
    & $Executable @NativeArgs
    if ($LASTEXITCODE -ne 0) { throw "Command failed: $Executable" }
}
$created = $false
try {
    Invoke-CheckedNative sqlcmd @('-S',$Server,'-E','-C','-b','-Q',"CREATE DATABASE [$testDbName];")
    $created = $true
    $env:ConnectionStrings__ReMarket = "Server=$Server;Database=$testDbName;Trusted_Connection=True;TrustServerCertificate=True"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $script = [IO.Path]::GetFullPath((Join-Path (Get-Location) '../docs/database/remarket-schema.sql'))
    # Apply the exported deployment script twice to a brand new, isolated DB.
    Invoke-CheckedNative sqlcmd @('-S',$Server,'-E','-C','-b','-d',$testDbName,'-i',$script)
    Invoke-CheckedNative sqlcmd @('-S',$Server,'-E','-C','-b','-d',$testDbName,'-i',$script)
    Invoke-CheckedNative dotnet @('run','--project','src/ReMarket.Api','--no-launch-profile','--','--seed-demo')
    Invoke-CheckedNative dotnet @('run','--project','tests/ReMarket.DatabaseChecks')
} finally {
    # Only this run's generated temporary DB can be removed. ReMarketDemo is untouched.
    if ($created -and $testDbName -match '^ReMarketTest_[a-f0-9]{32}$') {
        & sqlcmd -S $Server -E -C -b -Q "ALTER DATABASE [$testDbName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$testDbName];"
        if ($LASTEXITCODE -ne 0) { Write-Warning "Khong don duoc database tam $testDbName" }
    }
    $env:ConnectionStrings__ReMarket = $previousConnection
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
    Pop-Location
}
