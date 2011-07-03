// VisualGit.Services\UI\PathSelectorResult.cs
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
using System.Diagnostics;
using SharpGit;

namespace VisualGit.UI
{
    public class PathSelectorResult
    {
        readonly bool _succeeded;
        readonly List<GitItem> _selection;
        GitDepth _depth = GitDepth.Unknown;
        GitRevision _start;
        GitRevision _end;

        public PathSelectorResult(bool succeeded, IEnumerable<GitItem> items)
        {
            if (items == null)
                throw new ArgumentNullException("items");

            _succeeded = succeeded;
            _selection = new List<GitItem>(items);
        }

        public GitDepth Depth
        {
            [DebuggerStepThrough]
            get { return _depth; }
            set { _depth = value; }
        }

        public GitRevision RevisionStart
        {
            [DebuggerStepThrough]
            get { return _start; }
            set { _start = value; }
        }

        public GitRevision RevisionEnd
        {
            [DebuggerStepThrough]
            get { return _end; }
            set { _end = value; }
        }


        public IList<GitItem> Selection
        {
            [DebuggerStepThrough]
            get { return _selection; }
        }

        public bool Succeeded
        {
            [DebuggerStepThrough]
            get { return _succeeded; }
        }
    }
}
