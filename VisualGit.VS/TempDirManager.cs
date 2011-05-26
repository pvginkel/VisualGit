using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VisualGit.VS
{
    [GlobalService(typeof(IVisualGitTempDirManager))]
    class TempDirManager : VisualGitService, IVisualGitTempDirManager
    {
        readonly TempDirCollection _tempDirs = new TempDirCollection();

        public TempDirManager(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        public string GetTempDir()
        {
            string name = "";
            for (int i = 4; i < 32; i += 2)
            {
                name = Path.Combine(Path.GetTempPath(), "VisualGit\\" + Guid.NewGuid().ToString("N").Substring(0, i));

                if (!Directory.Exists(name))
                    break;
            }
            Directory.CreateDirectory(name);
            _tempDirs.AddDirectory(name, false);
            return name;
        }

        class TempDirCollection : IDisposable
        {
            readonly Dictionary<string, bool> _directories = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            public TempDirCollection()
            {
            }

            ~TempDirCollection()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                try
                {
                    Dispose(true);
                }
                finally
                {
                    GC.SuppressFinalize(this);
                }
            }

            void Dispose(bool disposing)
            {
                Delete();
            }

            void Delete()
            {
                foreach (string dir in _directories.Keys)
                {
                    if (!_directories[dir])
                    {
                        try
                        {
                            RecursiveDelete(new DirectoryInfo(dir));
                        }
                        catch 
                        {
                            // This code potentially runs in the finalizer thread, never throw from here
                        }
                    }
                }
            }

            void RecursiveDelete(DirectoryInfo dir)
            {
                if (dir == null)
                    throw new ArgumentNullException("dir");

                if (!dir.Exists)
                    return;

                DirectoryInfo[] directories = null;
                try
                {
                    directories = dir.GetDirectories();
                }
                catch
                { }

                if (directories != null)
                {
                    foreach (DirectoryInfo sd in directories)
                    {
                        RecursiveDelete(sd);
                    }
                }

                FileInfo[] files = null;
                try
                {
                    files = dir.GetFiles();
                }
                catch { }

                if (files != null)
                {
                    foreach (FileInfo file in files)
                    {
                        try
                        {
                            file.Attributes = FileAttributes.Normal;
                            file.Delete();
                        }
                        catch { }
                    }
                }

                try
                {
                    dir.Attributes = FileAttributes.Normal; // .Net fixes up FileAttributes.Directory
                    dir.Delete();
                }
                catch { }
            }

            public void AddDirectory(string name, bool keepDir)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("name");

                _directories.Add(name, keepDir);
            }
        }
    }
}
