using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PS.Build.Nuget.Extensions
{
    /// <summary>
    ///     PathExtensions class provides method(s) which extends native .net method(s) to work with
    ///     string instances that contain file or directory path information.
    /// </summary>
    public static class PathExtensions
    {
        #region Static members

        /// <summary>
        ///     Ensures directory exist.
        /// </summary>
        /// <param name="directory">Full directory path</param>
        /// <returns>Input directory for fluent operations.</returns>
        public static string EnsureDirectoryExist(this string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        ///     Enumerates directories by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Existing directories.</returns>
        public static IEnumerable<string> EnumerateDirectories(string pattern)
        {
            var wildcardChars = new[] { '*', '?' };

            if (!Path.IsPathRooted(pattern)) throw new ArgumentException("Path is not absolute");
            if (pattern == null) throw new InvalidOperationException();
            var containsWildcardChars = pattern.IndexOfAny(wildcardChars) >= 0;
            if (!containsWildcardChars && Directory.Exists(pattern)) return new[] { pattern };

            var pathRoot = Path.GetPathRoot(pattern);

            var subPathParts = pattern.Substring(pathRoot.Length).Split('\\', '/').Where(p => !string.IsNullOrEmpty(p)).ToList();
            var wildcardPartIndex = subPathParts.FindIndex(p => p.Any(s => wildcardChars.Contains(s)));
            if (wildcardPartIndex == -1) throw new InvalidDataException();

            var currentPath = Path.Combine(pathRoot, subPathParts.Take(wildcardPartIndex).Aggregate(string.Empty, Path.Combine));
            if (subPathParts[wildcardPartIndex] == "**")
            {
                var pathPostfix = string.Empty;
                var searchPattern = "*";

                if (subPathParts.Count > wildcardPartIndex + 1) searchPattern = subPathParts[wildcardPartIndex + 1];
                if (subPathParts.Count > wildcardPartIndex + 2)
                    pathPostfix = subPathParts.Skip(wildcardPartIndex + 2).Aggregate(string.Empty, Path.Combine);
                var directoriesEnumeration = Directory.EnumerateDirectories(currentPath, searchPattern, SearchOption.AllDirectories)
                                                      .SelectMany(d => EnumerateDirectories(Path.Combine(d, pathPostfix)));
                if (!string.IsNullOrEmpty(searchPattern)) directoriesEnumeration = directoriesEnumeration.Union(new[] { currentPath });
                return directoriesEnumeration;
            }
            else
            {
                var pathPostfix = string.Empty;
                if (subPathParts.Count > wildcardPartIndex + 1)
                    pathPostfix = subPathParts.Skip(wildcardPartIndex + 1).Aggregate(string.Empty, Path.Combine);

                var directories = Directory.EnumerateDirectories(currentPath, subPathParts[wildcardPartIndex], SearchOption.TopDirectoryOnly);
                return directories.SelectMany(d => EnumerateDirectories(Path.Combine(d, pathPostfix)));
            }
        }

        /// <summary>
        ///     Enumerates files by pattern.
        /// </summary>
        /// <param name="pattern">Search pattern.</param>
        /// <returns>Existing files.</returns>
        public static IEnumerable<string> EnumerateFiles(string pattern)
        {
            try
            {
                if (!Path.IsPathRooted(pattern)) throw new ArgumentException("Path is not absolute");
                var patternFilename = Path.GetFileName(pattern);
                var searchPattern = string.IsNullOrEmpty(patternFilename) ? "*" : patternFilename;
                var searchPath = Path.GetDirectoryName(pattern);
                if (searchPath == null) throw new InvalidOperationException();

                var directories = EnumerateDirectories(searchPath);
                return directories.SelectMany(d => Directory.EnumerateFiles(d, searchPattern));
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        ///     Filter files by specified wildcard pattern.
        /// </summary>
        /// <param name="files">Source files.</param>
        /// <param name="sourcePattern">Wildcard pattern.</param>
        /// <returns>Files that match wildcard pattern.</returns>
        public static IEnumerable<string> Match(IEnumerable<string> files, string sourcePattern)
        {
            files = files ?? Enumerable.Empty<string>();

            Regex fileRegex;
            try
            {
                var searchPattern = Path.GetFileName(sourcePattern);
                fileRegex = new Regex("^" + ConvertWildcardToRegex(searchPattern) + "$", RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }

            Regex directoryRegex;
            try
            {
                var searchPath = Path.GetDirectoryName(sourcePattern);
                directoryRegex = new Regex((Path.IsPathRooted(sourcePattern) ? "^" : string.Empty) + ConvertWildcardToRegex(searchPath) + "$",
                                           RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                directoryRegex = null;
            }

            return files.Where(f =>
            {
                string filename;
                try
                {
                    filename = Path.GetFileName(f) ?? string.Empty;
                }
                catch (Exception)
                {
                    filename = null;
                }

                string directory;
                try
                {
                    directory = Path.GetDirectoryName(f) ?? string.Empty;
                }
                catch (Exception)
                {
                    directory = null;
                }

                var fileComparison = filename != null && fileRegex.IsMatch(filename);

                var directoryComparison = true;
                if (directoryRegex != null && directory != null) directoryComparison = directoryRegex.IsMatch(directory);

                return directoryComparison && fileComparison;
            });
        }

        private static string ConvertWildcardToRegex(string wildcard)
        {
            var pattern = Regex.Escape(wildcard);
            pattern = pattern.Replace(@"\\\*\*\\", @".*");
            pattern = pattern.Replace(@"\\\*\*", @".*");
            pattern = pattern.Replace(@"\*\*", @".*");
            pattern = pattern.Replace(@"\*", @".+");
            pattern = pattern.Replace(@"\?", @".");
            return pattern;
        }

        #endregion
    }
}