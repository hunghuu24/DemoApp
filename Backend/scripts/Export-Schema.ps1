$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Use-DotNet8.ps1')
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $outputPath = [IO.Path]::GetFullPath((Join-Path (Get-Location) '../docs/database/remarket-schema.sql'))
    & dotnet ef migrations script --idempotent --project src/ReMarket.Infrastructure --startup-project src/ReMarket.Api --output $outputPath
    if ($LASTEXITCODE -ne 0) { throw 'Khong xuat duoc migration SQL.' }
    $sessionSettings = @'
-- Required for persisted computed columns and filtered indexes, including sqlcmd sessions.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

'@
    [IO.File]::WriteAllText($outputPath, $sessionSettings + [IO.File]::ReadAllText($outputPath), (New-Object Text.UTF8Encoding($true)))
} finally { Pop-Location }
