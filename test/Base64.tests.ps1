## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

Describe 'Base64 cmdlet tests' {
    BeforeAll {
        $testString = 'Hello World!'
        $testBase64 = 'SGVsbG8gV29ybGQh'
        $longString = $testString * 10
        $longBase64 = "SGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8gV29y`r`nbGQhSGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8gV29ybGQhSGVsbG8g`r`nV29ybGQh"
    }

    It 'ConvertTo-Base64 will accept text input from parameter' {
        ConvertTo-Base64 -Text $testString | Should -BeExactly $testBase64
    }

    It 'ConvertTo-Base64 will accept text input from positional parameter' {
        ConvertTo-Base64 $testString | Should -BeExactly $testBase64
    }

    It 'ConvertTo-Base64 will accept text input from pipeline' {
        $testString | ConvertTo-Base64 | Should -BeExactly $testBase64
    }

    It 'ConvertTo-Base64 will accept text and insert breaklines' {
        $longString | ConvertTo-Base64 -InsertBreakLines | Should -BeExactly $longBase64
    }

    It 'ConvertTo-Base64 will accept byte array' {
        [System.Text.Encoding]::Utf8.GetBytes($testString) | ConvertTo-Base64 | Should -BeExactly $testBase64
    }

    It 'ConvertTo-Base64 will accept byte array and insert break lines' {
        [System.Text.Encoding]::Utf8.GetBytes($longString) | ConvertTo-Base64 -InsertBreakLines | Should -BeExactly $longBase64
    }

    It 'ConvertFrom-Base64 will accept encoded input from parameter' {
        ConvertFrom-Base64 -EncodedText $testBase64 | Should -BeExactly $testString
    }

    It 'ConvertFrom-Base64 will accept encoded input from positional parameter' {
        ConvertFrom-Base64 $testBase64 | Should -BeExactly $testString
    }

    It 'ConvertFrom-Base64 will accept encoded input form pipeline' {
        $testBase64 | ConvertFrom-Base64 | Should -BeExactly $testString
    }

    It 'ConvertFrom-Base64 -AsByteArray returns byte array' {
        ($testBase64 | ConvertFrom-Base64 -AsByteArray) | Should -BeExactly ([System.Text.Encoding]::Utf8.GetBytes($testString))
    }

    It 'ConvertFrom-Base64 will accept text with breaks' {
        $longBase64 | ConvertFrom-Base64 | Should -Be $longString
    }
}
