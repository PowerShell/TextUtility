using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Text;
using Microsoft.PowerShell.TextUtility.Resources;

namespace Microsoft.PowerShell.TextUtility
{
    /// <summary>
    /// Defines generic path utilities and helper methods for TextUtility.
    /// </summary>
    internal static class PathUtils
    {
        /// <summary>
        /// Resolves user provided path using file system provider.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="command">The command.</param>
        /// <param name="isLiteralPath">True if the wildcard resolution should not be attempted.</param>
        /// <returns>The resolved (absolute) path.</returns>
        internal static string ResolveFilePath(string path, PSCmdlet command, bool isLiteralPath)
        {
            string resolvedPath;

            try
            {
                ProviderInfo provider = null;
                PSDriveInfo drive = null;

                PathIntrinsics sessionStatePath = command.SessionState.Path;

                if (isLiteralPath)
                {
                    resolvedPath = sessionStatePath.GetUnresolvedProviderPathFromPSPath(path, out provider, out drive);
                }
                else
                {
                    Collection<string> filePaths = sessionStatePath.GetResolvedProviderPathFromPSPath(path, out provider);

                    if (!provider.Name.Equals("FileSystem", StringComparison.OrdinalIgnoreCase))
                    {
                        ReportOnlySupportsFileSystemPaths(path, command);
                    }

                    if (filePaths.Count > 1)
                    {
                        ReportMultipleFilesNotSupported(command);
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
        /// <param name="command">The command.</param>
        internal static void ReportOnlySupportsFileSystemPaths(string path, PSCmdlet command)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, PathUtilityStrings.OnlySupportsFileSystemPaths, path);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "OnlySupportsFileSystemPaths", ErrorCategory.InvalidArgument, path);
            command.ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Throws terminating error for path not found.
        /// </summary>
        /// <param name="path">The path to report.</param>
        /// <param name="command">The command.</param>
        internal static void ReportPathNotFound(string path, PSCmdlet command)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, PathUtilityStrings.PathNotFound, path);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "PathNotFound", ErrorCategory.ObjectNotFound, path);
            command.ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Throws terminating error for multiple files being used.
        /// </summary>
        /// <param name="command">The command.</param>
        internal static void ReportMultipleFilesNotSupported(PSCmdlet command)
        {
            var errorMessage = string.Format(CultureInfo.CurrentCulture, PathUtilityStrings.MultipleFilesNotSupported);
            var exception = new ArgumentException(errorMessage);
            var errorRecord = new ErrorRecord(exception, "MultipleFilesNotSupported", ErrorCategory.InvalidArgument, null);
            command.ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Gets encoding for path.
        /// </summary>
        /// <param name="path">The path to get file encoding.</param>
        /// <returns>The encoding of file.</returns>
        internal static Encoding GetPathEncoding(string path)
        {
            using (var reader = new StreamReader(path, Encoding.Default, detectEncodingFromByteOrderMarks: true))
            {
                _ = reader.Read();
                return reader.CurrentEncoding;
            }
        }
    }
}