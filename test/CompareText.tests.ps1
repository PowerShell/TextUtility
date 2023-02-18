## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

Describe 'Compare-Test tests' {
    BeforeAll {
        $currentOutputRendering = $PSStyle.OutputRendering
        $PSStyle.OutputRendering = 'Ansi'
        $leftText = @("This is some", "example text.") -join [Environment]::NewLine
        $rightText = @("  This is other", "example text used!") -join [Environment]::NewLine
        $expectedInline = @(
            ""
            "`e[0;1;32m  `e[0mThis is `e[1;9;31msome`e[0m`e[0;1;32mother`e[0m"
            # "`e[0;1;32m`e[0m`e[1;9;31m`e[0m`e[0;1;32m`e[0mexample text`e[1;9;31m.`e[0m`e[0;1;32m used!`e[0m"
            "example text`e[1;9;31m.`e[0m`e[0;1;32m used!`e[0m"
            "" 
            "" # we need one extra because join doesn't add a newline at the end
        ) -join [environment]::NewLine
        $expectedSideBySide = @(
            ""
            "`e[0m1 | `e[0mThis is `e[1;9;31msome`e[0m`e[0m `e[0m | `e[0;1;32m  `e[0mThis is `e[0;1;32mother`e…`e[0m"
            # "`e[0m`e[0m`e[1;9;31m`e[0m`e[0m`e[0m`e[0;1;32m`e[0m`e[0;1;32m`e[0m2 | `e[0mexample text`e[1;9;31m.`e[0m`e[0m | `e[0mexample text`e[0;1;32m…`e[0m"
            "`e[0m2 | `e[0mexample text`e[1;9;31m.`e[0m`e[0m | `e[0mexample text`e[0;1;32m…`e[0m"
            ""
            ""
            "" # we need one extra because join doesn't add a newline at the end
        ) -join [environment]::NewLine
    }

    AfterAll {
        $PSStyle.OutputRendering = $currentOutputRendering
    }

    It 'Compare with no specified view uses inline' {
        $out = Compare-Text -LeftText $leftText -RightText $rightText | Out-String
        $out | Should -BeExactly $expectedInline -Because "$($out.Replace(""`e"",""``e"")) -ne $($expectedInline.Replace(""`e"",""``e"")))"
    }

    It 'Compare with no specified view uses inline and positional parameters' {
        $out = Compare-Text $leftText $rightText | Out-String
        $out | Should -BeExactly $expectedInline -Because "$($out.Replace(""`e"",""``e"")) -ne $($expectedInline.Replace(""`e"",""``e"")))"
    }

    It 'Compare with inline works' {
        $out = Compare-Text $leftText $rightText -View Inline | Out-String
        $out | Should -BeExactly $expectedInline -Because "$($out.Replace(""`e"",""``e"")) -ne $($expectedInline.Replace(""`e"",""``e"")))"
    }

    It 'Compare with sideybyside works' {
        $out = Compare-Text $leftText $rightText -View SideBySide | Out-String
        $out | Should -BeExactly $expectedSideBySide -Because "$($out.Replace(""`e"",""``e"")) -ne $($expectedSideBySide.Replace(""`e"",""``e"")))"
    }
}