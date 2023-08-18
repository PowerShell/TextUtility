// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Text.Json;

namespace TextTableParser
{
    public class ColumnInfo {
        public int Start;
        public int Length;
        public int SpaceLength;
    }

    public class HeaderField {
        public string Name;
        public int Start;
        public int Length;
    }

    public class ObjectPropertyInfo {
        public HeaderField[] HeaderFields;
        ObjectPropertyInfo(string line, ColumnInfo[] cInfos)
        {
            HeaderFields = new HeaderField[cInfos.Length];
            for(int i = 0; i < cInfos.Length; i++)
            {
                HeaderFields[i] = new HeaderField(){
                    Name = line.Substring(cInfos[i].Start, cInfos[i].Length).Trim(),
                    Start = cInfos[i].Start,
                    Length = cInfos[i].Length
                };

            }
        }
    }

    [Cmdlet("ConvertFrom","TextTable")]
    public class ConvertTextTableCommand : PSCmdlet
    {
        [Parameter()]
        public int MaximumWidth { get; set; } = 200;

        [Parameter()]
        public int AnalyzeRowCount { get; set; } = 0; // 0 is all

        [Parameter(ValueFromPipeline=true,Mandatory=true,Position=0)]
        [AllowEmptyString()]
        public string[] Line { get; set; }

        [Parameter()]
        public int Skip { get; set; } = 0;

        [Parameter()]
        public SwitchParameter NoHeader { get; set; }

        [Parameter()]
        public SwitchParameter AsJson { get; set; }

        [Parameter()]
        public int[] ColumnOffset { get; set; }

        [Parameter()]
        public SwitchParameter ConvertPropertyValue { get; set; }

        [Parameter()]
        public int HeaderLine { get; set; } = 0; // Assume the first line is the header

        [Parameter()]
        public string[] TypeName { get; set; }

        private List<string>Lines = new List<string>();

        private int SkippedLines = 0;
        protected override void BeginProcessing()
        {
            if (NoHeader)
            {
                HeaderLine = -1;
            }

        }

        private int[] SpaceArray;
        private string spaceRepresentation;
        private bool Analyzed = false;

        protected override void ProcessRecord()
        {
            if (Line is null)
            {
                return;
            }

            foreach(string _line in Line)
            {

                // don't add empty lines
                if(string.IsNullOrEmpty(_line))
                {
                    continue;
                }

                // add the line to the list if we've skipped enough lines
                if (SkippedLines++ >= Skip)
                {
                    Lines.Add(_line);
                }

                if (AnalyzeRowCount == 0)
                {
                    continue;
                }

                // We've done the analysis, so just emit the object or json.
                if(Analyzed)
                {
                    Emit(_line);
                    continue;
                }

                if (Lines.Count == 0) { continue; }
                // We've collected enough lines to analyze
                // analyze what we have and then emit them, set the analyzed flag so we will just emit from now on.
                if (! Analyzed && Lines.Count > AnalyzeRowCount)
                {
                    AnalyzeLines(Lines);
                    Analyzed = true;
                    foreach(string l in Lines)
                    {
                        Emit(l);
                    }
                    Lines.Clear(); // unneeded
                    // Calculate the column widths
                }
            }
        }

        protected override void EndProcessing()
        {
            if (Lines.Count == 0) { return; }
            if (!Analyzed)
            {
                AnalyzeLines(Lines);
                foreach(string _line in Lines)
                {
                    Emit(_line);
                }
            }
        }

        private void Emit(string line)
        {
            if (ColumnInfoList != null && columnHeaders != null)
            {
                if (AsJson)
                {
                    var jsonOptions = new JsonSerializerOptions();
                    jsonOptions.MaxDepth = 1;
                    WriteObject(GetJson(ColumnInfoList, line, columnHeaders));
                }
                else
                {
                    WriteObject(GetPsObject(ColumnInfoList, line, columnHeaders));
                }
            }
            else
            {
                WriteError(new ErrorRecord(new Exception("No column info"), "NoColumnInfo", ErrorCategory.InvalidOperation, null));
            }
        }

        private List<ColumnInfo> ColumnInfoList { get; set; }
        private string[] columnHeaders { get; set; }

