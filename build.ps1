## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

[CmdletBinding(DefaultParameterSetName="build")]
param (
    [Parameter(ParameterSetName="build")]
    [string]
    $Configuration = "Debug",

    [Parameter(ParameterSetName="package")]
    [switch]
    $NoBuild,

    [Parameter(ParameterSetName="package")]
    [switch]
    $Package,

    [Parameter(ParameterSetName="package")]
    [switch]
    $signed,

    [Parameter(ParameterSetName="package")]
    [Parameter(ParameterSetName="test")]
    [switch]
    $test,

    [Parameter()]
    [switch]
    $Clean,

    [Parameter()]
    [switch]
    $GetPackageVersion,

    [Parameter(ParameterSetName="bootstrap")]
    [switch]
    $Bootstrap

)


$moduleFileManifest = @(
    @{ Sign = $true ; File = "Microsoft.PowerShell.TextUtility.format.ps1xml" }
    @{ Sign = $true ; File = "Microsoft.PowerShell.TextUtility.psd1" }
    @{ Sign = $false; File = "dictionary.txt" }
    @{ Sign = $true ; File = "Microsoft.PowerShell.TextUtility.dll" }
)

$moduleName = "Microsoft.PowerShell.TextUtility"
$repoRoot = git rev-parse --show-toplevel
$testRoot = "${repoRoot}/test"

#
function Get-ModuleInfo {
    import-powershelldatafile "$repoRoot/src/${moduleName}.psd1"
}

# this takes the files for the module and publishes them to a created, local repository
# so the nupkg can be used to publish to the PSGallery
function Export-Module
{
    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSAvoidUsingWriteHost", "")]
    param($packageRoot)

    if ( -not (test-path $packageRoot)) {
        throw "'$packageRoot' does not exist"
    }
    # now construct a nupkg by registering a local repository and calling publish module
    $repoName = [guid]::newGuid().ToString("N")
    try {
        Register-PSRepository -Name $repoName -SourceLocation "${repoRoot}/out" -InstallationPolicy Trusted
        Publish-Module -Path $packageRoot -Repository $repoName
    }
    catch {
        throw $_
    }
    finally {
        if (Get-PackageSource -Name $repoName) {
            Unregister-PSRepository -Name $repoName
        }
    }
    Get-ChildItem -Recurse -Name $packageRoot | Write-Verbose -Verbose

    # construct the package path and publish it
    $nupkgName = "{0}.{1}" -f $moduleName,$moduleInfo.ModuleVersion
    $pre = $moduleInfo.PrivateData.PSData.Prerelease
    if ($pre) { $nupkgName += "-${pre}" }
    $nupkgName += ".nupkg"
    $nupkgPath = Join-Path $repoRoot out $nupkgName
    Write-Verbose -Verbose "package path: ${nupkgPath} (exists:$(Test-Path $nupkgPath))"
}

function Test-Module {
    try {
        $PSVersionTable | Out-String -Stream | Write-Verbose -Verbose
        $importTarget = "Import-Module ${PSScriptRoot}/out/${ModuleName}"
        $importPester = "Import-Module Pester -Max 4.10.1"
        $invokePester = "Invoke-Pester -OutputFormat NUnitXml -EnableExit -OutputFile ../testResults.xml"
        $sb = [scriptblock]::Create("${importTarget}; ${importPester}; ${invokePester}")
        Push-Location $testRoot
        # we support Windows PowerShell too so we need to calculate the shell to run rather than hardcoding it.
        $PSEXE = (Get-Process -id $PID).MainModule.FileName
        & $PSEXE -noprofile -command $sb
    }
    finally {
        Pop-Location
    }
}

function Invoke-Bootstrap
{
    $neededPesterModule = Get-Module -Name Pester -ListAvailable | Where-Object { $_.Version -eq "4.10.1" }
    $neededPesterVersion = [version]"4.10.1"
    if ($neededPesterModule.Version -eq $neededPesterVersion)
    {
        Write-Verbose -Verbose -Message "Required pester version $neededPesterVersion is available."
        return
    }

    Write-Verbose -Verbose -Message "Attempting install of Pester version ${neededPesterVersion}."
    Install-Module -Name Pester -Scope CurrentUser -RequiredVersion 4.10.1 -Force -SkipPublisherCheck
    $neededPesterModule = Get-Module -Name Pester -ListAvailable | Where-Object { $_.Version -eq $neededPesterVersion }
    if ($neededPesterModule.Version -ne $neededPesterVersion)
    {
        throw "Pester install failed"
    }

    Write-Verbose -Verbose -Message "Pester version $neededPesterVersion installed."
    return
}


try {
    Push-Location "$PSScriptRoot/src/code"
    $script:moduleInfo = Get-ModuleInfo
    if ($GetPackageVersion) {
        return $moduleInfo.ModuleVersion
    }

    if ($Bootstrap) {
        Invoke-Bootstrap
        return
    }

    $outPath = "$PSScriptRoot/out/${moduleName}"
    if ($Clean) {
        if (Test-Path $outPath) {
            Write-Verbose "Deleting $outPath"
            Remove-Item -recurse -force -path $outPath
        }

        dotnet clean
    }

    if (-not $NoBuild) {
        dotnet publish --verbosity d --output $outPath --configuration $Configuration
        Remove-Item -Path "$outPath/Microsoft.PowerShell.TextUtility.deps.json" -ErrorAction SilentlyContinue
        if ($Configuration -eq "Release") {
            Remove-Item -Path "$outPath/Microsoft.PowerShell.TextUtility.pdb" -ErrorAction SilentlyContinue
        }
    }

    if ($Test) {
        Test-Module
        return
    }

    if ($Package) {
        if ($Signed) {
            $pkgBase = "${PSScriptRoot}/signed/${moduleName}"
        }
        else {
            $pkgBase = "${PSScriptRoot}/out/${moduleName}"
        }

        if (-not (Test-Path $pkgBase)) {
            throw "Directory '$pkgBase' does not exist"
        }

        Export-Module -packageRoot $pkgBase
    }
}
finally {
    Pop-Location
}
