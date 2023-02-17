# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

@{
    RootModule = '.\Microsoft.PowerShell.TextUtility.dll'
    ModuleVersion = '0.4.0'
    CompatiblePSEditions = @('Desktop', 'Core')
    GUID = '5cb64356-cd04-4a18-90a4-fa4072126155'
    Author = 'Microsoft Corporation'
    CompanyName = 'Microsoft Corporation'
    Copyright = '(c) Microsoft Corporation. All rights reserved.'
    Description = "This module contains cmdlets to help with manipulating or reading text."
    PowerShellVersion = '5.1'
    FormatsToProcess = @('Microsoft.PowerShell.TextUtility.format.ps1xml')
    CmdletsToExport = @(
        'Compare-Text','ConvertFrom-Base64','ConvertTo-Base64',"Convert-TextTable"
    )
    PrivateData = @{
        PSData = @{
            LicenseUri = 'https://github.com/PowerShell/TextUtility/blob/main/LICENSE'
            ProjectUri = 'https://github.com/PowerShell/TextUtility'
            ReleaseNotes = 'Initial release'
            Prerelease = 'Preview2'
        }
    }

    # HelpInfoURI = ''
}
