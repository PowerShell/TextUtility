## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

[CmdletBinding(DefaultParameterSetName="build")]
param (
    [Parameter(ParameterSetName="build")]
    [string]
    $Configuration = "Debug",

    [Parameter(ParameterSetName="package")]
    [switch]
    $Package,

    [Parameter(ParameterSetName="package")]
    [switch]
    $signed,

    [Parameter(ParameterSetName="test")]
    [switch]
    $test,

    [Parameter()]
    [switch]
    $Clean
)

$moduleName = "Microsoft.PowerShell.TextUtility"
$repoRoot = git rev-parse --show-toplevel


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
        Register-PSRepository -Name $repoName -SourceLocation ${repoRoot} -InstallationPolicy Trusted
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
    $nupkgPath = Join-Path $repoRoot $nupkgName
    if ($env:TF_BUILD) {
        # In Azure DevOps
        Write-Host "##vso[artifact.upload containerfolder=$nupkgName;artifactname=$nupkgName;]$nupkgPath"
    }
    else {
        Write-Verbose -Verbose "package path: ${nupkgPath} (exists:$(Test-Path $nupkgPath))"
    }
}

try {
    Push-Location "$PSScriptRoot/src/code"

    $outPath = "$PSScriptRoot/out/${moduleName}"
    $script:moduleInfo = Get-ModuleInfo

    if ($Clean) {
        if (Test-Path $outPath) {
            Write-Verbose "Deleting $outPath"
            Remove-Item -recurse -force -path $outPath
        }

        dotnet clean
    }

    dotnet publish --output $outPath --configuration $Configuration

    if ($Test) {
        $script = [ScriptBlock]::Create("
            try {
                Import-Module '${repoRoot}/out/${moduleName}/'
                Import-Module -Name Pester -Max 4.99
                Push-Location '${repoRoot}/test'
                Invoke-Pester
            }
            finally {
                Pop-Location
            }")
        pwsh -c $script
    }

    if ($Package) {
        if ($Signed) {
            $pkgBase = "${PSScriptRoot}/signed/${moduleName}"
        }
        else {
            $pkgBase = "${PSScriptRoot}/out/${moduleName}"
        }
        Export-Module -packageRoot $pkgBase
    }
}
finally {
    Pop-Location
}
