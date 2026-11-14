param([switch]$SeedDemo)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Use-DotNet8.ps1')
& (Join-Path $PSScriptRoot 'Initialize-Authentication.ps1')
function Invoke-CheckedDotNet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed: $args" }
}
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    Invoke-CheckedDotNet tool restore
    Invoke-CheckedDotNet ef database update --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api
    if ($SeedDemo) {
        $previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
        try {
            $env:ASPNETCORE_ENVIRONMENT = 'Development'
            Invoke-CheckedDotNet run --project src/ReMarket.Api --no-launch-profile -- --seed-demo
        } finally { $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment }
    }
} finally { Pop-Location }
