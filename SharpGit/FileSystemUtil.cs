using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    internal static class FileSystemUtil
    {
        static FileSystemUtil()
        {
            // Detect whether the file system is read-only by creating a
            // file name with lower case and checking whether the upper case
            // exists.

            string filename = null;

            try
            {
                filename = Path.GetTempFileName();

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

        public static bool CaseSensitive { get; private set; }

        public static StringComparer StringComparer { get; private set; }

        public static StringComparison StringComparison { get; private set; }
    }
}
