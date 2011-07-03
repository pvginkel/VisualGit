// VisualGit.VS\TempFileManager.cs
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
using System.Text;
using System.CodeDom.Compiler;
using System.IO;

namespace VisualGit.VS
{
    [GlobalService(typeof(IVisualGitTempFileManager))]
    class TempFileManager : VisualGitService, IVisualGitTempFileManager
    {
        TempFileCollection _tempFileCollection;

        public TempFileManager(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        public TempFileCollection TempFileCollection
        {
            get { return _tempFileCollection ?? (_tempFileCollection = new TempFileCollection()); }
        }

        #region IVisualGitTempFileManager Members

        public string GetTempFile()
        {
            string name = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            File.WriteAllBytes(name, new byte[0]);
            TempFileCollection.AddFile(name, false);
            return name;
        }

        public string GetTempFile(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                throw new ArgumentNullException("extension");

            string name = Path.ChangeExtension(
                Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
                extension);

            File.WriteAllBytes(name, new byte[0]);
            TempFileCollection.AddFile(name, false);

            return name;
        }

        string _lastDir;

        public string GetTempFileNamed(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");

            filename = Path.GetFileName(filename); // Remove any paths

            string name;
            if (_lastDir == null || File.Exists(name = Path.Combine(_lastDir, filename)) ||
               !Directory.Exists(_lastDir))
            {
                _lastDir = GetService<IVisualGitTempDirManager>().GetTempDir();
                name = Path.Combine(_lastDir, filename);
            }

            File.WriteAllBytes(name, new byte[0]);
            TempFileCollection.AddFile(name, false);

            return name;
        }
        #endregion
    }
}
