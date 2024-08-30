---
external help file: Microsoft.PowerShell.TextUtility.dll-Help.xml
Module Name: Microsoft.PowerShell.TextUtility
online version:
ms.date: 08/29/2024
schema: 2.0.0
---

# Compare-Text

## SYNOPSIS

This cmdlet compares two text strings using diff-match-patch.

## SYNTAX

```
Compare-Text [-LeftText] <String> [-RightText] <String> [-View <CompareTextView>]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet compares two text strings using the **diff-match-patch** library.

The [diff-match-patch](https://github.com/google/diff-match-patch) library provides methods used for
synchronizing plain text, similar to the diff functions of `git`.

## EXAMPLES

### Example 1 - Compare two multiline strings

This example shows how to compare two multiline strings using the `Compare-Text` cmdlet. The cmdlet
uses ANSI escape codes to highlight the differences between the two strings.

```powershell
$leftText = @("This is some", "example text.") -join [Environment]::NewLine
$rightText = @("  This is other", "example text used!") -join [Environment]::NewLine
Compare-Text -LeftText $leftText -RightText $rightText -View SideBySide
```

```Output
1 | This is some  |   This is other
2 | example text. | example text…
```

This example output shows the differences in red with strikethrough characters on the left and
additions in green on the right.

## PARAMETERS

### -LeftText

The source string to be compared. This can be a single line or a multiline string.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RightText

The difference string to be compared. This can be a single line or a multiline string.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -View

The view mode to display the comparison. The possible values are:

- `Inline` - (Default) Shows the differences, side-by-side on the same line.
- `SideBySide` - Shows the differences, side-by-side in separate columns.

```yaml
Type: Microsoft.PowerShell.TextUtility.CompareTextView
Parameter Sets: (All)
Aliases:
Accepted values: Inline, SideBySide

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### Microsoft.PowerShell.TextUtility.CompareTextDiff

## NOTES

## RELATED LINKS
