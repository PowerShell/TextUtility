## Copyright (c) Microsoft Corporation.
## Licensed under the MIT License.

Describe 'Compare-Test tests' {
    BeforeAll {
        $leftText = @"
This is some
example text.
"@
        $rightText = @"
  This is other
example text used!
"@

        $expectedInline = @"

[0;1;32m  [0mThis is [1;9;31msome[0m[0;1;32mother[0m
example text[1;9;31m.[0m[0;1;32m used![0m


"@

        $expectedSideBySide = @"

[0m1 | [0mThis is [1;9;31msome[0m[0m [0m | [0;1;32m  [0mThis is [0;1;32mother…
[0m2 | [0mexample text[1;9;31m.[0m[0m | [0mexample text[0;1;32m…
[0m


"@

    }

    It 'Compare with no specified view uses inline' {
        $out = Compare-Text -LeftText $leftText -RightText $rightText | Out-String
        $out | Should -BeExactly $expectedInline
    }

    It 'Compare with no specified view uses inline and positional parameters' {
        $out = Compare-Text $leftText $rightText | Out-String
        $out | Should -BeExactly $expectedInline
    }

    It 'Compare with inline works' {
        $out = Compare-Text $leftText $rightText -View Inline | Out-String
        $out | Should -BeExactly $expectedInline
    }

    It 'Compare with sideybyside works' {
        $out = Compare-Text $leftText $rightText -View SideBySide | Out-String
        write-verbose -verbose "Width: $([Console]::WindowWidth)"
        $out | Should -BeExactly $expectedSideBySide
    }
}