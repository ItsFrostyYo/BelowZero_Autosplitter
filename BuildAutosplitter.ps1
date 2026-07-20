[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

# This script is intended to sit beside Livesplit.BelowZero.sln.
$Root = $PSScriptRoot

$SolutionPath = Join-Path $Root "Livesplit.BelowZero.sln"
$ProjectPath  = Join-Path $Root "Livesplit.BelowZero\LiveSplit.BelowZero.csproj"

$RepositoryComponentPath = Join-Path $Root "Components\LiveSplit.BelowZero.dll"
$LiveSplitComponentPath  = "C:\Users\mxz\Desktop\LiveSplit\LiveSplit_1.8.37\Components\LiveSplit.BelowZero.dll"

function Assert-FileExists {
    param(
        [Parameter(Mandatory)]
        [string]$Path,
        [Parameter(Mandatory)]
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "$Description was not found:`n$Path"
    }
}

function Find-MSBuild {
    $msbuildCommand = Get-Command "MSBuild.exe" -ErrorAction SilentlyContinue
    if ($msbuildCommand) {
        return $msbuildCommand.Source
    }

    $vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path -LiteralPath $vswhere -PathType Leaf) {
        $found = & $vswhere `
            -latest `
            -products * `
            -requires Microsoft.Component.MSBuild `
            -find "MSBuild\**\Bin\MSBuild.exe" |
            Select-Object -First 1

        if ($found) {
            return $found
        }
    }

    return $null
}

try {
    Assert-FileExists -Path $SolutionPath -Description "Solution"
    Assert-FileExists -Path $ProjectPath -Description "Project"

    # A running LiveSplit instance can keep the component DLL locked.
    $runningLiveSplit = Get-Process -Name "LiveSplit" -ErrorAction SilentlyContinue
    if ($runningLiveSplit) {
        throw "LiveSplit is currently running. Close LiveSplit, then run this script again."
    }

    Write-Host ""
    Write-Host "Building LiveSplit.BelowZero in Release mode..." -ForegroundColor Cyan
    Write-Host ""

    $msbuild = Find-MSBuild

    if ($msbuild) {
        Write-Host "Using MSBuild:" -ForegroundColor DarkGray
        Write-Host $msbuild -ForegroundColor DarkGray
        Write-Host ""

        & $msbuild `
            $SolutionPath `
            /restore `
            /t:Rebuild `
            /p:Configuration=Release `
            /m `
            /nologo

        if ($LASTEXITCODE -ne 0) {
            throw "MSBuild failed with exit code $LASTEXITCODE."
        }
    }
    else {
        $dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
        if (-not $dotnet) {
            throw @"
Neither MSBuild nor the .NET SDK was found.

Install Visual Studio Build Tools with the '.NET desktop build tools' workload,
or install the appropriate .NET SDK, then run this script again.
"@
        }

        Write-Host "MSBuild was not found directly; using dotnet build." -ForegroundColor Yellow
        Write-Host ""

        & $dotnet.Source build $SolutionPath --configuration Release

        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed with exit code $LASTEXITCODE."
        }
    }

    $ProjectDirectory = Split-Path -Parent $ProjectPath
    $BinDirectory = Join-Path $ProjectDirectory "bin"

    $BuiltDll = Get-ChildItem `
        -LiteralPath $BinDirectory `
        -Filter "LiveSplit.BelowZero.dll" `
        -File `
        -Recurse `
        -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match '[\\/]Release([^\\/]*)[\\/]' } |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1

    if (-not $BuiltDll) {
        throw "The build completed, but LiveSplit.BelowZero.dll was not found under:`n$BinDirectory"
    }

    Write-Host ""
    Write-Host "Built DLL:" -ForegroundColor Green
    Write-Host $BuiltDll.FullName
    Write-Host ""

    $Destinations = @(
        $RepositoryComponentPath,
        $LiveSplitComponentPath
    )

    foreach ($Destination in $Destinations) {
        $DestinationDirectory = Split-Path -Parent $Destination

        if (-not (Test-Path -LiteralPath $DestinationDirectory -PathType Container)) {
            New-Item -ItemType Directory -Path $DestinationDirectory -Force | Out-Null
        }

        Copy-Item `
            -LiteralPath $BuiltDll.FullName `
            -Destination $Destination `
            -Force

        Write-Host "Updated: $Destination" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "Release build and deployment completed successfully." -ForegroundColor Cyan
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "Build/deployment failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    exit 1
}
