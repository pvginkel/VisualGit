// VisualGit\Services\VisualGitClientPool.cs
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
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using SharpGit;
using VisualGit.Scc;
using VisualGit.UI;
using VisualGit.VS;

namespace VisualGit.Services
{
    [GlobalService(typeof(IGitClientPool))]
    sealed class VisualGitClientPool : VisualGitService, IGitClientPool
    {
        readonly Stack<GitPoolClient> _clients = new Stack<GitPoolClient>();
        readonly Stack<GitPoolClient> _uiClients = new Stack<GitPoolClient>();
        readonly Control _syncher;
        const int MaxPoolSize = 10;
        int _returnCookie;

        public VisualGitClientPool(IVisualGitServiceProvider context)
            : base(context)
        {
            _syncher = new Control();
            _syncher.Visible = false;
            _syncher.Text = "VisualGit Synchronizer";
            GC.KeepAlive(_syncher.Handle); // Ensure the window is created
        }

        IVisualGitDialogOwner _dialogOwner;
        IVisualGitDialogOwner DialogOwner
        {
            get { return _dialogOwner ?? (_dialogOwner = GetService<IVisualGitDialogOwner>()); }
        }

        IFileStatusCache _cache;
        IFileStatusCache StatusCache
        {
            get { return _cache ?? (_cache = GetService<IFileStatusCache>()); }
        }

        IFileStatusMonitor _monitor;
        IFileStatusMonitor StatusMonitor
        {
            get { return _monitor ?? (_monitor = GetService<IFileStatusMonitor>()); }
        }

        bool _ensuredNames;
        void EnsureNames()
        {
            if (_ensuredNames)
                return;
            _ensuredNames = true;
        }

        public GitPoolClient GetClient()
        {
            lock (_uiClients)
            {
                if (_uiClients.Count > 0)
                    return _uiClients.Pop();

                return CreateClient(true);
            }
        }

        public GitPoolClient GetNoUIClient()
        {
            lock (_clients)
            {
                if (_clients.Count > 0)
                    return _clients.Pop();

                return CreateClient(false);
            }
        }

        private GitPoolClient CreateClient(bool hookUI)
        {
            EnsureNames();

            if (DialogOwner == null)
                hookUI = false;

            VisualGitPoolClient client = new VisualGitPoolClient(this, hookUI, _returnCookie);

            if (hookUI)
                HookUI(client);

            return client;
        }

        // Use separate function to delay loading the SharpGit.UI.dll
        private void HookUI(VisualGitPoolClient client)
        {
            // Let SharpGitUI handle login and SSL dialogs
            GitUIBindArgs bindArgs = new GitUIBindArgs();
            bindArgs.ParentWindow = new OwnerWrapper(DialogOwner);
            bindArgs.UIService = GetService<IUIService>();
            bindArgs.Synchronizer = _syncher;

            GitUI.Bind(client, bindArgs);
        }

        internal void NotifyChanges(IDictionary<string, GitClientAction> actions)
        {
            StatusMonitor.HandleGitResult(actions);
        }