        private void AnalyzeLines(List<string> lines)
        {
            if (lines.Count == 0)
            {
                return;
            }
            SpaceArray = AnalyzeColumns(Lines);
            spaceRepresentation = GetSpaceRepresentation(Lines.Count, SpaceArray);
            if (ColumnOffset != null)
            {
                ColumnInfoList = GetColumnList(ColumnOffset);
            }
            else
            {
                ColumnInfoList = GetColumnList(Lines.Count, SpaceArray);
            }

            if (NoHeader)
            {
                columnHeaders = new string[ColumnInfoList.Count];
                for(int i = 0; i < ColumnInfoList.Count; i++)
                {
                    columnHeaders[i] = string.Format("Property_{0:00}", i+1);
                }
            }
            else
            {
                columnHeaders = GetHeaderColumns(ColumnInfoList, Lines[HeaderLine]);
                Lines.RemoveAt(HeaderLine);
            }
        }

        private string GetSpaceRepresentation(int count, int[] spaceArray)
        {
            char[] spaceChars = new char[spaceArray.Length];
            for(int i = 0; i < spaceArray.Length; i++)
            {
                if (spaceArray[i] == count)
                {
                    spaceChars[i] = 'S';
                }
                else
                {
                    spaceChars[i] = ' ';
                }
            }
            return new string(spaceChars);
        }

        public PSObject GetPsObject(List<ColumnInfo> cInfos, string line, string[] columnHeaders)
        {
            PSObject o = new PSObject();
            if (TypeName != null)
            {
                foreach (string t in TypeName)
                {
                    o.TypeNames.Insert(0, t);
                }
            }
            object[]data = GetObjectColumnData(cInfos, line);
            Debug.Assert(data.Length == columnHeaders.Length);
            for(int i = 0; i < cInfos.Count; i++)
            {
                o.Properties.Add(new PSNoteProperty(columnHeaders[i], data[i]));
            }
            return o;
        }

        public string GetJson(List<ColumnInfo> cInfos, string line, string[] columnHeaders)
        {
            string[]data = GetStringColumnData(cInfos, line);
            Debug.Assert(data.Length == columnHeaders.Length);
            string[]dataWithHeader = new string[data.Length];
            for(int j = 0; j < data.Length; j++)
            {
                if (data[j] is string)
                {
                    dataWithHeader[j] = string.Format("\"{0}\": \"{1}\"", columnHeaders[j], JsonEncodedText.Encode(data[j]));
                }
                else
                {
                    dataWithHeader[j] = string.Format("\"{0}\": \"{1}\"", columnHeaders[j], (JsonEncodedText.Encode((string)data[j])));
                }
            }
            return string.Format("{{ {0} }}", string.Join(", ", dataWithHeader));
        }

        private object[] GetObjectColumnData(List<ColumnInfo> cInfos, string line)
        {

            object[] data = new object[cInfos.Count];
            for(int i = 0; i < cInfos.Count; i++)
            {
                string value;
                if (cInfos[i].Length == -1) // end of line
                {
                    value = line.Substring(cInfos[i].Start).Trim();
                }
                else
                {
                    value = line.Substring(cInfos[i].Start, cInfos[i].Length).Trim();
                }

                // If ConvertPropertyValue is specified, try to convert to int, int64, decimal, datetime, or timespan.
                if (! ConvertPropertyValue)
                {
                    data[i] = value;
                }
                else if (LanguagePrimitives.TryConvertTo<int>(value, out int intValue))
                {
                    data[i] = intValue;
                }
                else if (LanguagePrimitives.TryConvertTo<Int64>(value, out Int64 int64Value))
                {
                    data[i] = int64Value;
                }
                else if (LanguagePrimitives.TryConvertTo<Decimal>(value, out Decimal decimalValue))
                {
                    data[i] = decimalValue;
                }
                else if (LanguagePrimitives.TryConvertTo<DateTime>(value, out DateTime dateTimeValue))
                {
                    data[i] = dateTimeValue;
                }
                else if (LanguagePrimitives.TryConvertTo<TimeSpan>(value, out TimeSpan timeSpanValue))
                {
                    data[i] = timeSpanValue;
                }
                else if (LanguagePrimitives.TryConvertTo<TimeSpan>(string.Format("0:{0}", value), out TimeSpan exTimeSpanValue))
                {
                    data[i] = exTimeSpanValue;
                }
                else
                {
                    data[i] = value;
                }
            }
            return data;
        }

