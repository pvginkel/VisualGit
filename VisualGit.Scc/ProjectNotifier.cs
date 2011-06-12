using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using SharpSvn;

using VisualGit.Commands;
using VisualGit.Selection;
using VisualGit.UI;
using SharpGit;

namespace VisualGit.Scc
{
    [GlobalService(typeof(IFileStatusMonitor))]
    sealed class ProjectNotifier : VisualGitService, IFileStatusMonitor, IVsBroadcastMessageEvents
    {
        readonly object _lock = new object();
        bool _posted;
        bool _onIdle;
        List<GitProject> _dirtyProjects;
        HybridCollection<string> _maybeAdd;
        uint _cookie;

        public ProjectNotifier(IVisualGitServiceProvider context)
            : base(context)
        {
            uint cookie;
            if (ErrorHandler.Succeeded(context.GetService<IVsShell>(typeof(SVsShell)).AdviseBroadcastMessages(this, out cookie)))
                _cookie = cookie;
        }

        IVisualGitCommandService _commandService;
        /// <summary>
        /// Gets the command service.
        /// </summary>
        /// <value>The command service.</value>
        IVisualGitCommandService CommandService
        {
            [DebuggerStepThrough]
            get { return _commandService ?? (_commandService = GetService<IVisualGitCommandService>()); }
        }

        IFileStatusCache _statusCache;
        IFileStatusCache Cache
        {
            [DebuggerStepThrough]
            get { return _statusCache ?? (_statusCache = GetService<IFileStatusCache>()); }
        }

        IProjectFileMapper _mapper;
        IProjectFileMapper Mapper
        {
            [DebuggerStepThrough]
            get { return _mapper ?? (_mapper = GetService<IProjectFileMapper>()); }
        }

        PendingChangeManager _changeManager;
        PendingChangeManager ChangeManager
        {
            [DebuggerStepThrough]
            get { return _changeManager ?? (_changeManager = GetService<PendingChangeManager>(typeof(IPendingChangesManager))); }
        }

        IVisualGitOpenDocumentTracker _tracker;
        IVisualGitOpenDocumentTracker DocumentTracker
        {
            [DebuggerStepThrough]
            get { return _tracker ?? (_tracker = GetService<IVisualGitOpenDocumentTracker>()); }
        }

        ISelectionContextEx _selection;
        ISelectionContextEx Selection
        {
            [DebuggerStepThrough]
            get { return _selection ?? (_selection = GetService<ISelectionContextEx>(typeof(ISelectionContext))); }
        }

        VisualGitSccProvider _sccProvider;
        VisualGitSccProvider SccProvider
        {
            [DebuggerStepThrough]
            get { return _sccProvider ?? (_sccProvider = GetService<VisualGitSccProvider>(typeof(IVisualGitSccService))); }
        }

        void PostDirty(bool checkDelay)
        {
            if (!_posted)
            {
                if (checkDelay)
                    Selection.MaybeInstallDelayHandler();

                CommandService.PostTickCommand(ref _posted, VisualGitCommand.MarkProjectDirty);
            }
        }

        void PostIdle()
        {
            if (!_onIdle)
            {
                CommandService.PostIdleCommand(VisualGitCommand.MarkProjectDirty);
                _onIdle = true;
            }
        }

        /// <summary>
        /// Schedules a glyph refresh of all specified projects
        /// </summary>
        /// <param name="projects"></param>
        public void ScheduleGlyphOnlyUpdate(IEnumerable<GitProject> projects)
        {
            if (projects == null)
                throw new ArgumentNullException("projects");

            lock (_lock)
            {
                if (_dirtyProjects == null)
                    _dirtyProjects = new List<GitProject>();

                foreach (GitProject project in projects)
                {
                    if (!_dirtyProjects.Contains(project))
                        _dirtyProjects.Add(project);
                }

                PostDirty(false);
            }
        }


        public void ScheduleAddFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            lock (_lock)
            {
                if (_maybeAdd == null)
                    _maybeAdd = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

                if (!_maybeAdd.Contains(path))
                    _maybeAdd.Add(path);

                PostDirty(true);
            }
        }

        HybridCollection<string> _dirtyCheck = null;

        /// <summary>
        /// Schedules a dirty check for the specified document
        /// </summary>
        /// <param name="path">The path.</param>
        public void ScheduleDirtyCheck(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            GitItem item = Cache[path];

            if (!item.IsVersioned || item.IsModified)
                return; // Not needed

            lock (_lock)
            {
                if (_dirtyCheck == null)
                    _dirtyCheck = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

                if (!_dirtyCheck.Contains(path))
                    _dirtyCheck.Add(path);

                PostIdle();
            }
        }

