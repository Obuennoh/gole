param(
    [Parameter(Mandatory)][string]$Version,
    [string]$Dotnet = 'dotnet',
    [string]$PublishDirectory = 'artifacts/publish',
    [string]$ReleaseDirectory = 'artifacts/releases'
)
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    & $Dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao restaurar o empacotador.' }
    & $Dotnet publish src/Gole.Desktop/Gole.Desktop.csproj -c Release -r win-x64 --self-contained true -p:UseAppHost=true "-p:Version=$Version" -p:DebugType=None -p:DebugSymbols=false -o $PublishDirectory
    if ($LASTEXITCODE -ne 0) { throw 'Falha no publish.' }
    & $Dotnet tool run vpk -- pack --packId GoleDesktop --packVersion $Version --packDir $PublishDirectory --outputDir $ReleaseDirectory --runtime win-x64 --channel win --mainExe Gole.Desktop.exe --packTitle Gole --packAuthors 'Lucas Bueno'
    if ($LASTEXITCODE -ne 0) { throw 'Falha ao empacotar.' }
} finally { Pop-Location }
