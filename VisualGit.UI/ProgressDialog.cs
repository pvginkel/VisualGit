// VisualGit.UI\ProgressDialog.cs
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Generic;
using VisualGit.VS;
using System.IO;
using SharpGit;

namespace VisualGit.UI
{
    /// <summary>
    /// A dialog used for long-running operations.
    /// </summary>
    public partial class ProgressDialog : ProgressDialogBase
    {
        readonly object _instanceLock = new object();
        /// <summary>
        /// Loader Form
        /// </summary>
        /// <param name="inText">Text to be printed in the form.</param>
        public ProgressDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        readonly List<VisualGitAction> _todo = new List<VisualGitAction>();
        const int _bucketCount = 16;
        readonly long[] _buckets = new long[_bucketCount];
        bool _queued;

        string GetActionText(GitNotifyAction action)
        {
            string actionText = action.ToString();

            switch (action)
            {
                case GitNotifyAction.UpdateAdd:
                case GitNotifyAction.UpdateDelete:
                case GitNotifyAction.UpdateReplace:
                case GitNotifyAction.UpdateUpdate:
                case GitNotifyAction.UpdateCompleted:
                case GitNotifyAction.UpdateExternal:
                    actionText = actionText.Substring(6);
                    break;
                case GitNotifyAction.CommitAdded:
                case GitNotifyAction.CommitDeleted:
                case GitNotifyAction.CommitModified:
                case GitNotifyAction.CommitReplaced:
                    actionText = actionText.Substring(6);
                    break;
                case GitNotifyAction.CommitSendData:
                    actionText = "Sending";
                    break;
                case GitNotifyAction.BlameRevision:
                    actionText = "Annotating";
                    break;
            }

            return actionText;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (!DesignMode)
                ResizeToFit();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
                ResizeToFit();
        }

        void ResizeToFit()
        {
            if (actionList != null && pathColumn != null)
                actionList.ResizeColumnsToFit(pathColumn);
        }

        string _splitRoot;

        string SplitRoot
        {
            get
            {
                if (_splitRoot == null && Context != null)
                {
                    IVisualGitSolutionSettings ss = Context.GetService<IVisualGitSolutionSettings>();

                    if (ss != null)
                        _splitRoot = ss.ProjectRootWithSeparator;

                    if (string.IsNullOrEmpty(_splitRoot))
                        _splitRoot = "";
                }

                if (string.IsNullOrEmpty(_splitRoot))
                    return null;
                else
                    return _splitRoot;
            }
        }

        public void OnClientNotify(object sender, GitNotifyEventArgs e)
        {
            string path = e.FullPath;
            GitNotifyAction action = e.Action;
            string rev = e.Revision;

            Enqueue(delegate()
            {
                ListViewItem item = null;
                item = new ListViewItem(GetActionText(action));

                switch (action)
                {
                    case GitNotifyAction.BlameRevision:
                        {
                            string file = Path.GetFileName(path);

                            item.SubItems.Add(string.Format("{0} - r{1}", file, rev));
                            break;
                        }
                    default:
                        if (!string.IsNullOrEmpty(path))
                        {
                            string sr = SplitRoot;
                            if (!string.IsNullOrEmpty(sr))
                            {
                                if (path.StartsWith(sr, StringComparison.OrdinalIgnoreCase))
                                    path = path.Substring(sr.Length).Replace(Path.DirectorySeparatorChar, '/');
                            }

                            item.SubItems.Add(path);
                        }
                        break;
                }

                if (item != null)
                    _toAdd.Add(item);
            });
        }

        private string SizeStr(long numberOfBytes)
        {
            if (numberOfBytes == 1)
                return "1 byte";
            else if (numberOfBytes < 1024)
                return string.Format("{0} bytes", numberOfBytes);
            else if (numberOfBytes < 16384)
                return string.Format("{0:0.0} kByte", numberOfBytes / 1024.0);
            else if (numberOfBytes < 1024 * 1024)
                return string.Format("{0} kByte", numberOfBytes / 1024);
            else if (numberOfBytes < 16 * 1024 * 1024)
                return string.Format("{0:0.0} MByte", numberOfBytes / (1024.0 * 1024.0));
            else
                return string.Format("{0} MByte", numberOfBytes / 1024 / 1024);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
        }

