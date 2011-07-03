// SharpGit\IgnoreFile.cs
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