        public bool ReturnClient(GitPoolClient poolClient)
        {
            VisualGitPoolClient pc = poolClient as VisualGitPoolClient;

            if (pc != null && pc.ReturnCookie == _returnCookie)
            {
                Stack<GitPoolClient> stack = pc.UIEnabled ? _uiClients : _clients;

                lock (stack)
                {
                    if (stack.Count < MaxPoolSize)
                    {
                        stack.Push(pc);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Flushes all clients to read settings again
        /// </summary>
        public void FlushAllClients()
        {
            _returnCookie++;

            List<IDisposable> toDispose = new List<IDisposable>();

            lock (_uiClients)
            {
                while (_uiClients.Count > 0)
                    toDispose.Add(_uiClients.Pop());
            }

            lock (_clients)
            {
                while (_clients.Count > 0)
                    toDispose.Add(_clients.Pop());
            }

            foreach (IDisposable d in toDispose)
            {
                d.Dispose();
            }
        }

        sealed class VisualGitPoolClient : GitPoolClient
        {
            readonly SortedDictionary<string, GitClientAction> _changes = new SortedDictionary<string, GitClientAction>(StringComparer.OrdinalIgnoreCase);
            readonly bool _uiEnabled;
            readonly int _returnCookie;
            public VisualGitPoolClient(VisualGitClientPool pool, bool uiEnabled, int returnCookie)
                : base(pool)
            {
                _uiEnabled = uiEnabled;
                _returnCookie = returnCookie;
            }

            public bool UIEnabled
            {
                get { return _uiEnabled; }
            }

            protected override void OnNotify(GitNotifyEventArgs e)
            {
                base.OnNotify(e);

                string path = e.FullPath;

                if (string.IsNullOrEmpty(path))
                    return;

                GitClientAction action;
                if (!_changes.TryGetValue(path, out action))
                    _changes.Add(path, action = new GitClientAction(path));

                switch (e.Action)
                {
                    case GitNotifyAction.CommitDeleted:
                    case GitNotifyAction.Revert:
                    case GitNotifyAction.TreeConflict:
                        action.Recursive = true;
                        break;
                    case GitNotifyAction.UpdateDelete:
                        action.Recursive = true;
                        action.AddOrRemove = true;
                        break;
                    case GitNotifyAction.UpdateReplace:
                    case GitNotifyAction.UpdateAdd:
                        action.AddOrRemove = true;
                        break;
                    case GitNotifyAction.UpdateUpdate:
                        // action.OldRevision = e.OldRevision;
                        break;
                }
            }

            protected override void OnCommitting(GitCommittingEventArgs e)
            {
                base.OnCommitting(e);

                if (e.CurrentCommandType != GitCommandType.Commit)
                    return;

                foreach (GitCommitItem item in e.Items)
                {
                    string fp = item.FullPath;

                    if (fp == null) // Non local operation
                        return;

                    GitClientAction action;

                    if (!_changes.TryGetValue(fp, out action))
                        _changes.Add(fp, action = new GitClientAction(fp));
                }
            }

            protected override void ReturnClient()
            {
                VisualGitClientPool pool = (VisualGitClientPool)GitClientPool;
                GitClientPool = null;

                if (pool == null)
                {
                    Debug.Assert(false, "Returning pool client a second time");
                    return;
                }

                try
                {
                    if (_changes.Count > 0)
                        pool.NotifyChanges(_changes);
                }
                finally
                {
                    _changes.Clear();
                }

                if (base.IsCommandRunning || base.IsDisposed)
                {
                    Debug.Assert(!IsCommandRunning, "Returning pool client while it is running");
                    Debug.Assert(!IsDisposed, "Returning pool client while it is disposed");

                    return; // No return on these errors.. Leave it to the GC to clean it up eventually
                }
                else if (!pool.ReturnClient(this))
                    InnerDispose(); // The pool wants to get rid of us
                else
                    GitClientPool = pool; // Reinstated
            }

            public int ReturnCookie
            {
                get { return _returnCookie; }
            }
        }
    }

    sealed class OwnerWrapper : IWin32Window
    {
        IVisualGitDialogOwner _owner;

        public OwnerWrapper(IVisualGitDialogOwner owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        public IntPtr Handle
        {
            get
            {
                IWin32Window window = _owner.DialogOwner;

                if (window != null)
                {
                    ISynchronizeInvoke invoker = window as ISynchronizeInvoke;

                    if (invoker != null && invoker.InvokeRequired && Control.CheckForIllegalCrossThreadCalls)
                    {
                        Control.CheckForIllegalCrossThreadCalls = false;
                        try
                        {
                            return window.Handle;
                        }
                        finally
                        {
                            Control.CheckForIllegalCrossThreadCalls = true;
                        }
                    }
                    else
                        return window.Handle;
                }
                else
                    return IntPtr.Zero;
            }
        }
    }
}
