param([string]$Dotnet = 'dotnet')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$solution = Join-Path $repo 'Gole.sln'
& $Dotnet restore $solution
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Dotnet build $solution --configuration Debug --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Dotnet test $solution --configuration Debug --no-build --logger 'console;verbosity=normal'
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& $Dotnet format whitespace $solution --no-restore --verify-no-changes
exit $LASTEXITCODE
