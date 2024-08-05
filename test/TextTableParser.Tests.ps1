Import-Module -Verbose $PSScriptRoot/../out/Microsoft.PowerShell.TextUtility/Microsoft.PowerShell.TextUtility.psd1 -Force
$testCases = @{
    FileName    = 'attrib.01.txt'
    convertArgs = @{ NoHeader = $true }
    Rows        = 12
    Results     = @{Row = 0; Property_01 = 'A'; Property_02 = 'C:\windows\system32\mmgaserver.exe' },
    @{Row = -1; Property_01 = 'A'; Property_02 = 'C:\windows\system32\msdtc.exe' }
},
@{
    FileName    = 'df.01.txt'
    convertArgs = @{}
    Rows        = 10
    Results     = @{ Row = 0; 'Filesystem' = '/dev/disk4s1s1'; '512-blocks' = '3907805752'; 'Used' = '17463888'; 'Available' = '1387159800'; 'Capacity' = '2%'; 'iused' = '349475'; 'ifree' = '4291443272'; '%iused' = '0%'; 'Mounted_on' = '/' },
    @{ Row = 1; 'Filesystem' = 'devfs'; '512-blocks' = '427'; 'Used' = '427'; 'Available' = '0'; 'Capacity' = '100%'; 'iused' = '739'; 'ifree' = '0'; '%iused' = '100%'; 'Mounted_on' = '/dev' }
},
@{
    FileName    = 'docker.01.txt'
    convertArgs = @{}
    Rows        = 8
    Results     = @{ ROW = 0; 'REPOSITORY' = 'docker101tutorial'; 'TAG' = 'latest'; 'IMAGE_ID' = '234e26cd95c2'; 'CREATED' = '5 weeks ago'; 'SIZE' = '28.9MB' },
    @{ Row = 3; 'REPOSITORY' = 'alpine/git'; 'TAG' = 'latest'; 'IMAGE_ID' = '692618a0d74d'; 'CREATED' = '6 weeks ago'; 'SIZE' = '43.4MB' }
},
@{
    FileName    = 'docker.02.txt'
    convertArgs = @{}
    Rows        = 25
    Results     = @{ ROW = 0; 'NAME' = 'centos/powershell'; 'DESCRIPTION' = "PowerShell is a cross-platform (Windows, Lin`u{2026}"; 'STARS' = '8'; 'OFFICIAL' = ''; 'AUTOMATED' = '[OK]' },
    @{ ROW = 20; 'NAME' = 'powershellduzero/api'; 'DESCRIPTION' = ''; 'STARS' = '0'; 'OFFICIAL' = ''; 'AUTOMATED' = '' }
},
@{
    FileName    = 'docker.03.txt'
    convertArgs = @{}
    Rows        = 25
    Results     = @{ ROW = 22; 'NAME' = 'zoilus/powershell'; 'DESCRIPTION' = ''; 'STARS' = '0'; 'OFFICIAL' = ''; 'AUTOMATED' = '' },
    @{ ROW = 23; 'NAME' = 'ephesoft/powershell.git'; 'DESCRIPTION' = 'Powershell image with Git pre-installed'; 'STARS' = '0'; 'OFFICIAL' = ''; 'AUTOMATED' = '' }
},
@{
    FileName    = 'docker.04.txt'
    convertArgs = @{}
    Rows        = 1
    Results     = @(@{ ROW = 0; 'DRIVER' = 'local'; 'VOLUME_NAME' = '6bf5c897a2cdcf0bfe5da45a795fa7fe94032f79b98d2f63563578ed40d0f0c6' })
},
@{
    FileName    = 'getmac.01.txt'
    convertArgs = @{}
    Rows        = 6
    Results     = @{ ROW = 3; 'Physical_Address' = '0C-C4-7A-28-C7-13'; 'Transport_Name' = '\Device\Tcpip_{8234FC65-751E-4B56-AB8A-0758A4C18889}' },
    @{ ROW = 4; 'Physical_Address' = '0C-C4-7A-28-C7-12'; 'Transport_Name' = 'N/A' }
},
@{
    FileName    = 'kmutil.01.txt'
    convertArgs = @{}
    Rows        = 30
    Results     = @{ ROW = 0; 'Index' = '1'; 'Refs' = '161'; 'Address' = '0'; 'Size' = '0'; 'Wired' = '0'; 'Name_(Version)_UUID_<Linked_Against>' = "com.apple.kpi.bsd (22.3.0) 10E5D254-4A37-3A2A-B560-E6956A093ADE `u{003C}`u{003E}" },
    @{ ROW = 1; 'Index' = '2'; 'Refs' = '12'; 'Address' = '0'; 'Size' = '0'; 'Wired' = '0'; 'Name_(Version)_UUID_<Linked_Against>' = "com.apple.kpi.dsep (22.3.0) 10E5D254-4A37-3A2A-B560-E6956A093ADE `u{003C}`u{003E}" }
},
@{
    FileName    = 'ls.01.txt'
    convertArgs = @{skip = 1; NoHeader = $true }
    Rows        = 5
    Results     = @{ ROW = 0; 'Property_01' = '-rw-r--r--'; 'Property_02' = '1'; 'Property_03' = 'james'; 'Property_04' = 'staff'; 'Property_05' = '2687'; 'Property_06' = 'Oct'; 'Property_07' = '12'; 'Property_08' = '16:58'; 'Property_09' = 'NativeTableParser.deps.json' },
    @{ ROW = 3; 'Property_01' = '-rwxr--r--'; 'Property_02' = '1'; 'Property_03' = 'james'; 'Property_04' = 'staff'; 'Property_05' = '354304'; 'Property_06' = 'Mar'; 'Property_07' = '7'; 'Property_08' = '2019'; 'Property_09' = 'System.Management.Automation.dll' }
},
@{
    FileName    = 'ls.02.txt'
    convertArgs = @{ skip = 1; NoHeader = $true }
    Rows        = 5
    Results     = @(@{ ROW = 4; 'Property_01' = '-rwxr--r--'; 'Property_02' = '1'; 'Property_03' = 'james'; 'Property_04' = '457864'; 'Property_05' = 'Aug'; 'Property_06' = '19'; 'Property_07' = '12:49'; 'Property_08' = 'System.Text.Json.dll' })
},
@{
    FileName    = 'ls.03.txt'
    convertArgs = @{ skip = 1; NoHeader = $true }
    Rows        = 5
    Results     = @{ ROW = 1; 'Property_01' = '-rw-r--r--'; 'Property_02' = '1'; 'Property_03' = '12288'; 'Property_04' = 'Oct'; 'Property_05' = '12'; 'Property_06' = '16:58'; 'Property_07' = 'NativeTableParser.dll' },
    @{ ROW = 2; 'Property_01' = '-rw-r--r--'; 'Property_02' = '1'; 'Property_03' = '14512'; 'Property_04' = 'Oct'; 'Property_05' = '12'; 'Property_06' = '16:58'; 'Property_07' = 'NativeTableParser.pdb' }
},
@{
    FileName    = 'ps.01.txt'
    convertArgs = @{ }
    Rows        = 6
    Results     = @{ ROW = 0; 'PID' = '2596'; 'TTY' = 'ttys000'; 'TIME' = '1:59.45'; 'CMD' = '/usr/local/bin/pwsh -l' },
    @{ ROW = 1; 'PID' = '2601'; 'TTY' = 'ttys001'; 'TIME' = '0:44.13'; 'CMD' = '/usr/local/bin/pwsh -l' },
    @{ ROW = 2; 'PID' = '2661'; 'TTY' = 'ttys002'; 'TIME' = '0:23.53'; 'CMD' = '/usr/local/bin/pwsh' }
},
@{
    FileName    = 'ps.02.txt'
    convertArgs = @{ }
    Rows        = 88
    Results     = @{ ROW = 82; 'UID' = '0'; 'PID' = '69835'; 'PPID' = '2596'; 'C' = '0'; 'STIME' = '8:25AM'; 'TTY' = 'ttys000'; 'TIME' = '0:00.01'; 'CMD' = '/bin/ps -ef' },
    @{ ROW = 85; 'UID' = '501'; 'PID' = '2669'; 'PPID' = '2658'; 'C' = '0'; 'STIME' = '9:45AM'; 'TTY' = 'ttys003'; 'TIME' = '0:23.04'; 'CMD' = '/usr/local/bin/pwsh' },
    @{ ROW = 87; 'UID' = '501'; 'PID' = '2676'; 'PPID' = '2658'; 'C' = '0'; 'STIME' = '9:45AM'; 'TTY' = 'ttys005'; 'TIME' = '0:23.16'; 'CMD' = '/usr/local/bin/pwsh' }
},
@{
    FileName    = 'ps.03.txt'
    convertArgs = @{ }
    Rows        = 6
    Results     = @{ ROW = 0; 'UID' = '501'; 'PID' = '2596'; 'PPID' = '2587'; 'F' = '4006'; 'CPU' = '0'; 'PRI' = '33'; 'NI' = '0'; 'SZ' = '39662748'; 'RSS' = '167544'; 'WCHAN' = '-'; 'S' = "S`u{002B}"; 'ADDR' = '0'; 'TTY' = 'ttys000'; 'TIME' = '2:01.35'; 'CMD' = '/usr/local/bin/pwsh -l' },
    @{ ROW = 5; 'UID' = '501'; 'PID' = '2676'; 'PPID' = '2658'; 'F' = '4006'; 'CPU' = '0'; 'PRI' = '33'; 'NI' = '0'; 'SZ' = '39658344'; 'RSS' = '136596'; 'WCHAN' = '-'; 'S' = "Ss`u{002B}"; 'ADDR' = '0'; 'TTY' = 'ttys005'; 'TIME' = '0:23.17'; 'CMD' = '/usr/local/bin/pwsh' }

},
@{
    FileName    = 'ps.04.txt'
    convertArgs = @{ }
    Rows        = 785
    Results     = @{ ROW = 782; 'PID' = '2676'; 'TTY' = 'ttys005'; 'TIME' = '0:27.57'; 'CMD' = 'pwsh' },
    @{ ROW = 783; 'PID' = '94197'; 'TTY' = 'ttys006'; 'TIME' = '1:07.57'; 'CMD' = 'pwsh' },
    @{ ROW = 784; 'PID' = '94200'; 'TTY' = 'ttys007'; 'TIME' = '0:05.13'; 'CMD' = 'pwsh' }
},
@{
    FileName    = 'tasklist.01.txt'
    convertArgs = @{}
    Rows        = 46
    Results     = @{ ROW = 0; 'Image_Name' = '========================='; 'PID' = '========'; 'Session_Name' = '================'; 'Session#' = '==========='; 'Mem_Usage' = '============' },
    @{ ROW = 1; 'Image_Name' = 'System Idle Process'; 'PID' = '0'; 'Session_Name' = 'Services'; 'Session#' = '0'; 'Mem_Usage' = '8 K' },
    @{ ROW = 2; 'Image_Name' = 'System'; 'PID' = '4'; 'Session_Name' = 'Services'; 'Session#' = '0'; 'Mem_Usage' = '8,240 K' }
},
@{
    FileName    = 'tasklist.02.txt'
    convertArgs = @{}
    Rows        = 39
    Results     = @{ ROW = 14; 'Image_Name' = 'svchost.exe'; 'PID' = '1364'; 'Session_Name' = 'Services'; 'Session#' = '0'; 'Mem_Usage' = '24,896 K' },
    @{ ROW = 15; 'Image_Name' = 'svchost.exe'; 'PID' = '1412'; 'Session_Name' = 'Services'; 'Session#' = '0'; 'Mem_Usage' = '18,576 K' }
},
@{
    FileName    = 'who.01.txt'
    convertArgs = @{ NoHeader = $true }
    Rows        = 6
    Results     = @{ ROW = 0; 'Property_01' = 'reboot'; 'Property_02' = '~'; 'Property_03' = 'Sep'; 'Property_04' = '14'; 'Property_05' = '10:10'; 'Property_06' = '00:04'; 'Property_07' = '1' },
    @{ ROW = 4; 'Property_01' = 'james'; 'Property_02' = 'ttys002'; 'Property_03' = 'Oct'; 'Property_04' = '5'; 'Property_05' = '16:36'; 'Property_06' = '.'; 'Property_07' = "90609`tterm=0 exit=0" },
    @{ ROW = 5; 'Property_01' = 'james'; 'Property_02' = 'ttys006'; 'Property_03' = 'Oct'; 'Property_04' = '11'; 'Property_05' = '15:41'; 'Property_06' = '.'; 'Property_07' = "25351`tterm=0 exit=0" }
}
Describe 'Test text table parser' {
    BeforeAll {
        $cmdletName = 'ConvertFrom-TextTable'

    }

    Context 'Test JSON output' {

        It "Should create proper json from '<FileName>' " -testCases $testCases {
            param ($FileName, $convertArgs, $rows, $Results)
            $Path = Join-Path $PSScriptRoot assets $FileName
            # do not alter convertArgs directly as it is a reference rather than a copy
            $localArgs = $convertArgs.Clone()
            $localArgs['AsJson'] = $true
            { Get-Content $Path | ConvertFrom-TextTable @localArgs | ConvertFrom-Json -ErrorAction Stop } | Should -Not -Throw
            $result = Get-Content $Path | ConvertFrom-TextTable @localArgs | ConvertFrom-Json -ErrorAction Ignore
            $result | Should -Not -BeNullOrEmpty
            $result.Count | Should -Be $Rows
            foreach ( $r in $results ) {
                $rObject = $result[$r.Row]
                foreach ( $k in $r.Keys.Where({ $_ -ne 'Row' }) ) {
                    $rObject."$k" | Should -Be $r."$k"
                }
            }
        }
    }

    Context 'Test PSObject output' {
        It "Should create proper psobject from '<FileName>' " -testCases $testCases {
            param ($FileName, $convertArgs, $rows, $Results )
            $Path = Join-Path $PSScriptRoot assets $FileName
            $result = Get-Content $Path | ConvertFrom-TextTable @convertArgs
            $result | Should -BeOfType System.Management.Automation.PSObject
            $result.Count | Should -Be $Rows
            foreach ( $r in $results ) {
                $rObject = $result[$r.Row]
                foreach ( $k in $r.Keys.Where({ $_ -ne 'Row' }) ) {
                    $rObject."$k" | Should -Be $r."$k"
                }
            }
        }
    }

    Context 'Column offset use' {
        $expectedResult = @(
            @{ Name = 'Property_01'; Value = 'S1234' }
            @{ Name = 'Property_02'; Value = '56789012' }
            @{ Name = 'Property_03'; Value = '3456789012' }
            @{ Name = 'Property_04'; Value = '34567890123456789' }
            @{ Name = 'Property_05'; Value = '012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789' }
        )
        $testFile = Join-Path $PSScriptRoot assets 'columns.01.txt'
        $result = Get-Content $testFile | ConvertFrom-TextTable -ColumnOffset 0, 5, 13, 23, 40 -noheader
        $line = 0
        $testCases = $result.ForEach({ @{Result = $_; Line = $line++ } })
        BeforeAll {
        }

        It "Specifying column offset breaks string properly for Line: '<Line>'" -TestCases $testCases {
            param ( $Result, $Line )
            foreach ($expected in $expectedResult) {
                $Result.$($expected.Name) | Should -Be $expected.Value
            }
        }

    }


}
