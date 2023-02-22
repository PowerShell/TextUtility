## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

Describe 'Compare-Text tests' {
    BeforeAll {
        $currentOutputRendering = $PSStyle.OutputRendering
        $PSStyle.OutputRendering = 'Ansi'
        $leftText = @("This is some", "example text.") -join [Environment]::NewLine
        $rightText = @("  This is other", "example text used!") -join [Environment]::NewLine
        $expectedInline = @(
            ""
            "``e[0;1;32m  ``e[0mThis is ``e[1;9;31msome``e[0m``e[0;1;32mother``e[0m"
            "example text``e[1;9;31m.``e[0m``e[0;1;32m used!``e[0m"
            "" 
        )
        $expectedSideBySide = @(
            ""
            "``e[0m1 | ``e[0mThis is ``e[1;9;31msome``e[0m``e[0m ``e[0m | ``e[0;1;32m  ``e[0mThis is ``e[0;1;32mother``e…``e[0m"
            "``e[0m2 | ``e[0mexample text``e[1;9;31m.``e[0m``e[0m | ``e[0mexample text``e[0;1;32m…``e[0m"
            ""
            ""
        )
        # this reset text was added in 7.3.0, we need to remove it from the output so the tests can pass on different ps versions.
        $inlineResetText = "``e[0;1;32m``e[0m``e[1;9;31m``e[0m``e[0;1;32m``e[0m"
        $sideBySideResetText = "``e[0m``e[0m``e[1;9;31m``e[0m``e[0m``e[0m``e[0;1;32m``e[0m``e[0;1;32m"
    }

    AfterAll {
        $PSStyle.OutputRendering = $currentOutputRendering
    }

    # These tests convert the emitted escape sequences to their literal representation
    # to ease debugging. Also, we use Out-String -Stream to get the output as a collection which Pester 4.10.1 can handle.
    It 'Compare with no specified view uses inline' {
        $out = Compare-Text -LeftText $leftText -RightText $rightText | Out-String -Stream | Foreach-Object {"$_".Replace("`e","``e").Replace($inlineResetText, "")}
        $out | Should -Be $expectedInline
    }

    It 'Compare with no specified view uses inline and positional parameters' {
        $out = Compare-Text $leftText $rightText | Out-String -Stream | Foreach-Object {"$_".Replace("`e","``e").Replace($inlineResetText, "")}
        $out | Should -Be $expectedInline
    }

    It 'Compare with inline works' {
        $out = Compare-Text $leftText $rightText -View Inline | Out-String -Stream | Foreach-Object {"$_".Replace("`e","``e").Replace($inlineResetText, "")}
        $out | Should -Be $expectedInline
    }

    It 'Compare with sideybyside works' {
        $out = Compare-Text $leftText $rightText -View SideBySide | Out-String -Stream | Foreach-Object {"$_".Replace("`e","``e").Replace($sideBySideResetText, "")}
        $out | Should -Be $expectedSideBySide
    }
}