using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace SharpGit
{
    public sealed class GitConflictEventArgs : CancelEventArgs
    {
        private bool _preparedFiles;
        private string _myFile;
        private string _theirFile;
        private string _baseFile;
        private bool? _isBinary;

        public GitConflictEventArgs()
        {
            Choice = GitAccept.Postpone;
        }

        public GitConflictReason ConflictReason { get; internal set; }

        public bool IsBinary
        {
            get
            {
                if (!_isBinary.HasValue)
                    _isBinary = FileSystemUtil.FileIsBinary(MergedFile);

                return _isBinary.Value;
            }
        }

        public string MergedFile { get; set; }

        public string MyFile
        {
            get
            {
                EnsureFiles();
                return _myFile;
            }
        }

        public string TheirFile
        {
            get
            {
                EnsureFiles();
                return _theirFile;
            }
        }

        public string BaseFile
        {
            get
            {
                EnsureFiles();
                return _baseFile;
            }
        }

        public string Path { get; internal set; }
        public GitAccept Choice { get; set; }

        private void EnsureFiles()
        {
            if (!_preparedFiles)
            {
                _preparedFiles = true;

                var args = new GitInfoArgs { PrepareMerge = true };
                GitInfoEventArgs result;

                new GitClient().GetInfo(MergedFile, args, out result);

                _myFile = result.ConflictWork;
                _theirFile = result.ConflictNew;
                _baseFile = result.ConflictOld;
            }
        }

        internal void Cleanup()
        {
            if (_preparedFiles)
            {
                if (_myFile != null && File.Exists(_myFile))
                    File.Delete(_myFile);
                if (_theirFile != null && File.Exists(_theirFile))
                    File.Delete(_theirFile);
                if (_baseFile != null && File.Exists(_baseFile))
                    File.Delete(_baseFile);
            }
        }
    }
}
