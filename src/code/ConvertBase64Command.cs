// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace Microsoft.PowerShell.TextUtility
{
    [Cmdlet(VerbsData.ConvertFrom, "Base64", DefaultParameterSetName="Text")]
    [OutputType(typeof(string))]
    public sealed class ConvertFromBase64Command : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the base64 encoded string.
        /// </summary>
        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Text")]
        public string EncodedText { get; set; }

        /// <summary>
        /// Gets or sets the AsByteArray switch.
        /// </summary>
        [Parameter()]
        public SwitchParameter AsByteArray { get; set; }

        protected override void ProcessRecord()
        {
            while (EncodedText.Length % 4 != 0)
            {
                EncodedText += "=";
            }
            var base64Bytes = Convert.FromBase64String(EncodedText);

            if (AsByteArray)
            {
                WriteObject(base64Bytes);
            }
            else
            {
                WriteObject(Encoding.UTF8.GetString(base64Bytes));
            }
        }
    }

    [Cmdlet(VerbsData.ConvertTo, "Base64", DefaultParameterSetName="Text")]
    [OutputType(typeof(string))]
    public sealed class ConvertToBase64Command : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the text to encoded to base64.
        /// </summary>
        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the base64 encoded byte array.
        /// </summary>
        [Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="ByteArray")]
        public byte[] ByteArray { get; set; }

        /// <summary>
        /// Gets or sets the InsertBreakLines switch.
        /// </summary>
        [Parameter()]
        public SwitchParameter InsertBreakLines { get; set; }

        private List<byte> _bytearray = new List<byte>();
        private Base64FormattingOptions _base64Option = Base64FormattingOptions.None;

        protected override void ProcessRecord()
        {
            if (InsertBreakLines)
            {
                _base64Option = Base64FormattingOptions.InsertLineBreaks;
            }

            if (ParameterSetName.Equals("Text"))
            {
                var textBytes = Encoding.UTF8.GetBytes(Text);
                WriteObject(Convert.ToBase64String(textBytes, _base64Option));
            }
            else
            {
                _bytearray.AddRange(ByteArray);
            }
        }

        protected override void EndProcessing()
        {
            if (ParameterSetName.Equals("ByteArray"))
            {
                WriteObject(Convert.ToBase64String(_bytearray.ToArray(), _base64Option));
            }
        }
    }
}
