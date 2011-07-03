// SharpGit\GitConflictEventArgs.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
                // TODO: GitRevertCommand does not produce a third staged entry
                // (_theirFile). Providing _baseFile does seem to give a logical
                // result, however this should also produce a 2-way merge.
                return _theirFile ?? _baseFile;
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
