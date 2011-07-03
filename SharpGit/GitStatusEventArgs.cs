// SharpGit\GitStatusEventArgs.cs
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
    public sealed class GitStatusEventArgs : CancelEventArgs
    {
        private string _name;

        public string Name
        {
            get
            {
                if (_name == null && FullPath != null)
                    _name = Path.GetFileName(FullPath);

                return _name;
            }
        }

        public string FullPath { get; internal set; }
        public GitNodeKind NodeKind { get; internal set; }
        public GitStatus LocalContentStatus { get; internal set; }
        public bool LocalCopied { get; internal set; }
        public GitWorkingCopyInfo WorkingCopyInfo { get; internal set; }
        public GitConflictData TreeConflict { get; internal set; }
        public GitInternalStatus InternalContentStatus { get; set; }
    }
}
