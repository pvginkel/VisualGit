using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NGit;
using NGit.Fnmatch;
using NGit.Ignore;

namespace SharpGit
{
    internal sealed class IgnoreFile
    {
        private string _fullPath;

        public IgnoreFile(string directory)
        {
            Directory = directory;
            LastModified = DateTime.MinValue;

            _fullPath = Path.Combine(Directory, Constants.DOT_GIT_IGNORE);

            Rules = new List<IgnoreRule>();

            Refresh();
        }

        public IList<IgnoreRule> Rules { get; private set; }

        public void Refresh()
        {
            bool update = true;

            try
            {
                var lastModified = new FileInfo(_fullPath).LastWriteTime;

                if (lastModified > LastModified)
                {
                    update = true;
                    LastModified = lastModified;
                }

                if (update)
                {
                    Rules.Clear();

                    foreach (string line in File.ReadAllLines(_fullPath))
                    {
                        if (
                            String.Empty.Equals(line) ||
                            line.StartsWith("#")
                        )
                            continue;

                        Rules.Add(new IgnoreRule(line));
                    }
                }
            }
            catch
            {
                // Ignore exceptions from refreshing the file.
            }
        }

        public string Directory { get; private set; }
        public DateTime LastModified { get; private set; }
    }
}