        /// <summary>
        /// Schedules a dirty check for the specified documents.
        /// </summary>
        /// <param name="paths">The paths.</param>
        public void ScheduleDirtyCheck(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            lock (_lock)
            {
                if (_dirtyCheck == null)
                    _dirtyCheck = new HybridCollection<string>(StringComparer.OrdinalIgnoreCase);

                _dirtyCheck.UniqueAddRange(paths);

                PostIdle();
            }
        }

        internal void HandleEvent(VisualGitCommand command)
        {
            List<GitProject> dirtyProjects;
            HybridCollection<string> dirtyCheck;
            HybridCollection<string> maybeAdd;

            VisualGitSccProvider provider = Context.GetService<VisualGitSccProvider>();

            lock (_lock)
            {
                _posted = false;
                _onIdle = false;

                if (provider == null)
                    return;

                dirtyProjects = _dirtyProjects;
                dirtyCheck = _dirtyCheck;
                maybeAdd = _maybeAdd;
                _dirtyProjects = null;
                _dirtyCheck = null;
                _maybeAdd = null;
            }

            if (dirtyCheck != null)
                foreach (string file in dirtyCheck)
                {
                    DocumentTracker.CheckDirty(file);
                }

            if (dirtyProjects != null)
            {
                foreach (GitProject project in dirtyProjects)
                {
                    if (project.RawHandle == null)
                    {
                        if (project.IsSolution)
                            provider.UpdateSolutionGlyph();

                        continue; // All IVsSccProjects have a RawHandle
                    }

                    try
                    {
                        project.RawHandle.SccGlyphChanged(0, null, null, null);
                    }
                    catch { }
                }
            }

            if (maybeAdd != null)
            {
                using (SvnClient cl = GetService<ISvnClientPool>().GetNoUIClient())
                {
                    foreach (string file in maybeAdd)
                    {
                        GitItem item = Cache[file];
                        // Only add
                        // * files
                        // * that are unversioned
                        // * that are addable
                        // * that are not ignored
                        // * and just to be sure: that are still part of the solution
                        if (item.IsFile && !item.IsVersioned &&
                            item.IsVersionable && !item.IsIgnored &&
                            item.InSolution)
                        {
                            SvnAddArgs aa = new SvnAddArgs();
                            aa.ThrowOnError = false; // Just ignore errors here; make the user add them themselves
                            aa.AddParents = true;

                            cl.Add(item.FullPath, aa);

                            // Detect if we have a file that Git might detect as binary
                            if (!item.IsTextFile)
                            {
                                // Only check small files, avoid checking big binary files
                                FileInfo fi = new FileInfo(item.FullPath);
                                if (fi.Length < 10)
                                {
                                    // We're sure it's at most 10 bytes here, so just read all
                                    byte[] fileBytes = File.ReadAllBytes(item.FullPath);

                                    // If the file starts with a UTF8 BOM, we're sure enough it's a text file, keep UTF16 & 32 binary
                                    if (StartsWith(fileBytes, new byte[] {0xEF, 0xBB, 0xBF}))
                                    {
                                        // Delete the mime type property, so it's detected as a text file again
                                        cl.DeleteProperty(item.FullPath, SvnPropertyNames.SvnMimeType);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static bool StartsWith(byte[] haystack, byte[] needle)
        {
            if (haystack == null)
                return false;
            if (needle == null)
                return false;
            if (needle.Length > haystack.Length)
                return false;

            for (int i = 0; i < needle.Length; i++)
            {
                if (haystack[i] != needle[i])
                    return false;
            }

            return true;
        }

        public void ScheduleGitStatus(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            Cache.MarkDirty(path);

            ScheduleGlyphUpdate(path);
        }

        public void ScheduleGitStatus(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            Cache.MarkDirty(paths);

            ScheduleGlyphUpdate(paths);
        }

        public void HandleGitResult(IDictionary<string, GitClientAction> actions)
        {
            List<GitClientAction> sccRefreshItems = null;
            ScheduleMonitor(actions.Keys);

            ScheduleGitStatus(actions.Keys);

            foreach (GitClientAction action in actions.Values)
            {
                if (action.Recursive)
                {
                    foreach (GitItem item in Cache.GetCachedBelow(action.FullPath))
                    {
                        item.MarkDirty();
                        ScheduleGlyphUpdate(item.FullPath);
                    }
                }

                if (action.AddOrRemove)
                {
                    if(sccRefreshItems == null)
                        sccRefreshItems = new List<GitClientAction>();

                    sccRefreshItems.Add(action);
                }
            }

            if (sccRefreshItems != null)
                SccProvider.ScheduleGitRefresh(sccRefreshItems);
        }

        public void ScheduleGlyphUpdate(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            ScheduleGlyphOnlyUpdate(Mapper.GetAllProjectsContaining(path));
            ChangeManager.Refresh(path);
        }

        public void ScheduleGlyphUpdate(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            ScheduleGlyphOnlyUpdate(Mapper.GetAllProjectsContaining(paths));
            ChangeManager.Refresh(paths);
        }

        public void ScheduleMonitor(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            ChangeManager.ScheduleMonitor(path);
        }

        public void ScheduleMonitor(IEnumerable<string> paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            ChangeManager.ScheduleMonitor(paths);
        }

        public void StopMonitoring(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            ChangeManager.StopMonitor(path);
        }

        readonly Dictionary<string, DocumentLock> _externallyChanged = new Dictionary<string, DocumentLock>(StringComparer.OrdinalIgnoreCase);

        public void ExternallyChanged(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            ScheduleGitStatus(path);

            lock (_externallyChanged)
            {
                if (!_externallyChanged.ContainsKey(path))
                {
                    _externallyChanged.Add(path, null);
                    if (DocumentTracker.IsDocumentOpenInTextEditor(path) && !DocumentTracker.IsDocumentDirty(path, true))
                    {
                        // Locking will trigger a file change!
                        _externallyChanged[path] = DocumentTracker.LockDocument(path, DocumentLockType.ReadOnly);
                    }
                }
            }
        }

        private void ReleaseExternalWrites()
        {
            Dictionary<string, DocumentLock> modified;
            lock (_externallyChanged)
            {
                if (_externallyChanged.Count == 0)
                    return;

                modified = new Dictionary<string, DocumentLock>(_externallyChanged, StringComparer.OrdinalIgnoreCase);
                _externallyChanged.Clear();
            }

            try
            {
                foreach (KeyValuePair<string, DocumentLock> file in modified)
                {
                    ScheduleGitStatus(file.Key);
                    GitItem item = Cache[file.Key];

                    if (item.IsConflicted)
                    {
                        VisualGitMessageBox mb = new VisualGitMessageBox(Context);

                        DialogResult dr = mb.Show(string.Format(Resources.YourMergeToolSavedXWouldYouLikeItMarkedAsResolved, file.Key),
                            Resources.MergeCompleted, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);

                        switch (dr)
                        {
                            case DialogResult.Yes:
                                using (GitClient c = Context.GetService<IGitClientPool>().GetNoUIClient())
                                {
                                    GitResolveArgs ra = new GitResolveArgs();
                                    ra.ThrowOnError = false;

                                    c.Resolve(file.Key, GitAccept.Merged, ra);
                                }
                                goto case DialogResult.No;
                            case DialogResult.No:
                                if (!item.IsModified)
                                {
                                    // Reload?
                                }
                                break;
                            default:
                                // Let VS handle the file
                                return; // No reload
                        }
                    }

                    if (!item.IsDocumentDirty)
                    {
                        if (file.Value != null)
                            file.Value.Reload(file.Key);
                    }
                }
            }
            catch (Exception ex)
            {
                IVisualGitErrorHandler eh = GetService<IVisualGitErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
            finally
            {
                foreach (DocumentLock dl in modified.Values)
                {
                    if (dl != null)
                        dl.Dispose();
                }
            }
        }

        #region IVsBroadcastMessageEvents Members

        const uint WM_ACTIVATE = 0x0006;
        const uint WM_ACTIVATEAPP = 0x001C;

        int IVsBroadcastMessageEvents.OnBroadcastMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_ACTIVATEAPP:
                    if (wParam != IntPtr.Zero)
                        ReleaseExternalWrites();
                    break;
                case VSConstants.VSM_ENTERMODAL:
                case VSConstants.VSM_EXITMODAL:
                case VSConstants.VSM_TOOLBARMETRICSCHANGE:
                    break;

            }
            return VSConstants.S_OK;
        }

        #endregion
    }
}
