$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Use-DotNet8.ps1')
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $existing = & dotnet user-secrets list --project src/ReMarket.Api
    if ($LASTEXITCODE -ne 0) { throw 'Cannot read user-secrets' }
    if (-not ($existing -match '^Jwt:SigningKey = ')) {
        $randomKey = New-Object byte[] 64
        $randomGenerator = [Security.Cryptography.RandomNumberGenerator]::Create()
        try { $randomGenerator.GetBytes($randomKey) } finally { $randomGenerator.Dispose() }
        $signingValue = [Convert]::ToBase64String($randomKey)
        & dotnet user-secrets set 'Jwt:SigningKey' $signingValue --project src/ReMarket.Api | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'Cannot persist JWT signing key' }
    }
    Write-Output 'JWT signing key configured in development user-secrets (value hidden).'
} finally { Pop-Location }
