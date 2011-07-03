// VisualGit.Services\Scc\VisualGitStatus.cs
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

namespace VisualGit
{
    public enum NoSccStatus
    {
        Unknown,
        NotVersioned,
        NotExisting
    }

    [DebuggerDisplay("Content={State}, Uri={Uri}")]
    public sealed class VisualGitStatus
    {
        readonly GitConflictData _treeConflict;
        readonly GitNodeKind _nodeKind;        
        readonly GitStatus _state;
        readonly bool _localCopied;

        readonly DateTime _lastChangeTime;
        readonly string _lastChangeAuthor;
        readonly long _lastChangeRevision;
        readonly long _revision;

        public VisualGitStatus(GitStatusEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            _nodeKind = args.NodeKind;
            _state = args.LocalContentStatus;
            _localCopied = args.LocalCopied;

            if (args.WorkingCopyInfo != null)
            {
                _lastChangeTime = args.WorkingCopyInfo.LastChangeTime;
                _lastChangeRevision = args.WorkingCopyInfo.LastChangeRevision;
                _lastChangeAuthor = args.WorkingCopyInfo.LastChangeAuthor;
                _revision = args.WorkingCopyInfo.Revision;
            }

            _treeConflict = args.TreeConflict;
            if(_treeConflict != null)
                _treeConflict.Detach();
        }

        /// <summary>
        /// Create non-locked, non-copied item with status specified
        /// </summary>
        /// <param name="allStatuses"></param>
        private VisualGitStatus(GitStatus allStatuses)
        {
            _state = allStatuses;
            //_localLocked = false;
            //_localCopied = false;
        }

        #region Static instances
        readonly static VisualGitStatus _unversioned = new VisualGitStatus(GitStatus.NotVersioned);
        readonly static VisualGitStatus _none = new VisualGitStatus(GitStatus.None);
        /// <summary>
        /// Default status for nodes which do exist but are not managed
        /// </summary>
        internal static VisualGitStatus NotVersioned
        {
            get { return _unversioned; }
        }

        /// <summary>
        /// Default status for nodes which don't exist and are not managed
        /// </summary>
        internal static VisualGitStatus NotExisting
        {
            get { return _none; }
        }
        #endregion

        /// <summary>
        /// Content status in working copy
        /// </summary>
        public GitStatus State
        {
            get { return _state; }
        }

        public GitNodeKind NodeKind
        {
            get { return _nodeKind; }
        }

        public DateTime LastChangeTime
        {
            get { return _lastChangeTime; }
        }

        public string LastChangeAuthor
        {
            get { return _lastChangeAuthor; }
        }

        public long LastChangeRevision
        {
            get { return _lastChangeRevision; }
        }

        public long Revision
        {
            get { return _revision; }
        }

        /// <summary>
        /// Gets a boolean indicating whether the file is copied in the working copy
        /// </summary>
        public bool IsCopied
        {
            get { return _localCopied; }
        }

        internal bool HasTreeConflict
        {
            get { return _treeConflict != null; }
        }

        public GitConflictData TreeConflict
        {
            get { return _treeConflict; }
        }
    }

}
