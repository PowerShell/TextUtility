// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.PowerShell.TextUtility
{
    [Cmdlet(VerbsData.Edit, "Content", DefaultParameterSetName = ParameterSetPath, SupportsShouldProcess = true)]
    public class EditContentCommand : PSCmdlet
    {
        internal const string CommandName = "Edit-Content";
        private const string ParameterSetPath = "PathParameterSet";
        private const string ParameterSetLiteralPath = "PathLiteralParameterSet";
        private const int lohObjectSizeThreshold = 85000;

        private Regex[] _regexes;
        private string _patternArrayAsString;
        private string _replacementArrayAsString;

        /// <summary>
        /// Specifies a path to one or more file locations. Wildcards are permitted.
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
                   ParameterSetName = ParameterSetPath)]
        [ValidateNotNullOrEmpty]
        public string[] Path { get; set; }

        /// <summary>
        /// Specifies a path to one or more file locations. Unlike the Path parameter, the value of the LiteralPath
        /// parameter is used exactly as entered. No characters are interpreted as wildcards. If the path includes
        /// escape characters, enclose them in single quotation marks.
        /// Single quotation marks tell PowerShell not to interpret any characters as escape sequences.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true,
                   ParameterSetName = ParameterSetLiteralPath)]
        [ValidateNotNullOrEmpty]
        [Alias("PSPath", "LP")]
        public string[] LiteralPath { get; set; }

        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNull]
        [AllowEmptyString]
        public string[] Pattern { get; set; }

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNull]
        [AllowEmptyString]
        public string[] Replacement { get; set; }

        [Parameter]
        [ArgumentToEncodingTransformation]
        [ArgumentCompleter(typeof(ArgumentEncodingCompletionsAttribute))]
        [ValidateNotNullOrEmpty]
        public Encoding Encoding { get; set; }

        [Parameter]
        public SwitchParameter CaseSensitive { get; set; }

        [Parameter]
        public SwitchParameter SimpleMatch { get; set; }

        [Parameter]
        public SwitchParameter SingleString { get; set; }

        [Parameter]
        public SwitchParameter PassThru { get; set; }

        [Parameter]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing()
        {
            if (Pattern.Length != Replacement.Length)
            {
                var msg = "The array length must be the same for both the Pattern and Replacement parameters.";
                var exc = new PSArgumentException(msg, "Replacement");
                ThrowTerminatingError(new ErrorRecord(exc, CommandName, ErrorCategory.InvalidArgument, null));
            }

            var patternStrBld = new StringBuilder();
            var replacementStrBld = new StringBuilder();

            _regexes = new Regex[Pattern.Length];
            for (int i = 0; i < Pattern.Length; i++)
            {
                string pattern = SimpleMatch ? Regex.Escape(Pattern[i]) : Pattern[i];
                RegexOptions regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                // TODO: RKH 2020-12-05 Determine if there are cases where using a compiled regex gives better perf.
                _regexes[i] = new Regex(pattern, regexOptions);

                if (i != 0)
                {
                    patternStrBld.Append(",");
                    replacementStrBld.AppendFormat(",");
                }

                patternStrBld.AppendFormat("'{0}'", Pattern[i]);
                replacementStrBld.AppendFormat("'{0}'", Replacement[i]);
            }

            _patternArrayAsString = patternStrBld.ToString();
            _replacementArrayAsString = replacementStrBld.ToString();
        }

        protected override void ProcessRecord()
        {
            // Resolve paths for the selected parameterset
            var resolvedPaths = new List<string>();
            string[] paths = ParameterSetName.Equals(ParameterSetPath) ? Path : LiteralPath;
            foreach (string path in paths)
            {
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        if (ParameterSetName.Equals(ParameterSetPath))
                        {
                            Collection<PathInfo> pathInfos = SessionState.Path.GetResolvedPSPathFromPSPath(path);
                            resolvedPaths.AddRange(pathInfos.Select(pi => pi.Path));
                        }
                        else
                        {
                            resolvedPaths.Add(SessionState.Path.GetUnresolvedProviderPathFromPSPath(path));
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteError(new ErrorRecord(ex, "PathNotFound", ErrorCategory.ObjectNotFound, path));
                    }
                }
            }

            // Process each path
            foreach (string path in resolvedPaths)
            {
                try
                {
                    // Check each path to verify it is not a directory.  
                    FileAttributes attrs = File.GetAttributes(path);
                    if ((attrs | FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        var exc = new PSArgumentException(
                            $"Unable to edit content because it is a directory: '{path}'. Specify a path to a file.",
                            ParameterSetName.Equals(ParameterSetPath) ? "Path" : "LiteralPath");
                        WriteError(new ErrorRecord(exc, "InvalidOperation", ErrorCategory.InvalidOperation, path));
                        continue;
                    }

                    if (ShouldProcess(path, $"{CommandName} replacing pattern " + _patternArrayAsString + " with " + _replacementArrayAsString))
                    {
                        if (Force) MakeFileWritable(path);


                        var fileData = new FileData(path);
                        if (SingleString)
                        {
                            EditFileAsSingleString(fileData);
                        }
                        else if (fileData.Length < (lohObjectSizeThreshold - 1000))
                        {
                            EditFileByLineMemoryBacked(fileData);
                        }
                        else
                        {
                            // The file size is large enough that editing it in memory would place MemStream objects in the LOH, so edit use a temp file.
                            // The modified temp file contents are then copied back to the source file (after regex processing).
                            EditFileByLineFileBacked(fileData);
                        }
                    }

                    if (PassThru)
                    {
                        Collection<PSObject> results = SessionState.InvokeProvider.Item.Get(path);
                        if (results.Count > 0)
                        {
                            WriteObject(results[0]);
                        }
                    }
                }
                catch (FileNotFoundException ex)
                {
                    WriteError(new ErrorRecord(ex, "PathNotFound", ErrorCategory.ObjectNotFound, path));
                }
                catch (SecurityException ex)
                {
                    WriteError(new ErrorRecord(ex, "SecurityError", ErrorCategory.SecurityError, path));
                }
                catch (UnauthorizedAccessException ex)
                {
                    WriteError(new ErrorRecord(ex, "UnauthorizedError", ErrorCategory.SecurityError, path));
                }
                catch (PipelineStoppedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "FileError", ErrorCategory.NotSpecified, path));
                }
            }
        }

        private void EditFileAsSingleString(FileData fileData)
        {
            using (var fileStream = new FileStream(fileData.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                if (Encoding == null) WriteVerboseEncodingInfo(fileData);

                Encoding encoding = Encoding ?? fileData.Encoding;
                var streamReader = new StreamReader(fileStream, encoding);
                var content = streamReader.ReadToEnd();

                for (int i = 0; i < _regexes.Length; i++)
                {
                    content = _regexes[i].Replace(content, Replacement[i]);
                }

                streamReader.DiscardBufferedData();
                fileStream.SetLength(0L);
                var streamWriter = new StreamWriter(fileStream, encoding);
                streamWriter.Write(content);
                streamWriter.Flush();
            }
        }

        private void EditFileByLineFileBacked(FileData fileData)
        {
            string tempPath = System.IO.Path.GetTempFileName();
            WriteVerbose(string.Format($"{CommandName} using temp file '{tempPath}' for '{fileData.Path}'"));

            try
            {
                using (var sourceFileStream = new FileStream(fileData.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                using (var editResultsStream = new FileStream(tempPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    EditFileByLineImpl(fileData, sourceFileStream, editResultsStream);
                }
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        private void EditFileByLineMemoryBacked(FileData fileData)
        {
            int memoryStreamCapacity;

            // If file length is within 20% of LOH size or higher, jump up to next order ot magnitude
            // to limit the number of different sized LOH segments created.  Keeping in mind that the
            // edit operation can make the file larger.
            if (fileData.Length >= (lohObjectSizeThreshold * 0.8))
            {
                memoryStreamCapacity = (int)Math.Pow(10, (int)(Math.Ceiling(Math.Log10(fileData.Length))));
            }
            else
            {
                memoryStreamCapacity = (int)Math.Max(10, fileData.Length);
            }

            using (var sourceFileStream = new FileStream(fileData.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            using (var editResultsStream = new MemoryStream(memoryStreamCapacity))
            {
                EditFileByLineImpl(fileData, sourceFileStream, editResultsStream);
            }
        }

        private void EditFileByLineImpl(FileData fileData, FileStream sourceFileStream, Stream editResultsStream)
        {
            if (Encoding == null) WriteVerboseEncodingInfo(fileData);

            Encoding writeEncoding = Encoding ?? fileData.Encoding;
            var streamReader = new StreamReader(sourceFileStream);
            var streamWriter = new StreamWriter(editResultsStream, writeEncoding);

            string prevLine = null;
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                if (prevLine != null) streamWriter.WriteLine(prevLine);
                prevLine = line;
                for (int i = 0; i < _regexes.Length; i++)
                {
                    prevLine = _regexes[i].Replace(prevLine, Replacement[i]);
                }
            }

            // Use Write or WriteLine on last line depending on whether the source file ends in a newline.
            if (fileData.LastLineEndsWithNewline)
            {
                streamWriter.WriteLine(prevLine ?? "");
            }
            else
            {
                streamWriter.Write(prevLine ?? "");
            }
            streamWriter.Flush();

            // Resets results stream and source file stream to beginning to prep for copy operation.
            streamReader.DiscardBufferedData();
            sourceFileStream.SetLength(0L);
            editResultsStream.Seek(0L, SeekOrigin.Begin);

            editResultsStream.CopyTo(sourceFileStream);
            sourceFileStream.Flush();
        }

        private void MakeFileWritable(string path)
        {
            var fileInfo = new FileInfo(path);
            if (fileInfo.IsReadOnly)
            {
                WriteVerbose($"{CommandName} -Force specified, making readonly file writable: '{path}'");
                fileInfo.IsReadOnly = false;
            }
        }

        private void WriteVerboseEncodingInfo(FileData fileData)
        {
            var msg = string.Format("{0} detected encoding of {1} with {2}BOM for '{3}'{4}",
                                    CommandName,
                                    fileData.Encoding.EncodingName,
                                    (fileData.EncoderEmitsUtf8Identifier ? "" : "no "),
                                    fileData.Path,
                                    ((Encoding == null) ? "" : " but overriden with " + Encoding + " encoding."));
            WriteVerbose(msg);
        }

        internal class FileData
        {
            private readonly byte[] _utf8Bom = { 0xEF, 0xBB, 0xBF };
            private readonly char[] _tempReadEncodingBuffer = new char[256];

            public FileData(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException("path");

                Path = path;
                EncoderEmitsUtf8Identifier = true;

                using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Length = fileStream.Length;

                    // According to MSDN topic, stream reader can't return accurate encoding until after the first read,
                    // so read some bytes if stream position indicates no reading has been done.
                    var streamReader = new StreamReader(fileStream, detectEncodingFromByteOrderMarks: true);
                    streamReader.Read(_tempReadEncodingBuffer, 0, _tempReadEncodingBuffer.Length);
                    Encoding = streamReader.CurrentEncoding;

                    // Do not use streamReader after this point. If so, you need to call streamReader.DiscardBufferedData()
                    // to resync buffer with the underlying stream.

                    // Determine if file ends with a newline
                    byte[] endBytes;
                    if (fileStream.Length >= 2)
                    {
                        fileStream.Seek(-2, SeekOrigin.End);
                        endBytes = new byte[2];
                        fileStream.Read(endBytes, 0, 2);
                        LastLineEndsWithNewline = (endBytes[0] == '\n') || (endBytes[0] == '\r') || (endBytes[1] == '\n') || (endBytes[1] == '\r');
                    }
                    else if (fileStream.Length == 1)
                    {
                        fileStream.Seek(-1, SeekOrigin.End);
                        endBytes = new byte[1];
                        fileStream.Read(endBytes, 0, 1);
                        LastLineEndsWithNewline = (endBytes[0] == '\n') || (endBytes[0] == '\r');
                    }

                    // Just because StreamReader says it is UTF8, that doesn't mean the original
                    // file has a UTF-8 BOM, this code attempts to detect that configure the returned
                    // encoding to only write a BOM if the original file had a BOM.
                    if (Encoding.Equals(Encoding.UTF8))
                    {
                        if (fileStream.Length < _utf8Bom.Length)
                        {
                            // Can't have a BOM if file length is less than that of BOM
                            EncoderEmitsUtf8Identifier = false;
                            Encoding = new UTF8Encoding(EncoderEmitsUtf8Identifier, throwOnInvalidBytes: true);
                        }
                        else
                        {
                            var fileBytes = new byte[_utf8Bom.Length];
                            fileStream.Seek(0L, SeekOrigin.Begin);
                            fileStream.Read(fileBytes, 0, fileBytes.Length);
                            for (int i = 0; i < _utf8Bom.Length; i++)
                            {
                                if (fileBytes[i] != _utf8Bom[i])
                                {
                                    EncoderEmitsUtf8Identifier = false;
                                    Encoding = new UTF8Encoding(EncoderEmitsUtf8Identifier, throwOnInvalidBytes: true);
                                }
                            }
                        }
                    }
                    else if (fileStream.Length < 2)
                    {
                        // No BOM at all so default to UTF8 with no BOM for output
                        EncoderEmitsUtf8Identifier = false;
                        Encoding = new UTF8Encoding(EncoderEmitsUtf8Identifier, throwOnInvalidBytes: true);
                    }
                }
            }

            public string Path { get; private set; }

            public long Length { get; private set; }

            public Encoding Encoding { get; private set; }

            public bool EncoderEmitsUtf8Identifier { get; private set; }

            public bool LastLineEndsWithNewline { get; private set; }
        }
    }

    /// <summary>
    /// To make it easier to specify -Encoding parameter, we add an ArgumentTransformationAttribute here.
    /// When the input data is of type string and is valid to be converted to System.Text.Encoding, we do
    /// the conversion and return the converted value. Otherwise, we just return the input data.
    /// </summary>
    internal sealed class ArgumentToEncodingTransformationAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            switch (inputData)
            {
                case string stringName:
                    if (EncodingConversion.encodingMap.TryGetValue(stringName, out Encoding foundEncoding))
                    {
                        return foundEncoding;
                    }
                    else
                    {
                        return Encoding.GetEncoding(stringName);
                    }
                case int intName:
                    return Encoding.GetEncoding(intName);
            }

            return inputData;
        }
    }

    /// <summary>
    /// Provides the set of Encoding values for tab completion of an Encoding parameter.
    /// </summary>
    internal sealed class ArgumentEncodingCompletionsAttribute : IArgumentCompleter
    {
        private readonly string[] _completions;

        public ArgumentEncodingCompletionsAttribute()
        {
            _completions = new string[] {
                EncodingConversion.Ascii,
                EncodingConversion.BigEndianUnicode,
                EncodingConversion.BigEndianUtf32,
                EncodingConversion.OEM,
                EncodingConversion.Unicode,
                EncodingConversion.Utf7,
                EncodingConversion.Utf8,
                EncodingConversion.Utf8Bom,
                EncodingConversion.Utf8NoBom,
                EncodingConversion.Utf32
             };
        }

        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            var wordToCompletePattern = WildcardPattern.Get(string.IsNullOrWhiteSpace(wordToComplete) ? "*" : wordToComplete + "*", WildcardOptions.IgnoreCase);

            foreach (var str in _completions)
            {
                if (wordToCompletePattern.IsMatch(str))
                {
                    yield return new CompletionResult(str, str, CompletionResultType.ParameterValue, str);
                }
            }
        }
    }

    internal static class EncodingConversion
    {
        internal const string Unknown = "unknown";
        internal const string String = "string";
        internal const string Unicode = "unicode";
        internal const string BigEndianUnicode = "bigendianunicode";
        internal const string BigEndianUtf32 = "bigendianutf32";
        internal const string Ascii = "ascii";
        internal const string Utf8 = "utf8";
        internal const string Utf8NoBom = "utf8NoBOM";
        internal const string Utf8Bom = "utf8BOM";
        internal const string Utf7 = "utf7";
        internal const string Utf32 = "utf32";
        internal const string Default = "default";
        internal const string OEM = "oem";

        internal static readonly string[] TabCompletionResults = {
                Ascii, BigEndianUnicode, BigEndianUtf32, OEM, Unicode, Utf7, Utf8, Utf8Bom, Utf8NoBom, Utf32
            };

        internal static readonly Dictionary<string, Encoding> encodingMap = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase)
        {
            { Ascii, Encoding.ASCII },
            { BigEndianUnicode, Encoding.BigEndianUnicode },
            { BigEndianUtf32, new UTF32Encoding(bigEndian: true, byteOrderMark: true) },
            { Default, ClrFacade.GetDefaultEncoding() },
            { OEM, ClrFacade.GetOEMEncoding() },
            { Unicode, Encoding.Unicode },
#pragma warning disable SYSLIB0001
            { Utf7, Encoding.UTF7 },
#pragma warning restore SYSLIB0001
            { Utf8, ClrFacade.GetDefaultEncoding() },
            { Utf8Bom, Encoding.UTF8 },
            { Utf8NoBom, ClrFacade.GetDefaultEncoding() },
            { Utf32, Encoding.UTF32 },
            { String, Encoding.Unicode },
            { Unknown, Encoding.Unicode },
        };

        /// <summary>
        /// Warn if the encoding has been designated as obsolete.
        /// </summary>
        /// <param name="cmdlet">A cmdlet instance which is used to emit the warning.</param>
        /// <param name="encoding">The encoding to check for obsolescence.</param>
        internal static void WarnIfObsolete(Cmdlet cmdlet, Encoding encoding)
        {
            // Check for UTF-7 by checking for code page 65000
            // See: https://docs.microsoft.com/en-us/dotnet/core/compatibility/corefx#utf-7-code-paths-are-obsolete
            if (encoding != null && encoding.CodePage == 65000)
            {
                cmdlet.WriteWarning("Encoding 'UTF-7' is obsolete, please use UTF-8.");
            }
        }
    }

    /// <summary>
    /// ClrFacade contains all diverging code (different implementation for FullCLR and CoreCLR using if/def).
    /// It exposes common APIs that can be used by the rest of the code base.
    /// </summary>
    internal static class ClrFacade
    {
        private static volatile Encoding s_defaultEncoding;
        private static volatile Encoding s_oemEncoding;

        /// <summary>
        /// Facade for getting default encoding.
        /// </summary>
        internal static Encoding GetDefaultEncoding()
        {
            if (s_defaultEncoding == null)
            {
                // load all available encodings
                EncodingRegisterProvider();
                s_defaultEncoding = new UTF8Encoding(false);
            }

            return s_defaultEncoding;
        }

        /// <summary>
        /// Facade for getting OEM encoding
        /// OEM encodings work on all platforms, or rather codepage 437 is available on both Windows and Non-Windows.
        /// </summary>
        internal static Encoding GetOEMEncoding()
        {
            if (s_oemEncoding == null)
            {
                // load all available encodings
                EncodingRegisterProvider();
#if UNIX
                s_oemEncoding = new UTF8Encoding(false);
#else
                uint oemCp = NativeMethods.GetOEMCP();
                s_oemEncoding = Encoding.GetEncoding((int)oemCp);
#endif
            }

            return s_oemEncoding;
        }

        private static void EncodingRegisterProvider()
        {
            if (s_defaultEncoding == null && s_oemEncoding == null)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
        }

        /// <summary>
        /// Native methods that are used by facade methods.
        /// </summary>
        private static class NativeMethods
        {
            /// <summary>
            /// Pinvoke for GetOEMCP to get the OEM code page.
            /// </summary>
            [DllImport("api-ms-win-core-localization-l1-2-0.dll", SetLastError = false, CharSet = CharSet.Unicode)]
            internal static extern uint GetOEMCP();
        }
    }
}
