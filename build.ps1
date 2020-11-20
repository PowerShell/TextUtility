# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

[CmdletBinding()]
param(
    [switch] $Clean = $false
)

Push-Location $PSScriptRoot/src/code
if ($Clean) {
    Remove-Item -Recurse -Path ./bin -Force -ErrorAction SilentlyContinue
    Remove-Item -Recurse -Path ./obj -Force -ErrorAction SilentlyContinue
}

dotnet build
Pop-Location