using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using IOPath = System.IO.Path;

namespace SharpGit
{
    [Serializable]
    public sealed class FilePath : IEquatable<FilePath>
    {
        public static bool CaseSensitive { get; private set; }
        private static StringComparer StringComparer { get; set; }
        private static StringComparison StringComparison { get; set; }

        public string Path { get; private set; }

        static FilePath()
        {
            // Verify that the directory seperator is something we understand.

            if (IOPath.DirectorySeparatorChar != '/' && IOPath.DirectorySeparatorChar != '\\')
                throw new NotSupportedException("Path.DirectorySeperatorChar is not something we understand");

            // Detect whether the file system is read-only by creating a
            // file name with lower case and checking whether the upper case
            // exists.

            string filename = null;

            try
            {
                filename = IOPath.GetTempFileName();

                CaseSensitive =
                    !(File.Exists(filename.ToLower()) && File.Exists(filename.ToUpper()));

                // Select the correct string comparers for easy access.

                if (CaseSensitive)
                {
                    StringComparer = System.StringComparer.OrdinalIgnoreCase;
                    StringComparison = System.StringComparison.OrdinalIgnoreCase;
                }
                else
                {
                    StringComparer = System.StringComparer.Ordinal;
                    StringComparison = System.StringComparison.Ordinal;
                }
            }
            finally
            {
                if (filename != null && File.Exists(filename))
                    File.Delete(filename);
            }
        }

        public FilePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (String.Empty.Equals(path))
                throw new ArgumentException("Path may not be empty", "path");

            Path = SanitizePath(path);
        }

        public FilePath(FilePath other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            Path = other.Path;
        }

        public FilePath(string path, string child)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (String.Empty.Equals(path))
                throw new ArgumentException("Path may not be empty", "path");
            if (child == null)
                throw new ArgumentNullException("child");
            if (String.Empty.Equals(child))
                throw new ArgumentException("Path may not be empty", "child");

            Init(path, new FilePath(child));
        }

        public FilePath(FilePath path, string child)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (child == null)
                throw new ArgumentNullException("child");
            if (String.Empty.Equals(child))
                throw new ArgumentException("Path may not be empty", "child");

            Init(path.Path, new FilePath(child));
        }

        public FilePath(FilePath path, FilePath child)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (child == null)
                throw new ArgumentNullException("child");

            Init(path.Path, child);
        }

        private void Init(string path, FilePath child)
        {
            if (child.IsAbsolute)
                throw new ArgumentException("Path name may not be absolute", "child");

            Path = IOPath.Combine(SanitizePath(path), child.Path);
        }

        private string SanitizePath(string path)
        {
            if (IOPath.DirectorySeparatorChar == '\\')
                path = path.Replace('/', '\\');
            else
                path = path.Replace('\\', '/');
            return path;
        }

        public bool Equals(FilePath other)
        {
            if ((object)other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Path.Equals(other.Path, StringComparison);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as FilePath);
        }

        public override int GetHashCode()
        {
            if (CaseSensitive)
                return Path.GetHashCode();
            else
                return Path.ToUpper().GetHashCode();
        }

        public bool IsAbsolute
        {
            get
            {
                if (IOPath.DirectorySeparatorChar == '\\')
                {
                    // Check for UNC path
                    if (Path.StartsWith("\\\\"))
                        return true;
                    // Check for a normal root
                    if (Path.Length >= 3)
                        return Path.Substring(1, 2) == ":\\" && Char.IsLetter(Path[0]);
                    return false;
                }
                else
                {
                    return Path[0] == '/';
                }
            }
        }

        public string FileName
        {
            get { return IOPath.GetFileName(Path); }
        }

        public string Parent
        {
            get { return IOPath.GetDirectoryName(Path); }
        }

        public FilePath ParentPath
        {
            get
            {
                string parent = Parent;

                if (String.IsNullOrEmpty(parent))
                    return null;
                else
                    return new FilePath(parent);
            }
        }

        public string GetAbsolute()
        {
            if (IsAbsolute)
                return Path;
            else
                return IOPath.Combine(Environment.CurrentDirectory, Path);
        }

        public FilePath GetAbsolutePath()
        {
            return new FilePath(GetAbsolute());
        }

        public string GetCanonical()
        {
            return IOPath.GetFullPath(GetAbsolute());
        }

        public FilePath GetCanonicalPath()
        {
            return new FilePath(GetCanonical());
        }

        public override string ToString()
        {
            return Path;
        }

        public string GetCorrectlyCased()
        {
            return GetCorrectlyCased(true);
        }

        public string GetCorrectlyCased(bool mustExist)
        {
            string filename = GetCanonical();

            if (filename.StartsWith("\\\\"))
                return GetCorrectlyCasedUncPath(filename, mustExist);
            else
                return GetCorrectlyCasedLocalPath(filename, mustExist);
        }

        public FilePath GetCorrectlyCasedPath()
        {
            return GetCorrectlyCasedPath(true);
        }

        public FilePath GetCorrectlyCasedPath(bool mustExist)
        {
            return new FilePath(GetCorrectlyCased(mustExist));
        }

        private string GetCorrectlyCasedUncPath(string filename, bool mustExist)
        {
            int pos = filename.IndexOf('\\', 2);

            if (pos == -1)
                return filename;

            pos = filename.IndexOf('\\', pos + 1);

            if (pos == -1 || filename.Length < pos + 1)
                return filename;

            // We convert the server and share name of UNC paths to lower case.
            // This may not be the nicest, but it is consisten.

            return FindCorrectlyCased(
                filename.Substring(0, pos).ToLower(),
                filename.Substring(pos + 1),
                mustExist
            );
        }

        private string GetCorrectlyCasedLocalPath(string filename, bool mustExist)
        {
            if (filename.Length < 2)
                return null;

            string root = Char.ToUpper(filename[0]) + ":";

            if (filename.Length > 3)
                return FindCorrectlyCased(root, filename.Substring(3), mustExist);
            else
                return root + "\\";
        }

        private string FindCorrectlyCased(string root, string filename, bool mustExist)
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
                    if (mustExist)
                        return null;

                    dirInfo = null;
                    result.Append(parts[i]);
                }
            }

            return result.ToString();
        }

        public static FilePath operator +(FilePath path, string child)
        {
            return new FilePath(path, child);
        }

        public static FilePath operator +(FilePath path, FilePath child)
        {
            return new FilePath(path, child);
        }

        public static bool operator ==(FilePath a, FilePath b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;
            return a.Equals(b);
        }

        public static bool operator !=(FilePath a, FilePath b)
        {
            return !(a == b);
        }
    }
}
