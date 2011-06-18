using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    public static class FileSystemUtil
    {
        private const int BinaryTestScope = 512 * 1024;
        private const int BinaryTestConsecutiveNulls = 3;
        private static readonly object _syncLock = new object();
        private static readonly HashSet<string> _textFiles = new HashSet<string>();
        private static readonly HashSet<string> _binaryFiles = new HashSet<string>();

        static FileSystemUtil()
        {
            // Detect whether the file system is read-only by creating a
            // file name with lower case and checking whether the upper case
            // exists.

            string filename = Path.GetTempFileName();

            try
            {
                CaseSensitive = !(File.Exists(filename.ToLower()) && File.Exists(filename.ToUpper()));

                // Select the correct string comparers for easy access.

                if (CaseSensitive)
                {
                    StringComparer = System.StringComparer.Ordinal;
                    StringComparison = System.StringComparison.Ordinal;
                }
                else
                {
                    StringComparer = System.StringComparer.OrdinalIgnoreCase;
                    StringComparison = System.StringComparison.OrdinalIgnoreCase;
                }
            }
            finally
            {
                File.Delete(filename);
            }
        }

        public static bool CaseSensitive { get; private set; }

        public static StringComparer StringComparer { get; private set; }

        public static StringComparison StringComparison { get; private set; }

        public static bool FileIsBinary(string fullPath)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");

            // Run some statistical tests on my local machine, and three
            // consecutive \0's in the first 512K seems to look like a good
            // indication of a binary file.

            string extension = Path.GetExtension(fullPath).ToLower();

            if (!String.Empty.Equals(extension))
            {
                lock (_syncLock)
                {
                    if (_textFiles.Contains(extension))
                        return false;
                    if (_binaryFiles.Contains(extension))
                        return true;
                }
            }

            bool isBinary = false;

            using (var stream = File.Open(fullPath, System.IO.FileMode.Open))
            {
                var buffer = new byte[4096];
                int longest = 0;

                for (int i = 0; i < BinaryTestScope / buffer.Length && !isBinary; i++)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);

                    // Fast path when we have too few bytes to make a informed
                    // decision. Result is not cached.

                    if (i == 0 && read < BinaryTestConsecutiveNulls)
                        return false;

                    if (read <= 0)
                        break;

                    for (int j = 0; j < read && !isBinary; j++)
                    {
                        if (buffer[j] == 0)
                        {
                            longest++;

                            if (longest == BinaryTestConsecutiveNulls)
                            {
                                isBinary = true;
                                break;
                            }
                        }
                        else
                        {
                            longest = 0;
                        }
                    }
                }
            }

            if (!String.Empty.Equals(extension))
            {
                lock (_syncLock)
                {
                    if (isBinary)
                        _binaryFiles.Add(extension);
                    else
                        _textFiles.Add(extension);
                }
            }

            return isBinary;
        }

    }
}
