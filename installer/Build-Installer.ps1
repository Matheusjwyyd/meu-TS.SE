param(
    [switch]$SkipAppBuild
)

$ErrorActionPreference = "Stop"
$repoRoot = (Resolve-Path $PSScriptRoot).Path | Split-Path -Parent
$distPath = Join-Path $repoRoot "dist"
$stagePath = Join-Path $env:TEMP ("MeuSaveEditor-release-" + [Guid]::NewGuid().ToString("N"))
$appStagePath = Join-Path $stagePath "payload"
$payloadZip = Join-Path $stagePath "Payload.zip"
$version = "0.3.13.1"

if (-not $SkipAppBuild) {
    & (Join-Path $repoRoot "build_and_run.ps1")
}

if (-not (Test-Path -LiteralPath (Join-Path $distPath "MeuSaveEditor.exe") -PathType Leaf)) {
    throw "dist\MeuSaveEditor.exe não foi encontrado. Compile primeiro o aplicativo."
}

$compilerCandidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)
$compilerPath = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $compilerPath) {
    throw "O compilador C# do .NET Framework não foi encontrado."
}

try {
    New-Item -ItemType Directory -Path $appStagePath -Force | Out-Null

    foreach ($fileName in @("MeuSaveEditor.exe", "MeuSaveEditor.exe.config")) {
        $source = Join-Path $distPath $fileName
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
            throw "Arquivo necessário não encontrado: $source"
        }
        Copy-Item -LiteralPath $source -Destination $appStagePath -Force
    }

    Get-ChildItem -LiteralPath $distPath -File -Filter "*.dll" | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $appStagePath -Force
    }

    foreach ($directoryName in @("amd64", "dbs", "gameref", "img", "lang", "libs", "updater", "x86")) {
        $source = Join-Path $distPath $directoryName
        if (-not (Test-Path -LiteralPath $source -PathType Container)) {
            throw "Pasta necessária não encontrada: $source"
        }
        Copy-Item -LiteralPath $source -Destination $appStagePath -Recurse -Force
    }

    foreach ($fileName in @("README.md", "ANALISE_TECNICA.md", "LICENSE", "LICENSE-THIRD-PARTY.txt")) {
        $source = Join-Path $repoRoot $fileName
        if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
            $source = Join-Path $distPath $fileName
        }
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination $appStagePath -Force
        }
    }
    Copy-Item -LiteralPath (Join-Path $repoRoot "TUTORIAL_INSTALACAO.md") -Destination $appStagePath -Force

    $uninstaller = Join-Path $appStagePath "Uninstall.exe"
    $uninstallerArgs = @(
        "/nologo", "/target:winexe", "/platform:anycpu", "/optimize+",
        "/out:$uninstaller", "/reference:System.Windows.Forms.dll",
        "/reference:System.dll", (Join-Path $PSScriptRoot "Uninstaller.cs")
    )
    & $compilerPath @uninstallerArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Não foi possível compilar o desinstalador."
    }

    Compress-Archive -Path (Join-Path $appStagePath "*") -DestinationPath $payloadZip -CompressionLevel Optimal -Force
    New-Item -ItemType Directory -Path $distPath -Force | Out-Null

    $setupPath = Join-Path $distPath "MeuSaveEditor-Setup.exe"
    $installerArgs = @(
        "/nologo", "/target:winexe", "/platform:anycpu", "/optimize+",
        "/out:$setupPath", "/reference:System.Windows.Forms.dll",
        "/reference:System.Drawing.dll", "/reference:System.dll",
        "/reference:System.Core.dll", "/reference:System.IO.Compression.dll",
        "/reference:System.IO.Compression.FileSystem.dll",
        "/resource:$payloadZip,Payload.zip", (Join-Path $PSScriptRoot "Installer.cs")
    )
    & $compilerPath @installerArgs
    if ($LASTEXITCODE -ne 0) {
        throw "Não foi possível compilar o instalador."
    }

    $portablePath = Join-Path $distPath "MeuSaveEditor-Portable.zip"
    Copy-Item -LiteralPath $payloadZip -Destination $portablePath -Force
    Copy-Item -LiteralPath (Join-Path $repoRoot "TUTORIAL_INSTALACAO.md") -Destination $distPath -Force

    $checksumsPath = Join-Path $distPath "SHA256SUMS.txt"
    $assets = @($setupPath, $portablePath, (Join-Path $distPath "TUTORIAL_INSTALACAO.md"))
    $checksumLines = foreach ($asset in $assets) {
        $hash = (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant()
        "$hash  $(Split-Path -Leaf $asset)"
    }
    Set-Content -LiteralPath $checksumsPath -Value $checksumLines -Encoding Ascii

    Write-Host "Pacote de release criado:"
    Get-Item -LiteralPath $setupPath, $portablePath, (Join-Path $distPath "TUTORIAL_INSTALACAO.md"), $checksumsPath |
        Select-Object FullName, Length, LastWriteTime
}
finally {
    $resolvedTemp = [IO.Path]::GetFullPath($env:TEMP).TrimEnd('\') + '\'
    $resolvedStage = [IO.Path]::GetFullPath($stagePath)
    if ($resolvedStage.StartsWith($resolvedTemp, [StringComparison]::OrdinalIgnoreCase) -and
        (Test-Path -LiteralPath $resolvedStage -PathType Container)) {
        Remove-Item -LiteralPath $resolvedStage -Recurse -Force
    }
}
