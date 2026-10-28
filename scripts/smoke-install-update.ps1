param([Parameter(Mandatory)][string]$Version)
$ErrorActionPreference = 'Stop'
if ($env:GITHUB_ACTIONS -ne 'true') { throw 'Este teste de instalação é exclusivo do runner descartável do CI.' }
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    $setup = @(Get-ChildItem 'artifacts/releases' -Filter '*Setup.exe')
    if ($setup.Count -ne 1) { throw 'Instalador não identificado.' }
    $installDir = Join-Path $env:RUNNER_TEMP ('Gole.InstallSmoke.' + [Guid]::NewGuid().ToString('N'))
    $report = Join-Path $env:RUNNER_TEMP 'gole-installed.json'
    $preferencesDir = Join-Path $env:LOCALAPPDATA 'Gole'
    New-Item -ItemType Directory -Force -Path $preferencesDir | Out-Null
    $preferences = Join-Path $preferencesDir 'preferences.json'
    '{"Version":3,"Name":"Teste de preservação","Accessories":[3,15,21],"Goal":2500}' | Set-Content -LiteralPath $preferences -Encoding utf8
    $preferencesHash = (Get-FileHash -LiteralPath $preferences).Hash
    $install = Start-Process -FilePath $setup[0].FullName -ArgumentList '--silent', '--installto', ('"' + $installDir + '"') -WindowStyle Hidden -Wait -PassThru
    if ($install.ExitCode -ne 0) { throw "Falha na instalação: $($install.ExitCode)" }
    $exe = Join-Path $installDir 'current/Gole.Desktop.exe'
    "GOLE_SMOKE_EXE=$exe" >> $env:GITHUB_ENV
    for ($attempt = 0; $attempt -lt 30 -and !(Test-Path -LiteralPath $exe); $attempt++) { Start-Sleep -Seconds 1 }
    if (!(Test-Path -LiteralPath $exe)) {
        if (Test-Path -LiteralPath $installDir) { Get-ChildItem -LiteralPath $installDir | Select-Object Name,Length }
        throw 'O executável principal não foi instalado na pasta current.'
    }
    $probe = Start-Process -FilePath $exe -ArgumentList '--smoke-status', ('"' + $report + '"') -WindowStyle Hidden -Wait -PassThru
    if ($probe.ExitCode -ne 0) { throw 'A instalação não foi reconhecida pelo atualizador.' }
    $installed = Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
    if (!$installed.Installed -or $installed.Version -ne $Version) { throw 'Versão instalada incorreta.' }
    $parts = [Version]::Parse($Version)
    $nextVersion = "$($parts.Major).$($parts.Minor).$($parts.Build + 1)-smoke"
    New-Item -ItemType Directory -Force -Path 'artifacts/smoke-feed' | Out-Null
    Copy-Item -Path 'artifacts/releases/*' -Destination 'artifacts/smoke-feed'
    & ./scripts/package.ps1 -Version $nextVersion -PublishDirectory 'artifacts/smoke-publish' -ReleaseDirectory 'artifacts/smoke-feed'
    $updateReport = Join-Path $env:RUNNER_TEMP 'gole-update-target.txt'
    $feed = (Resolve-Path 'artifacts/smoke-feed').Path
    $upgrade = Start-Process -FilePath $exe -ArgumentList '--smoke-update', ('"' + $feed + '"'), ('"' + $updateReport + '"') -WindowStyle Hidden -Wait -PassThru
    if ($upgrade.ExitCode -ne 0) { throw "Falha ao preparar atualização: $($upgrade.ExitCode)" }
    $updated = $false
    for ($attempt = 0; $attempt -lt 15; $attempt++) {
        Start-Sleep -Seconds 2
        $probe = Start-Process -FilePath $exe -ArgumentList '--smoke-status', ('"' + $report + '"') -WindowStyle Hidden -Wait -PassThru
        if ($probe.ExitCode -eq 0) {
            $installed = Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
            if ($installed.Version -eq $nextVersion) { $updated = $true; break }
        }
    }
    if (!$updated) { throw 'A atualização instalada não atingiu a versão esperada.' }
    if ((Get-FileHash -LiteralPath $preferences).Hash -ne $preferencesHash) { throw 'As preferências foram alteradas pela instalação/atualização.' }
    Write-Output "Instalação $Version e atualização $nextVersion verificadas sem janelas; preferências preservadas."
} finally { Pop-Location }
