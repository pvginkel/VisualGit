using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpGit
{
    /// <summary>
    /// Rewrite of GitTools to remove dependency on SharpSvn. See
    /// http://sharpsvn.open.collab.net/source/browse/sharpsvn/trunk/src/SharpSvn/GitTools.cpp
    /// for original version.
    /// </summary>
    public static class GitTools
    {
        private const int MAX_PATH = 260;
        private static char[] _invalidChars;
        private static readonly long EPOCH_TICKS;

        static GitTools()
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            EPOCH_TICKS = time.Ticks;
        }

        public static string GetNormalizedFullPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (PathContainsInvalidChars(path))
                throw new ArgumentException("Path contains invalid characters", "path");
            else if (IsNormalizedFullPath(path))
                return path;


            bool retry = true;

            if (path.Length < MAX_PATH)
            {
                try
                {
                    path = Path.GetFullPath(path);
                    retry = false;
                }
                catch (PathTooLongException) // Path grew by getting full path
                {
                    // Use the retry
                }
            }

            if (retry)
            {
                path = LongGetFullPath(path);

                if (GetPathRootPart(path) == null)
                    throw new PathTooLongException("Paths with a length above MAX_PATH must be rooted");
            }

            if (path.Length >= 2 && path[1] == ':')
            {
                char c = path[0];

                if ((c >= 'a') && (c <= 'z'))
                    path = char.ToUpperInvariant(c) + path.Substring(1);

                string r = path.TrimEnd('\\');

                if (r.Length > 3)
                    return r;
                else
                    return path.Substring(0, 3);
            }
            else if (path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
            {
                string root = GetPathRootPart(path);

                if (root != null && !path.StartsWith(root, StringComparison.Ordinal))
                    path = root + path.Substring(root.Length).TrimEnd('\\');
            }
            else
                path = path.TrimEnd('\\');

            return path;
        }

        public static bool PathContainsInvalidChars(string path)
        {
            char[] invalidChars = _invalidChars;

            if (invalidChars == null)
                _invalidChars = invalidChars = Path.GetInvalidPathChars();

            if (0 <= path.IndexOfAny(invalidChars))
                return true;

            return false;
        }

        public static bool IsNormalizedFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            int c = path.Length;

            if (path.Length < 3)
                return false;

            int i, n;
            if (IsDirSeparator(path, 0))
            {
                if (!IsDirSeparator(path, 1))
                    return false;

                for (i = 2; i < path.Length; i++)
                {
                    // Check hostname rules
                    if (!Char.IsLetterOrDigit(path, i) && 0 > "._-".IndexOf(path[i]))
                        break;
                }

                if (i == 2 || !IsDirSeparator(path, i))
                    return false;

                i++;

                n = i;

                for (; i < path.Length; i++)
                {
                    // Check share name rules
                    if (!Char.IsLetterOrDigit(path, i) && 0 > "._-".IndexOf(path[i]))
                        break;
                }

                if (i == n)
                    return false; // "\\server\"
                else if (i == c)
                    return true; // "\\server\path"
                else if (c > i && !IsDirSeparator(path, i))
                    return false;
                else if (c == i + 1)
                    return false; // "\\server\path\"

                i++;
            }
            else if ((path[1] != ':') || !IsDirSeparator(path, 2))
                return false;
            else if (!((path[0] >= 'A') && (path[0] <= 'Z')))
                return false;
            else
                i = 3;

            while (i < c)
            {
                if (i >= c && IsDirSeparator(path, i))
                    return false; // '\'-s behind each other

                if (i < c && path[i] == '.')
                {
                    int j = i;

                    while (j < c && path[j] == '.')
                        j++;

                    if (IsSeparator(path, j) || j >= c)
                        return false; // Relative path
                }

                n = i;

                while (i < c && !IsInvalid(path, i) && !IsDirSeparator(path, i) && path[i] != Path.AltDirectorySeparatorChar)
                    i++;

                if (n == i)
                    return false;
                else if (i == c)
                    return true;
                else if (!IsDirSeparator(path, i++))
                    return false;

                if (i == c)
                    return false; // We don't like paths with a '\' at the end
            }

            return true;
        }

        private static bool IsDirSeparator(string v, int index)
        {
            if (index < 0 || (index >= v.Length))
                return false;

            return (v[index] == Path.DirectorySeparatorChar);
        }

        private static bool IsSeparator(string v, int index)
        {
            if (index < 0 || (index >= v.Length))
                return false;

            char c = v[index];

            return (c == Path.DirectorySeparatorChar) || (c == Path.AltDirectorySeparatorChar);
        }


        private static bool IsInvalid(string v, int index)
        {
            if (index < 0 || (index >= v.Length))
                return false;

            char[] invalidChars = _invalidChars;

            if (invalidChars == null)
                _invalidChars = invalidChars = Path.GetInvalidPathChars();

            return 0 <= Array.IndexOf(invalidChars, v[index]);
        }

        public static string LongGetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            // Subversion does not have a problem with paths over MAX_PATH, as long as they are absolute
            if (!path.StartsWith("\\\\?\\"))
                path = "\\\\?\\" + path;

            var rPath = new StringBuilder();

            uint sz = (uint)path.Length * sizeof(char);
            uint c = GetFullPathName(path, sz, rPath, null);

            if (c == 0 || c >= sz)
                throw new PathTooLongException("GetFullPath for long paths failed");

            path = rPath.ToString();

            if (path.StartsWith("\\\\?\\"))
                path = path.Substring(4);

            return path;
        }

        private static string GetPathRootPart(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (path.Length >= 3 && path[1] == ':' && path[2] == '\\')
            {
                if (path[0] >= 'a' && path[0] <= 'z')
                    return Char.ToUpper(path[0]) + ":\\";
                else
                    return path.Substring(0, 3);
            }

            if (!path.StartsWith("\\\\"))
                return null;

            int nSlash = path.IndexOf('\\', 2);

            if (nSlash <= 2)
                return null; // No hostname

            int nEnd = path.IndexOf('\\', nSlash + 1);

            if (nEnd < 0)
                nEnd = path.Length;

            return "\\\\" + path.Substring(2, nSlash - 2).ToLowerInvariant() + path.Substring(nSlash, nEnd - nSlash);
        }

        public static string GetNormalizedDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            path = GetNormalizedFullPath(path);

            string root = GetPathRootPart(path);

            int nLs = path.LastIndexOf(Path.DirectorySeparatorChar);

            if (nLs > root.Length)
                return path.Substring(0, nLs);
            else if (nLs == root.Length || (nLs < root.Length && path.Length > root.Length))
                return root;
            else
                return null;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetFullPathName(string lpFileName, uint nBufferLength, [Out] StringBuilder lpBuffer, [Out] StringBuilder lpFilePart);

        internal static DateTime CreateDate(long milliSecondsSinceEpoch)
        {
            long num = EPOCH_TICKS + (milliSecondsSinceEpoch * 10000);

            return new DateTime(num);
        }

        internal static DateTime CreateDateFromGitTime(long time)
        {
            return CreateDate(time * 1000L);
        }

        public static string GetRepositoryPath(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            return GetRepositoryPath(GetAbsolutePath(uri));
        }

        public static string GetRepositoryPath(string fullPath)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");

            var repositoryEntry = RepositoryManager.GetRepository(fullPath);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            // Repository entry is not locked here because no communication
            // with Git is required to get the repository path.

            return repositoryEntry.Repository.GetRepositoryPath(fullPath);
        }

        public static Uri GetUri(string absolutePath)
        {
            if (absolutePath == null)
                throw new ArgumentNullException("absolutePath");

            return new Uri("file:///" + absolutePath);
        }

        public static string GetAbsolutePath(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            string stringUri = uri.ToString();

            if (!stringUri.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
                throw new GitException(GitErrorCode.UnsupportedUriScheme);

            return stringUri.Substring(8).Replace('/', Path.DirectorySeparatorChar);
        }

        public static bool IsBelowManagedPath(string fullPath)
        {
            string repositoryPath;
            return RepositoryUtil.TryGetRepositoryRoot(fullPath, out repositoryPath);
        }

        public static bool IsManagedPath(string fullPath)
        {
            // With Subversion, every path is managed separately. With Git, they
            // aren't, so we don't make a distinction between IsManagedPath and
            // IsBelowManagedPath.

            return IsBelowManagedPath(fullPath);
        }

        public static string GetTruePath(string path)
        {
            return GetTruePath(path, false);
        }

        public static string GetTruePath(string filename, bool bestEffort)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");

            // Strip out any ., .. and \\'s

            filename = Path.GetFullPath(filename);

            if (filename.StartsWith("\\\\"))
                return GetTrueUncPath(filename, bestEffort);
            else
                return GetTrueLocalPath(filename, bestEffort);
        }

        private static string GetTrueUncPath(string filename, bool bestEffort)
        {
            int pos = filename.IndexOf('\\', 2);

            if (pos == -1)
                return filename;

            pos = filename.IndexOf('\\', pos + 1);

            if (pos == -1 || filename.Length < pos + 1)
                return filename;

            string root = filename.Substring(0, pos);

            return FindTruePath(root, filename.Substring(pos + 1), bestEffort);
        }

        private static string GetTrueLocalPath(string filename, bool bestEffort)
        {
            if (filename.Length < 2)
                return null;

            string root = Char.ToUpper(filename[0]) + ":";

            if (filename.Length > 3)
                return FindTruePath(root, filename.Substring(3), bestEffort);
            else
                return root + "\\";
        }

        private static string FindTruePath(string root, string filename, bool bestEffort)
        {
            string[] parts = filename.Split('\\');
            var result = new StringBuilder(root);

            var dirInfo = new DirectoryInfo(root + "\\");

            for (int i = 0; i < parts.Length; i++)
            {
                result.Append('\\');

                // When the path does not exist, we loose the dirInfo and just
                // append the remainder of the path.

                bool resolved = false;

                if (dirInfo != null)
                {
                    var partDirInfo = dirInfo.GetDirectories(parts[i]);

                    if (partDirInfo.Length != 0)
                    {
                        result.Append(partDirInfo[0].Name);
                        dirInfo = partDirInfo[0];
                        resolved = true;
                    }
                    else if (i == parts.Length - 1)
                    {
                        // Only the last part can be a file.

                        var partFileInfo = dirInfo.GetFiles(parts[i]);

                        if (partFileInfo.Length != 0)
                        {
                            result.Append(partFileInfo[0].Name);
                            break;
                        }
                    }
                }

                if (!resolved)
                {
                    if (!bestEffort)
                        return null;

                    dirInfo = null;
                    result.Append(parts[i]);
                }
            }

            return result.ToString();
        }

        public static Uri GetNormalizedUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (!uri.IsAbsoluteUri)
                throw new ArgumentOutOfRangeException("uri");

            return GitUriTarget.CanonicalizeUri(uri);
        }

        public static Uri PathToRelativeUri(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            return PathToUri(path);
        }

        private static Uri PathToUri(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            StringBuilder sb = new StringBuilder();
            Uri result;

            bool afterFirst = false;

            foreach (string p in path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            {
                if (afterFirst)
                    sb.Append((Char)'/');
                else
                    afterFirst = true;

                sb.Append(Uri.EscapeDataString(p));
            }

            if (Uri.TryCreate(sb.ToString(), UriKind.Relative, out result))
                return result;

            throw new ArgumentException("Path is not convertible to uri part", "path");
        }

    }
}