        // This will return the data in the columns as an array of objects.
        // We will try to convert the data to a type that makes sense.
        private string[] GetStringColumnData(List<ColumnInfo> cInfos, string line)
        {
            string[] data = new string[cInfos.Count];
            for(int i = 0; i < cInfos.Count; i++)
            {
                string value;
                if (cInfos[i].Length == -1) // end of line
                {
                    value = line.Substring(cInfos[i].Start).Trim();
                }
                else
                {
                    value = line.Substring(cInfos[i].Start, cInfos[i].Length).Trim();
                }
                data[i] = value;

            }
            return data;
        }

        private string[] GetHeaderColumns(List<ColumnInfo> cInfos, string line)
        {
            string[] columns = new string[cInfos.Count];
            for(int i = 0; i < cInfos.Count; i++)
            {
                if (cInfos[i].Length == -1) // end of line
                {
                    columns[i] = line.Substring(cInfos[i].Start).Trim().Replace(" ", "_");
                }
                else
                {
                    columns[i] = line.Substring(cInfos[i].Start, cInfos[i].Length).Trim().Replace(" ", "_");
                }
            }
            return columns;
        }

        private int GetMaxLength(List<string>lines)
        {
            int maximumLength = 0;
            foreach(string line in lines)
            {
                if (line.Length > maximumLength)
                {
                    maximumLength = line.Length;
                }
            }
            return maximumLength;
        } 

        // Analyze for white space. If we find consistent white space,
        // then we can use that to determine the columns.
        // If the value in the array element is the same as the number of lines,
        // we have a column.
    
        private int[] AnalyzeColumns(List<string>lines)
        {
            int maximumLength = GetMaxLength(lines);
            int[] SpaceArray = new int[maximumLength];
            for(int i = 0; i < maximumLength; i++)
            {
                SpaceArray[i] = 0;
            }

            foreach(string line in lines)
            {
                for(int i = 0; i < line.Length; i++)
                {
                    if(char.IsWhiteSpace(line[i]))
                    {
                        SpaceArray[i] += 1;
                    }
                }
            }
            return SpaceArray;
        }

        private List<ColumnInfo> GetColumnList(int[]StartColumns)
        {
            List<ColumnInfo> ColumnInfoList = new List<ColumnInfo>();
            for(int i = 0; i < StartColumns.Length; i++)
            {
                int length;
                try
                {
                    length = StartColumns[i+1] - StartColumns[i];
                }
                catch
                {
                    length = -1;
                }
                ColumnInfoList.Add(new ColumnInfo() { Start = StartColumns[i], Length = length, SpaceLength = 0 });

            }
            return ColumnInfoList;
        }

        // Get the column list from the space array.
        private List<ColumnInfo> GetColumnList(int count, int[]SpaceArray)
        {
            List<ColumnInfo> ColumnInfoList = new List<ColumnInfo>();
            for(int i = 0; i < SpaceArray.Length; i++) {
                ColumnInfoList.Add(
                    new ColumnInfo() { Start = i, Length = 0, SpaceLength = 0 }
                    );
                // Chew up the spaces
                while (i < SpaceArray.Length && SpaceArray[i] == count)
                {
                    ColumnInfoList[ColumnInfoList.Count - 1].SpaceLength++;
                    ColumnInfoList[ColumnInfoList.Count - 1].Length++;
                    i++;
                }

                // chew up the non spaces or end of line
                while(i < SpaceArray.Length && SpaceArray[i] != count)
                {
                    ColumnInfoList[ColumnInfoList.Count - 1].Length++;
                    i++;
                }

                int totalLength = ColumnInfoList[ColumnInfoList.Count-1].Length + ColumnInfoList[ColumnInfoList.Count-1].Start;
                if (totalLength >= SpaceArray.Length)
                {
                    ColumnInfoList[ColumnInfoList.Count - 1].Length = -1;
                }

                i--;
            }

            return ColumnInfoList;
        }
    }
}
