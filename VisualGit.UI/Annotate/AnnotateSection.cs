// VisualGit.UI\Annotate\AnnotateSection.cs
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
using VisualGit.Scc;
using System.ComponentModel;
using VisualGit.Scc.UI;
using System.Globalization;
using SharpGit;

namespace VisualGit.UI.Annotate
{
    /// <summary>
    /// 
    /// </summary>
    class AnnotateRegion
    {
        readonly AnnotateSource _source;
        readonly int _startLine;
        int _endLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotateRegion"/> class.
        /// </summary>
        /// <param name="startLine">The start line.</param>
        /// <param name="endLine">The end line.</param>
        /// <param name="source">The source.</param>
        public AnnotateRegion(int line, AnnotateSource source)
        {
            if(source == null)
                throw new ArgumentNullException("source");

            _source = source;
            _startLine = _endLine = line;
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public AnnotateSource Source
        {
            get { return _source; }
        }

        public int StartLine
        {
            get { return _startLine; }
        }

        /// <summary>
        /// Gets the end line.
        /// </summary>
        /// <value>The end line.</value>
        public int EndLine
        {
            get { return _endLine; }
            internal set { _endLine = value; }
        }

        #region Internal State
        internal bool Hovered;
        #endregion
    }

    class AnnotateSource : VisualGitPropertyGridItem, IAnnotateSection, IGitRepositoryItem
    {
        readonly GitBlameEventArgs _args;
        readonly GitOrigin _origin;
        string _logMessage;

        public AnnotateSource(GitBlameEventArgs blameArgs, GitOrigin origin)
        {
            _args = blameArgs;
            _origin = origin;
        }

        [Category("Git")]
        public string Revision
        {
            get { return _args.Revision; }
        }

        [Category("Git")]
        public string Author
        {
            get { return _args.Author; }
        }

        [Category("Git")]
        public DateTime Time
        {
            get { return _args.Time.ToLocalTime(); }
        }

        [Browsable(false)]
        public GitOrigin Origin
        {
            get { return _origin; }
        }

        [Browsable(false)]
        public string LogMessage
        {
            get
            {
                lock (_args)
                    return _logMessage;
            }
            internal set
            {
                lock (_args)
                    _logMessage = value;
            }
        }

        protected override string ClassName
        {
            get { return string.Format(CultureInfo.InvariantCulture, "r{0}", Revision); }
        }

        protected override string ComponentName
        {
            get { return Origin.Target.FileName; }
        }        

        #region IGitRepositoryItem Members

        string IGitRepositoryItem.Path
        {
            get { return Origin.Target.FullPath; }
        }

        GitNodeKind IGitRepositoryItem.NodeKind
        {
            get { return GitNodeKind.File; }
        }

        GitRevision IGitRepositoryItem.Revision
        {
            get { return Revision; }
        }

        public void RefreshItem(bool refreshParent)
        {
            // Ignore
        }

        #endregion
    }
}
