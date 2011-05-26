using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using SharpSvn;
using SharpSvn.Remote;
using SharpSvn.UI;

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
        readonly List<GitPoolRemoteSession> _remoteSessions = new List<GitPoolRemoteSession>();
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

            SvnClient.AddClientName("VisualStudio", GetService<IVisualGitSolutionSettings>().VisualStudioVersion);
            SvnClient.AddClientName("VisualGit", GetService<IVisualGitPackage>().UIVersion);
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

        public SvnWorkingCopyClient GetWcClient()
        {
            return new SvnWorkingCopyClient();
        }

        private GitPoolClient CreateClient(bool hookUI)
        {
            EnsureNames();

            if (DialogOwner == null)
                hookUI = false;

            VisualGitGitPoolClient client = new VisualGitGitPoolClient(this, hookUI, _returnCookie);

            if (hookUI)
                HookUI(client);

            return client;
        }

        // Use separate function to delay loading the SharpSvn.UI.dll
        private void HookUI(VisualGitGitPoolClient client)
        {
            // Let SharpSvnUI handle login and SSL dialogs
            SvnUIBindArgs bindArgs = new SvnUIBindArgs();
            bindArgs.ParentWindow = new OwnerWrapper(DialogOwner);
            bindArgs.UIService = GetService<IUIService>();
            bindArgs.Synchronizer = _syncher;

            SvnUI.Bind(client, bindArgs);
        }

		private void HookUI(VisualGitGitPoolRemoteSession client)
		{
			// Let SharpSvnUI handle login and SSL dialogs
			SvnUIBindArgs bindArgs = new SvnUIBindArgs();
			bindArgs.ParentWindow = new OwnerWrapper(DialogOwner);
			bindArgs.UIService = GetService<IUIService>();
			bindArgs.Synchronizer = _syncher;

			SvnUI.Bind(client, bindArgs);
		}

        internal void NotifyChanges(IDictionary<string, GitClientAction> actions)
        {
            StatusMonitor.HandleGitResult(actions);
        }

        public bool ReturnClient(GitPoolClient poolClient)
        {
            VisualGitGitPoolClient pc = poolClient as VisualGitGitPoolClient;

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

            lock (_remoteSessions)
            {
                foreach (GitPoolRemoteSession rs in _remoteSessions)
                    toDispose.Add(rs);
                _remoteSessions.Clear();
            }

            foreach (IDisposable d in toDispose)
            {
                d.Dispose();
            }
        }

        #region IGitClientPool Members


        public GitPoolRemoteSession GetRemoteSession(Uri sessionUri, bool parentOk)
        {
            if (sessionUri == null)
                throw new ArgumentNullException("sessionUri");

            GitPoolRemoteSession reuse = null;
            lock (_remoteSessions)
            {
                if (_remoteSessions.Count > 0)
                {
                    foreach (GitPoolRemoteSession rs in _remoteSessions)
                    {
                        if (rs.SessionUri == sessionUri)
                        {
                            reuse = rs;
                            _remoteSessions.Remove(rs);
                            break;
                        }
                    }

                    if (reuse == null)
                    {
                        string schemeAndServer = sessionUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped);
                        foreach (GitPoolRemoteSession rs in _remoteSessions)
                        {
                            Uri reposUri = rs.RepositoryRootUri ?? rs.SessionUri;

                            if (reposUri == null || reposUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped) != schemeAndServer)
                                continue;

                            if (sessionUri.AbsolutePath.StartsWith(reposUri.AbsolutePath, StringComparison.Ordinal))
                            {
                                reuse = rs;
                            }
                        }
                    }

                    if (reuse != null)
                        _remoteSessions.Remove(reuse);
                }
            }

            if (reuse != null)
            {
                if (sessionUri != reuse.SessionUri)
                {
                    if (!parentOk || !sessionUri.AbsolutePath.StartsWith(reuse.SessionUri.AbsolutePath))
                    {
                        SvnRemoteCommonArgs rca = new SvnRemoteCommonArgs();
                        rca.ThrowOnError = false;

                        if (!reuse.Reparent(sessionUri, rca))
                            reuse = null;
                    }
                }

                if (reuse != null)
                    return reuse;

                // else -> GC will cleanup
            }

            VisualGitGitPoolRemoteSession session = new VisualGitGitPoolRemoteSession(this, true, _returnCookie);
            HookUI(session);
            session.Open(sessionUri);
            return session;
        }

        public bool ReturnClient(GitPoolRemoteSession session)
        {
            VisualGitGitPoolRemoteSession pc = session as VisualGitGitPoolRemoteSession;

            if (pc != null && pc.ReturnCookie == _returnCookie && pc.SessionUri != null)
            {
                pc.ReturnTime = DateTime.Now;

                lock (_remoteSessions)
                {
                    _remoteSessions.Insert(0, pc);

                    ScheduleDisposeSessions();
                    return true;
                }
            }
            return false;
        }

        IVisualGitScheduler _scheduler;
        IVisualGitScheduler Scheduler
        {
            get { return _scheduler ?? (_scheduler = GetService<IVisualGitScheduler>()); }
        }


        DateTime? nextCleanup;
        private void ScheduleDisposeSessions()
        {
            if (nextCleanup.HasValue)
            {
                TimeSpan ts = nextCleanup.Value - DateTime.Now;

                if (ts.Minutes < 5 && ts.Minutes > -1)
                    return;
            }

            DateTime nextTime = DateTime.Now + new TimeSpan(0, 2, 0);
            nextCleanup = nextTime;

            Scheduler.ScheduleAt(nextTime, OnCleanup);
        }

        void OnCleanup()
        {
            List<GitPoolRemoteSession> toDispose = null;
            bool left = false;
            lock (_remoteSessions)
            {
                DateTime now = DateTime.Now;

                foreach (VisualGitGitPoolRemoteSession rs in _remoteSessions)
                {
                    bool dispose = false;
                    switch (rs.SessionUri.Scheme)
                    {
                        case "svn":
                            dispose = (now - rs.ReturnTime) > new TimeSpan(0, 3, 0);
                            break;
                        case "https":
                        case "http":
                            dispose = (now - rs.ReturnTime) > new TimeSpan(0, 8, 0);
                            break;
                        default:
                            dispose = (now - rs.ReturnTime) > new TimeSpan(0, 1, 0);
                            break;
                    }

                    if (!dispose)
                        left = true;
                    else
                    {
                        if (toDispose == null)
                            toDispose = new List<GitPoolRemoteSession>();

                        toDispose.Add(rs);
                    }
                }

                if (toDispose != null)
                    foreach (GitPoolRemoteSession rs in toDispose)
                        _remoteSessions.Remove(rs);

                if (left)
                    ScheduleDisposeSessions();
            }

            if (toDispose != null)
                foreach (VisualGitGitPoolRemoteSession rs in toDispose)
                {
                    try
                    {
                        rs.DisposeSession();
                    }
                    catch
                    { }
                }
        }

        #endregion

        sealed class VisualGitGitPoolClient : GitPoolClient
        {
            readonly SortedDictionary<string, GitClientAction> _changes = new SortedDictionary<string, GitClientAction>(StringComparer.OrdinalIgnoreCase);
            readonly bool _uiEnabled;
            readonly int _returnCookie;
            public VisualGitGitPoolClient(VisualGitClientPool pool, bool uiEnabled, int returnCookie)
                : base(pool)
            {
                _uiEnabled = uiEnabled;
                _returnCookie = returnCookie;
            }

            public bool UIEnabled
            {
                get { return _uiEnabled; }
            }

            protected override void OnNotify(SvnNotifyEventArgs e)
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
                    case SvnNotifyAction.CommitDeleted:
                    case SvnNotifyAction.Revert:
                    case SvnNotifyAction.TreeConflict:
                        action.Recursive = true;
                        break;
                    case SvnNotifyAction.UpdateDelete:
                        action.Recursive = true;
                        action.AddOrRemove = true;
                        break;
                    case SvnNotifyAction.UpdateReplace:
                    case SvnNotifyAction.UpdateAdd:
                        action.AddOrRemove = true;
                        break;
                    case SvnNotifyAction.UpdateUpdate:
                        action.OldRevision = e.OldRevision;
                        break;
                }
            }

            protected override void OnCommitting(SvnCommittingEventArgs e)
            {
                base.OnCommitting(e);

                if (e.CurrentCommandType != SvnCommandType.Commit)
                    return;

                foreach (SvnCommitItem item in e.Items)
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

        sealed class VisualGitGitPoolRemoteSession : GitPoolRemoteSession
        {
            readonly bool _uiEnabled;
            readonly int _returnCookie;
            DateTime _returnTime;
            public VisualGitGitPoolRemoteSession(VisualGitClientPool pool, bool uiEnabled, int returnCookie)
                : base(pool)
            {
                _uiEnabled = uiEnabled;
                _returnCookie = returnCookie;
            }

            public bool UIEnabled
            {
                get { return _uiEnabled; }
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

            public DateTime ReturnTime
            {
                get { return _returnTime.ToLocalTime(); }
                set { _returnTime = value.ToUniversalTime(); }
            }

            /// <summary>
            /// Really dispose the session (.Dispose() returns to the pool)
            /// </summary>
            internal void DisposeSession()
            {
                InnerDispose();
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
