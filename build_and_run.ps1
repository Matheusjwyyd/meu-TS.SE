$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path $PSScriptRoot).Path
$packageRoot = Split-Path -Parent $repoRoot
$runtimeAssets = @("img", "lang", "libs")
$hasRuntimeAssets = $true
foreach ($assetName in $runtimeAssets) {
    if (-not (Test-Path -LiteralPath (Join-Path $packageRoot $assetName) -PathType Container)) {
        $hasRuntimeAssets = $false
        break
    }
}
if (-not $hasRuntimeAssets) {
    $packageRoot = Join-Path $repoRoot "dist"
}
$solutionPath = Join-Path $repoRoot "Meu Save Editor.sln"
$packagesConfigPath = Join-Path $repoRoot "TS SE Tool\packages.config"
$packagesPath = Join-Path $repoRoot "packages"
$compilerVersion = "4.14.0"
$compilerPackagePath = Join-Path $packagesPath "Microsoft.Net.Compilers.Toolset.$compilerVersion"
$compilerPath = Join-Path $compilerPackagePath "tasks\net472"
$toolsPath = Join-Path $repoRoot ".tools"
$nugetPath = Join-Path $toolsPath "nuget.exe"
$distPath = Join-Path $repoRoot "dist"
$buildOutputPath = Join-Path $repoRoot "TS SE Tool\bin\Release"

foreach ($assetName in $runtimeAssets) {
    $assetPath = Join-Path $packageRoot $assetName
    if (-not (Test-Path -LiteralPath $assetPath -PathType Container)) {
        throw "Pasta necessária não encontrada: $assetPath"
    }
}

if (-not (Test-Path -LiteralPath $solutionPath -PathType Leaf)) {
    throw "Solução não encontrada: $solutionPath"
}

if (-not (Test-Path -LiteralPath $nugetPath -PathType Leaf)) {
    $nugetOnPath = Get-Command nuget.exe -ErrorAction SilentlyContinue
    if ($nugetOnPath) {
        $nugetPath = $nugetOnPath.Source
    }
    else {
        New-Item -ItemType Directory -Path $toolsPath -Force | Out-Null
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetPath
    }
}

$msbuildCandidates = @(
    (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"),
    (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe")
)
$msbuildPath = $msbuildCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $msbuildPath) {
    throw "MSBuild do .NET Framework não foi encontrado. Instale Visual Studio Build Tools."
}

& $nugetPath restore $packagesConfigPath -PackagesDirectory $packagesPath -NonInteractive
if ($LASTEXITCODE -ne 0) {
    throw "Falha ao restaurar os pacotes NuGet."
}

if (-not (Test-Path -LiteralPath (Join-Path $compilerPath "csc.exe") -PathType Leaf)) {
    & $nugetPath install Microsoft.Net.Compilers.Toolset -Version $compilerVersion -OutputDirectory $packagesPath -NonInteractive
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao baixar o compilador Roslyn."
    }
}

$targetingPackPath = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2"
$buildArguments = @(
    $solutionPath,
    "/m",
    "/t:Build",
    "/p:Configuration=Release",
    "/p:CscToolPath=$compilerPath",
    "/p:CscToolExe=csc.exe",
    "/v:minimal"
)
if (-not (Test-Path -LiteralPath $targetingPackPath -PathType Container)) {
    & $nugetPath install Microsoft.NETFramework.ReferenceAssemblies.net472 -Version 1.0.3 -OutputDirectory $packagesPath -NonInteractive
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao baixar as referências de compilação do .NET Framework 4.7.2."
    }
    $referenceBuildRoot = Join-Path $packagesPath "Microsoft.NETFramework.ReferenceAssemblies.net472.1.0.3\build"
    $buildArguments += "/p:TargetFrameworkRootPath=$referenceBuildRoot"
}

& $msbuildPath @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "A compilação do Meu Save Editor falhou."
}

$appPath = Join-Path $buildOutputPath "MeuSaveEditor.exe"
if (-not (Test-Path -LiteralPath $appPath -PathType Leaf)) {
    throw "O executável compilado não foi encontrado: $appPath"
}

New-Item -ItemType Directory -Path $distPath -Force | Out-Null
& robocopy $buildOutputPath $distPath /E /XF *.pdb /R:1 /W:1
if ($LASTEXITCODE -ge 8) {
    throw "Falha ao copiar os arquivos compilados para dist."
}

foreach ($assetName in $runtimeAssets) {
    $sourcePath = Join-Path $packageRoot $assetName
    $destinationPath = Join-Path $distPath $assetName
    New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
    if ([IO.Path]::GetFullPath($sourcePath).TrimEnd('\') -ne [IO.Path]::GetFullPath($destinationPath).TrimEnd('\')) {
        Copy-Item -Path (Join-Path $sourcePath "*") -Destination $destinationPath -Recurse -Force
    }
}

$updaterOutputPath = Join-Path $buildOutputPath "updater"
$updaterDistPath = Join-Path $distPath "updater"
New-Item -ItemType Directory -Path $updaterDistPath -Force | Out-Null
if (Test-Path -LiteralPath $updaterOutputPath -PathType Container) {
    Copy-Item -Path (Join-Path $updaterOutputPath "*") -Destination $updaterDistPath -Recurse -Force
}

foreach ($fileName in @("README.md", "ANALISE_TECNICA.md", "LICENSE")) {
    Copy-Item -LiteralPath (Join-Path $repoRoot $fileName) -Destination $distPath -Force
}

foreach ($optionalFile in @(@("HowTo.pdf", "HowTo.pdf"), @("LICENSE.txt", "LICENSE-THIRD-PARTY.txt"))) {
    $sourcePath = Join-Path $packageRoot $optionalFile[0]
    $destinationPath = Join-Path $distPath $optionalFile[1]
    if ((Test-Path -LiteralPath $sourcePath -PathType Leaf) -and ([IO.Path]::GetFullPath($sourcePath) -ne [IO.Path]::GetFullPath($destinationPath))) {
        Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
    }
}

Get-ChildItem -LiteralPath $distPath -Recurse -File -Filter "*.pdb" | ForEach-Object {
    Remove-Item -LiteralPath $_.FullName -Force
}

Write-Host "Compilação concluída: $(Join-Path $distPath 'MeuSaveEditor.exe')"
