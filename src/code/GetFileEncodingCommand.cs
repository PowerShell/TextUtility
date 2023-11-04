// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;
using Microsoft.PowerShell.TextUtility.Properties;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// This class implements the Get-FileEncoding command.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "FileEncoding", DefaultParameterSetName = PathParameterSet)]
    [OutputType(typeof(Encoding))]
    public sealed class GetFileEncodingCommand : PSCmdlet
    {
        #region Parameter Sets

        private const string PathParameterSet = "ByPath";
        private const string LiteralPathParameterSet = "ByLiteralPath";

        #endregion

        #region Parameters

        /// <summary>
        /// Gets or sets path from from which to get encoding.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = PathParameterSet)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets literal path from which to get encoding.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = LiteralPathParameterSet)]
        [Alias("PSPath", "LP")]
        public string LiteralPath
        {
            get
            {
                return _isLiteralPath ? Path : null;
            }

            set
            {
                Path = value;
                _isLiteralPath = true;
            }
        }

        private bool _isLiteralPath;

        #endregion

        /// <summary>
        /// Process paths to get file encoding.
        /// </summary>
        protected override void ProcessRecord()
        {
            string resolvedPath = ResolveFilePath(Path, _isLiteralPath);

            if (!File.Exists(resolvedPath))
            {
                ReportPathNotFound(Path);
            }

            WriteObject(GetPathEncoding(resolvedPath));
        }

        /// <summary>
        /// Resolves user provided path using file system provider.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="isLiteralPath">True if the wildcard resolution should not be attempted.</param>
        /// <returns>The resolved (absolute) path.</returns>
        private string ResolveFilePath(string path, bool isLiteralPath)
        {
            string resolvedPath;

            try
            {
                ProviderInfo provider = null;
                PSDriveInfo drive = null;

                if (isLiteralPath)
                {
                    resolvedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive);
                }
                else
                {
                    Collection<string> filePaths = SessionState.Path.GetResolvedProviderPathFromPSPath(path, out provider);

                    if (!provider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
                    {
                        ReportOnlySupportsFileSystemPaths(path);
                    }

                    if (filePaths.Count > 1)
                    {
                        ReportMultipleFilesNotSupported();
                    }

                    resolvedPath = filePaths[0];
                }
            }
            catch (ItemNotFoundException)
            {
                resolvedPath = null;
            }

            return resolvedPath;
        }

        /// <summary>
        /// Throws terminating error for not using file system provider.
        /// </summary>
        /// <param name="path">The path to report.</param>
        private void ReportOnlySupportsFileSystemPaths(string path)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.OnlySupportsFileSystemPaths, path);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "OnlySupportsFileSystemPaths", ErrorCategory.InvalidArgument, path);
            ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Throws terminating error for path not found.
        /// </summary>
        /// <param name="path">The path to report.</param>
        private void ReportPathNotFound(string path)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.PathNotFound, path);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "PathNotFound", ErrorCategory.InvalidArgument, path);
            ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Throws terminating error for multiple files being used.
        /// </summary>
        private void ReportMultipleFilesNotSupported()
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, Resources.MultipleFilesNotSupported);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "MultipleFilesNotSupported", ErrorCategory.InvalidArgument, null);
            ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Gets encoding for path.
        /// </summary>
        /// <param name="path">The path to get file encoding.</param>
        /// <returns>The encoding of file.</returns>
        private static Encoding GetPathEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                _ = reader.Read();
                return reader.CurrentEncoding;
            }
        }
    }
}