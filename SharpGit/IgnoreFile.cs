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

                    foreach (string line in File.ReadAllText(_fullPath).Split('\n'))
                    {
                        string trimmedLine = line.TrimEnd('\r');
                        
                        if (
                            String.Empty.Equals(trimmedLine) ||
                            trimmedLine.StartsWith("#")
                        )
                            continue;

                        Rules.Add(new IgnoreRule(trimmedLine));
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
