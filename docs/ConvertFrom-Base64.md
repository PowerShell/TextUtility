---
external help file: Microsoft.PowerShell.TextUtility.dll-Help.xml
Module Name: Microsoft.PowerShell.TextUtility
online version:
ms.date: 08/29/2024
schema: 2.0.0
---

# ConvertFrom-Base64

## SYNOPSIS

Converts a Base64-encoded string back to its original form.

## SYNTAX

```
ConvertFrom-Base64 [-EncodedText] <String> [-AsByteArray] [<CommonParameters>]
```

## DESCRIPTION

The command converts a Base64-encoded string back to its original form. The original form can be a
string or any arbitrary binary data.

## EXAMPLES

### Example 1 - Convert a Base64-encoded string to its original form

```powershell
ConvertFrom-Base64 -EncodedText "SGVsbG8gV29ybGQh"
```

```output
Hello World!
```

### Example 1 - Convert a Base64-encoded string to its original form as a byte array

```powershell
ConvertFrom-Base64 -EncodedText "SGVsbG8gV29ybGQh" -AsByteArray
```

```output
72
101
108
108
111
32
87
111
114
108
100
33
```

## PARAMETERS

### -AsByteArray

Returns the converted output as an array of bytes. This is useful when the original form is binary
data.

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

### -EncodedText

The Base64-encoded string to convert back to its original form.

```yaml
Type: System.String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable,
-InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose,
-WarningAction, and -WarningVariable. For more information, see
[about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String

## OUTPUTS

### System.String

By default, this command returns the converted data as a string.

### System.Byte

When you use the **AsByteArray** parameter, this command returns the converted data as an array of
bytes.

## NOTES

## RELATED LINKS

[ConvertTo-Base64](ConvertTo-Base64.md)
