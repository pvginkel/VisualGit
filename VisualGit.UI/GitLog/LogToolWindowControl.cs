// VisualGit.UI\GitLog\LogToolWindowControl.cs
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
using System.ComponentModel;
using System.Text;
using System.Diagnostics;
using VisualGit.Scc.UI;
using VisualGit.Scc;
using SharpGit;

namespace VisualGit.UI.GitLog
{
    public sealed partial class LogToolWindowControl : VisualGitToolWindowControl, ILogControl
    {
        string _originalText;
        IList<GitOrigin> _origins;
        public LogToolWindowControl()
        {
            InitializeComponent();
            LogSource = logControl.LogSource;
        }

        public LogToolWindowControl(IContainer container)
            : this()
        {
            container.Add(this);
        }

        LogDataSource _dataSource;
        internal LogDataSource LogSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        protected override void OnFrameCreated(EventArgs e)
        {
            base.OnFrameCreated(e);

            _originalText = Text;

            ToolWindowHost.CommandContext = VisualGitId.LogContextGuid;
            ToolWindowHost.KeyboardContext = VisualGitId.LogContextGuid;
            logControl.Context = Context;
        }

        public IList<GitOrigin> Origins
        {
            get
            {
                if (_origins == null)
                    return null;

                return new List<GitOrigin>(_origins);
            }
        }

        void UpdateTitle()
        {
            Text = _originalText;
            if (_origins == null)
                return;

            StringBuilder sb = new StringBuilder(Text);
            int n = 0;

            foreach (GitOrigin origin in _origins)
            {
                sb.Append((n++ == 0) ? " - " : ", ");
                    
                sb.Append(origin.Target.FileName);
            }

            Text = sb.ToString();
        }

        public void StartLog(GitOrigin target, GitRevision start, GitRevision end)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            StartLog(new GitOrigin[] { target }, start, end);
        }

        public void StartLog(ICollection<GitOrigin> targets, GitRevision start, GitRevision end)
        {
            if (targets == null)
                throw new ArgumentNullException("targets");

            _origins = new List<GitOrigin>(targets);

            UpdateTitle();

            logControl.StartLog(_origins, start, end);
        }

        public void StartMergesEligible(IVisualGitServiceProvider context, GitItem target, string source)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            GitOrigin origin = new GitOrigin(target);
            _origins = new GitOrigin[] { origin };
            UpdateTitle();
            logControl.StartMergesEligible(context, origin, source);
        }

        public void StartMergesMerged(IVisualGitServiceProvider context, GitItem target, string source)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            GitOrigin origin = new GitOrigin(target);
            _origins = new GitOrigin[] { origin };
            UpdateTitle();
            logControl.StartMergesMerged(context, origin, source);
        }

        public bool LogMessageVisible
        {
            [DebuggerStepThrough]
            get { return logControl.ShowLogMessage; }
            [DebuggerStepThrough]
            set { logControl.ShowLogMessage = value; }
        }

        public bool ChangedPathsVisible
        {
            [DebuggerStepThrough]
            get { return logControl.ShowChangedPaths; }
            [DebuggerStepThrough]
            set { logControl.ShowChangedPaths = value; }
        }

        public bool IncludeMerged
        {
            [DebuggerStepThrough]
            get { return logControl.IncludeMergedRevisions; }
            [DebuggerStepThrough]
            set { logControl.IncludeMergedRevisions = value; }
        }

        [DebuggerStepThrough]
        public void FetchAll()
        {
            logControl.FetchAll();
        }

        [DebuggerStepThrough]
        public void Restart()
        {
            logControl.Restart();
        }

        #region ILogControl Members

        public bool ShowChangedPaths
        {
            get
            {
                return logControl.ShowChangedPaths;
            }
            set
            {
                logControl.ShowChangedPaths = value;
            }
        }

        public bool ShowLogMessage
        {
            get
            {
                return logControl.ShowLogMessage;
            }
            set
            {
                logControl.ShowLogMessage = value;
            }
        }

        public bool IncludeMergedRevisions
        {
            get { return logControl.IncludeMergedRevisions; }
            set
            {
                if (value != logControl.IncludeMergedRevisions)
                {
                    logControl.IncludeMergedRevisions = value;
                    logControl.Restart();
                }
            }
        }

        public bool StrictNodeHistory
        {
            [DebuggerStepThrough]
            get { return logControl.StrictNodeHistory; }
            set
            {
                if (value != StrictNodeHistory)
                {
                    logControl.StrictNodeHistory = value;
                    logControl.Restart();
                }
            }
        }

        #endregion
    }
}