        /// <summary>
        /// Enqueus a task for processing in the UI thread. All tasks will run in the same order as in which they are enqueued
        /// </summary>
        /// <param name="task"></param>
        void Enqueue(VisualGitAction task)
        {
            if (task == null)
                return;

            lock (_todo)
            {
                _todo.Add(task);

                try
                {
                    if (!_queued && IsHandleCreated)
                    {
                        BeginInvoke(new VisualGitAction(RunQueue));
                        _queued = true;
                    }
                }
                catch
                {
                    // Don't kill svn on a failed begin invoke
                }
            }
        }

        readonly List<ListViewItem> _toAdd = new List<ListViewItem>();
        void RunQueue()
        {
            VisualGitAction[] actions;
            lock (_todo)
            {
                _queued = false;
                actions = _todo.ToArray();
                _todo.Clear();
            }

            int n = actionList.Items.Count;

            foreach (VisualGitAction ds in actions)
            {
                ds();
            }

            if (_toAdd.Count > 0)
            {
                ListViewItem[] items = _toAdd.ToArray();
                _toAdd.Clear();
                actionList.Items.AddRange(items);
            }

            if (actionList.Items.Count != n)
            {
                actionList.Items[actionList.Items.Count - 1].EnsureVisible();
                actionList.RedrawItems(n, actionList.Items.Count - 1, false);
            }
        }

        public override IDisposable Bind(GitClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            //client.Processing += new EventHandler<SvnProcessingEventArgs>(OnClientProcessing);
            client.Notify += new EventHandler<GitNotifyEventArgs>(OnClientNotify);
            //client.Progress += new EventHandler<SvnProgressEventArgs>(OnClientProgress);
            //client.Cancel += new EventHandler<SvnCancelEventArgs>(OnClientCancel);

            return new UnbindDisposer(client, this);
        }

        class UnbindDisposer : IDisposable
        {
            GitClient _client;
            ProgressDialog _dlg;

            public UnbindDisposer(GitClient client, ProgressDialog dlg)
            {
                if (client == null)
                    throw new ArgumentNullException("client");
                else if (dlg == null)
                    throw new ArgumentNullException("dlg");

                _client = client;
                _dlg = dlg;
            }

            #region IDisposable Members

            public void Dispose()
            {
                _dlg.Unbind(_client);
            }

            #endregion
        }

        void Unbind(GitClient client)
        {
            client.Notify -= new EventHandler<GitNotifyEventArgs>(OnClientNotify);
            //client.Processing -= new EventHandler<GitProcessingEventArgs>(OnClientProcessing);
            //client.Progress -= new EventHandler<GitProgressEventArgs>(OnClientProgress);
            //client.Cancel -= new EventHandler<CancelEventArgs>(OnClientCancel);
        }

        private void CancelClick(object sender, System.EventArgs e)
        {
            OnCancel(EventArgs.Empty);

            this.args.SetCancelled(true);
            this.cancelButton.Text = "Cancelling...";
            this.cancelButton.Enabled = false;
        }

        private ProgressStatusEventArgs args = new ProgressStatusEventArgs();
    }

    /// <summary>
    /// An event args class used by the ProgressDialog.ProgressStatus event.
    /// </summary>
    public class ProgressStatusEventArgs : EventArgs
    {
        /// <summary>
        /// Event handlers can set this to true if the operation is finished.
        /// </summary>
        public bool Done
        {
            get { return this.done; }
            set { this.done = value; }
        }

        /// <summary>
        /// The dialog uses this to indicate that the user has clicked 
        /// Cancel. Event handlers should detect this and attempt to 
        /// cancel the ongoing operation.
        /// </summary>
        public bool Cancelled
        {
            get { return this.cancelled; }
        }

        internal void SetCancelled(bool val)
        {
            this.cancelled = val;
        }

        private bool done;
        private bool cancelled;
    }
}
