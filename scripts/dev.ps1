param(
    [string]$Dotnet = 'dotnet',
    [switch]$Isolated
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repo 'src/Gole.Desktop/Gole.Desktop.csproj'
$runArgs = @('run', '--project', $project, '--configuration', 'Debug')
if ($Isolated) {
    $runArgs += @('--', '--development-data', (Join-Path $repo '.local/dev-state'))
}
& $Dotnet @runArgs
exit $LASTEXITCODE
