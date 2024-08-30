---
external help file: Microsoft.PowerShell.TextUtility.dll-Help.xml
Module Name: Microsoft.PowerShell.TextUtility
online version:
ms.date: 08/29/2024
schema: 2.0.0
---

# ConvertFrom-TextTable

## SYNOPSIS

Converts tabular data to a **PSObject**.

## SYNTAX

```
ConvertFrom-TextTable [-MaximumWidth <Int32>] [-AnalyzeRowCount <Int32>] [-Line] <String[]>
 [-Skip <Int32>] [-NoHeader] [-AsJson] [-ColumnOffset <Int32[]>] [-ConvertPropertyValue]
 [-HeaderLine <Int32>] [-TypeName <String[]>] [<CommonParameters>]
```

## DESCRIPTION

This command converts text data that is formatted as a table into a custom **PSObject**. The command
analyzes the input text to determine the column layout. The command can also accept an array of
column offsets to define the column layout.

## EXAMPLES

### Example 1 - Convert the output of `getmac.exe` to a **PSObject**

The `getmac.exe` command outputs the MAC addresses of the network adapters. This example converts
the output to a **PSObject**. The `Where-Object` cmdlet filters out the separates the header from
the data.

```powershell
getmac.exe | ConvertFrom-TextTable | Where-Object Physical_Address -NotMatch '==========='
```

```Output
Physical_Address  Transport_Name
----------------  --------------
A4-AE-11-11-6A-B4 \Device\Tcpip_{1437A0C9-2898-450E-B49D-72B7D5946E21}
00-1A-7D-DA-71-13 Media disconnected
Disabled          Disconnected
```

### Example 2 - Convert the output of `getmac.exe` to a JSON string

The `getmac.exe` command outputs the MAC addresses of the network adapters. This example converts
the output to a **PSObject**. The `Where-Object` cmdlet filters out the separates the header from
the data.

```powershell
getmac.exe | ConvertFrom-TextTable -AsJson
```

```Output
{ "Physical_Address": "===================", "Transport_Name": "==========================================================" }
{ "Physical_Address": "A4-AE-11-11-6A-B4", "Transport_Name": "\\Device\\Tcpip_{1437A0C9-2898-450E-B49D-72B7D5946E21}" }
{ "Physical_Address": "00-1A-7D-DA-71-13", "Transport_Name": "Media disconnected" }
{ "Physical_Address": "Disabled", "Transport_Name": "Disconnected" }
```

Each row of the table is deserialized to a separate JSON object.

### Example 3 - Convert a table with non-string data values

This example converts the output of the `dir` from the Windows Command Shell to a **PSObject**. The
`Where-Object` cmdlet is used to filter out only the lines that contain data values. Using the
**ColumnOffset** parameter, `ConvertFrom-TextTable` splits the rows into columns at the specified
offsets. The **ConvertPropertyValue** parameter is used to convert the property values to the types
other than strings.

```powershell
$object = cmd /c dir |
    Where-Object {$_ -match '\d{2}:\d{2}'} |
    ConvertFrom-TextTable -NoHeader -ConvertPropertyValue -ColumnOffset 0,24,30,39
$object[3] | Get-Member
```

```Output
   TypeName: System.Management.Automation.PSCustomObject

Name        MemberType   Definition
----        ----------   ----------
Equals      Method       bool Equals(System.Object obj)
GetHashCode Method       int GetHashCode()
GetType     Method       type GetType()
ToString    Method       string ToString()
Property_01 NoteProperty datetime Property_01=8/29/2024 10:08:00 AM
Property_02 NoteProperty int Property_02=0
Property_03 NoteProperty int Property_03=2323
Property_04 NoteProperty string Property_04=ConvertFrom-Base64.md
```

## PARAMETERS

### -AnalyzeRowCount

The maximum number of rows of input to analyze before deciding on the column layout. The default
value is 0, which means that all rows are analyzed. For large data sets, use a non-zero value to
speed up the analysis.

```yaml
Type: System.Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -AsJson

Outputs the result as a JSON string.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnOffset

An array of integers that specify the column offset for each column. When provided, the command
splits the rows into columns at the column offset values, rather than doing analysis.

```yaml
Type: System.Int32[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConvertPropertyValue

When this parameter is specified, the command attempts to convert the property values to one of the
data types: int, int64, decimal, datetime, or timespan. Values that can't be converted are returned
as strings.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HeaderLine

Specifies the line number to use as the header. The default is 0 (the first line).

```yaml
Type: System.Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Line

A string array that contains the text table to be converted.

```yaml
Type: System.String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -MaximumWidth

The maximum width of the tabular text to be converted. The default is 200 characters.

```yaml
Type: System.Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 200
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoHeader

Specifies that the input text has no header row. The command generates column names as
`Property_01`, `Property_02`, and so on.

```yaml
Type: System.Management.Automation.SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TypeName

Adds the type name to **TypesNames** property of the resulting object. If you provide more than one
type name, the names are inserted in reverse order, putting the last type name at the beginning of
list.

Adding type a custom type name allows you to create type-specific formatting for the object output.

```yaml
Type: System.String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Skip

Ignores the specified number of objects and then gets the remaining objects.
Enter the number of objects to skip. The default is 0.

```yaml
Type: System.Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String[]

You can pipe table-formatted text to the command.

## OUTPUTS

### System.Object

## NOTES

## RELATED LINKS
