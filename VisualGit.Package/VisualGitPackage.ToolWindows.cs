using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using OLEConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using ShellPackage = Microsoft.VisualStudio.Shell.Package;

using VisualGit.Commands;
using VisualGit.Scc.UI;
using VisualGit.UI;
using VisualGit.UI.DiffWindow;
using VisualGit.UI.RepositoryExplorer;
using VisualGit.UI.GitLog;
using VisualGit.UI.WorkingCopyExplorer;

namespace VisualGit.VSPackage
{
    // We define the toolwindows here. We implement them as some kind of
    // .Net control hosted in this container. This container makes sure
    // user settings are persisted, etc.
    [ProvideToolWindow(typeof(WorkingCopyExplorerToolWindow), Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(RepositoryExplorerToolWindow), Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindow(typeof(PendingChangesToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Transient = false, Window = ToolWindowGuids80.Outputwindow)]
    [ProvideToolWindow(typeof(LogToolWindow), Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Transient = true)]
    [ProvideToolWindow(typeof(DiffToolWindow), Style = VsDockStyle.MDI, Transient = true)]
    [ProvideToolWindowVisibility(typeof(PendingChangesToolWindow), VisualGitId.SccProviderId)]
    public partial class VisualGitPackage
    {
        public void ShowToolWindow(VisualGitToolWindow window)
        {
            ShowToolWindow(window, 0, true);
        }

        Type GetPaneType(VisualGitToolWindow toolWindow)
        {
            switch (toolWindow)
            {
                case VisualGitToolWindow.RepositoryExplorer:
                    return typeof(RepositoryExplorerToolWindow);
                case VisualGitToolWindow.WorkingCopyExplorer:
                    return typeof(WorkingCopyExplorerToolWindow);
                case VisualGitToolWindow.PendingChanges:
                    return typeof(PendingChangesToolWindow);
                case VisualGitToolWindow.Log:
                    return typeof(LogToolWindow);
                case VisualGitToolWindow.Diff:
                    return typeof(DiffToolWindow);
                default:
                    throw new ArgumentOutOfRangeException("toolWindow");
            }
        }

        public void ShowToolWindow(VisualGitToolWindow toolWindow, int id, bool create)
        {
            ToolWindowPane pane = FindToolWindow(GetPaneType(toolWindow), id, create);
            
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
            {
                throw new InvalidOperationException("FindToolWindow failed");
            }
            // Bring the tool window to the front and give it focus
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(frame.Show());
        }

        public void CloseToolWindow(VisualGitToolWindow toolWindow, int id, __FRAMECLOSE frameClose)
        {
            ToolWindowPane pane = FindToolWindow(GetPaneType(toolWindow), id, false);
            
            IVsWindowFrame frame = pane.Frame as IVsWindowFrame;
            if (frame == null)
                return;

            ErrorHandler.ThrowOnFailure(frame.CloseFrame((uint) frameClose));
        }

        AmbientProperties _ambientProperties;
        public AmbientProperties AmbientProperties
        {
            get
            {
                if (_ambientProperties == null)
                {
                    IUIService uis = GetService<System.Windows.Forms.Design.IUIService>();
                    _ambientProperties = new AmbientProperties();
                    Font f = (Font)uis.Styles["DialogFont"];

                    _ambientProperties.Font = new Font(f.FontFamily, f.Size);
                }
                return _ambientProperties;
            }
        }
    }

    class VisualGitToolWindowHost : ISite, IVisualGitToolWindowHost, IOleCommandTarget
    {
        readonly List<IOleCommandTarget> _targets = new List<IOleCommandTarget>();
        readonly VisualGitToolWindowPane _pane;
        Container _container;
        string _originalTitle;
        string _title;

        public VisualGitToolWindowHost(VisualGitToolWindowPane pane)
        {
            if (pane == null)
                throw new ArgumentNullException("pane");

            _pane = pane;
        }

        internal void Load()
        {
            _originalTitle = _title = _pane.Caption;
        }
        #region IVisualGitToolWindowSite Members

        IVisualGitPackage _package;
        public IVisualGitPackage Package
        {
            get
            {
                if (_package != null)
                    return _package;

                if (_pane != null && _pane.Package != null)
                    _package = (IVisualGitPackage)_pane.Package;
                else
                    _package = (IVisualGitPackage)ShellPackage.GetGlobalService(typeof(IVisualGitPackage));

                return _package;
            }
        }

        public IVsWindowFrame Frame
        {
            get { return ((IVsWindowFrame)_pane.Frame); }
        }

        public IVsWindowPane Pane
        {
            get { return _pane; }
        }

        public void AddCommandTarget(IOleCommandTarget target)
        {
            if (!_targets.Contains(target))
                _targets.Add(target);
        }

        public string Title
        {
            get { return _title; }
            set { _pane.Caption = _title = value; }
        }

        public string OriginalTitle
        {
            get { return _originalTitle; }
        }

        #endregion

        #region ISite Members

        public System.ComponentModel.IComponent Component
        {
            get { return _pane.Window as IComponent; }
        }

        public System.ComponentModel.IContainer Container
        {
            get { return _container ?? (_container = new Container()); }
        }

        public bool DesignMode
        {
            get { return false; }
        }

        public string Name
        {
            get { return ToString(); }
            set { }
        }

        #endregion

        #region IServiceProvider Members
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(AmbientProperties))
            {
                if (Package != null)
                    return Package.AmbientProperties;
                else
                    return null;
            }

            System.IServiceProvider paneSp = _pane;

            object ob = paneSp.GetService(serviceType);

            if (ob != null)
                return ob;
            else if (Package != null)
                return Package.GetService(serviceType);
            else
                return null;
        }

        #endregion

        #region IVisualGitServiceProvider Members

        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return (T)GetService(typeof(T));
        }

        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return (T)GetService(serviceType);
        }

        #endregion

        IOleCommandTarget _baseTarget;
        #region IOleCommandTarget Members

        CommandMapper _mapper;

        CommandMapper Mapper
        {
            get { return _mapper ?? (_mapper = GetService<CommandMapper>()); }
        }

        VisualGitContext _context;
        VisualGitContext VisualGitContext
        {
            get { return _context ?? (_context = VisualGitContext.Create(this)); }
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            foreach (IOleCommandTarget target in _targets)
            {
                int hr = target.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);

                if (hr != (int)OLEConstants.OLECMDERR_E_NOTSUPPORTED && hr != (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP)
                    return hr;
            }

            IOleCommandTarget t = _baseTarget ?? (_baseTarget = (IOleCommandTarget)_pane.BaseGetService(typeof(IOleCommandTarget)));

            if (t != null)
                return t.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            else
                return (int)OLEConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            foreach (IOleCommandTarget target in _targets)
            {
                int hr = target.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

                if (hr != (int)OLEConstants.OLECMDERR_E_NOTSUPPORTED && hr != (int)OLEConstants.OLECMDERR_E_UNKNOWNGROUP)
                    return hr;
            }

            IOleCommandTarget t = _baseTarget ?? (_baseTarget = (IOleCommandTarget)_pane.BaseGetService(typeof(IOleCommandTarget)));

            int r = (int)OLEConstants.OLECMDERR_E_NOTSUPPORTED;

            if (t != null)
                r = t.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);

            if (ErrorHandler.Succeeded(r))
                return r;
            else
                return (int)OLEConstants.OLECMDERR_E_NOTSUPPORTED;
        }

        #endregion

        #region IVisualGitToolWindowSite Members

        public Guid KeyboardContext
        {
            get { return GetGuid(__VSFPROPID.VSFPROPID_InheritKeyBindings); }
            set { SetGuid(__VSFPROPID.VSFPROPID_InheritKeyBindings, value); }
        }

        public Guid CommandContext
        {
            get { return GetGuid(__VSFPROPID.VSFPROPID_CmdUIGuid); }
            set { SetGuid(__VSFPROPID.VSFPROPID_CmdUIGuid, value); }
        }

        private Guid GetGuid(__VSFPROPID id)
        {
            Guid gResult;
            if (ErrorHandler.Succeeded(Frame.GetGuidProperty((int)id, out gResult)))
                return gResult;
            else
                return Guid.Empty;
        }

        private void SetGuid(__VSFPROPID id, Guid value)
        {
            ErrorHandler.ThrowOnFailure(Frame.SetGuidProperty((int)id, ref value));
        }

        #endregion

        #region IVisualGitToolWindowHost Members


        public bool IsOnScreen
        {
            get
            {
                IVsWindowFrame frame = Frame;
                if (frame != null)
                {
                    int onScreen;

                    if (ErrorHandler.Succeeded(frame.IsOnScreen(out onScreen)) && onScreen != 0)
                        return true;
                }

                return false;
            }
        }

        #endregion
    }

    class VisualGitToolWindowPane : ToolWindowPane, IOleCommandTarget, IVsWindowFrameNotify3, IVsWindowFrameNotify2, IVsWindowFrameNotify
    {
        readonly VisualGitToolWindowHost _host;
        VisualGitToolWindowControl _control;
        IVisualGitToolWindowControl _twControl;
        VisualGitToolWindow _toolWindow;

        protected VisualGitToolWindowPane()
            : base(null)
        {
            _host = new VisualGitToolWindowHost(this);
        }

        public VisualGitToolWindow VisualGitToolWindow
        {
            get { return _toolWindow; }
            protected set
            {
                _toolWindow = value;
                BitmapResourceID = 401;
                BitmapIndex = (int)_toolWindow;
            }
        }

        protected VisualGitToolWindowControl Control
        {
            get { return _control; }
            set
            {
                Debug.Assert(_control == null);
                _control = value;
                _twControl = (IVisualGitToolWindowControl)value;
            }
        }

        bool _created;
        public override IWin32Window Window
        {
            get
            {
                if (!_created)
                {
                    _created = true;
                    if (!_control.IsHandleCreated)
                    {
                        Size sz = _control.Size;
                        _control.Location = new Point(-15000, -15000); // Far, far away
                        _control.Size = new Size(0, 0); // And just 1 pixel

                        _control.Visible = true; // If .Visible = false no window is created!
                        _control.CreateControl();
                        _control.Visible = false; // And hide the window now or we hijack the focus. See issue #507
                        _control.Size = sz;
                    }
                }
                return _control;
            }
        }

        [DebuggerStepThrough]
        protected T GetService<T>()
            where T : class
        {
            return GetService(typeof(T)) as T;
        }

        protected override object GetService(Type serviceType)
        {
            if (serviceType == typeof(IOleCommandTarget))
                return _host;
            else
                return base.GetService(serviceType);
        }

        internal object BaseGetService(Type serviceType)
        {
            return base.GetService(serviceType);
        }

        protected override void OnCreate()
        {
            _host.Load();
            //Control.Site = _host;
            Control.ToolWindowHost = _host;
            base.OnCreate();
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
        }

        public override void OnToolBarAdded()
        {
            base.OnToolBarAdded();

            _twControl.OnFrameCreated(EventArgs.Empty);
        }

        #region IVsWindowFrameNotify* Members

        public int OnClose(ref uint pgrfSaveOptions)
        {
            _twControl.OnFrameClose(EventArgs.Empty);

            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable, int x, int y, int w, int h)
        {
            _twControl.OnFrameDockableChanged(new FrameEventArgs(fDockable != 0, new Rectangle(x, y, w, h), (__FRAMESHOW)0));

            return VSConstants.S_OK;
        }

        public int OnMove(int x, int y, int w, int h)
        {
            _twControl.OnFrameMove(new FrameEventArgs(false, new Rectangle(x, y, w, h), (__FRAMESHOW)0));

            return VSConstants.S_OK;
        }

        public int OnShow(int fShow)
        {
            _twControl.OnFrameShow(new FrameEventArgs(false, Rectangle.Empty, (__FRAMESHOW)fShow));

            return VSConstants.S_OK;
        }

        public int OnSize(int x, int y, int w, int h)
        {
            _twControl.OnFrameSize(new FrameEventArgs(false, new Rectangle(x, y, w, h), (__FRAMESHOW)0));

            return VSConstants.S_OK;
        }

        #endregion

        #region IVsWindowFrameNotify Members

        public int OnDockableChange(int fDockable)
        {
            return OnDockableChange(fDockable, 0, 0, 0, 0);
        }

        public int OnMove()
        {
            return OnMove(0, 0, 0, 0);
        }

        public int OnSize()
        {
            return OnSize(0, 0, 0, 0);
        }

        #endregion
    }

    /// <summary>
    /// Wrapper for the WorkingCopyExplorer in the VisualGit assembly
    /// </summary>
    [Guid(VisualGitId.WorkingCopyExplorerToolWindowId)]
    class WorkingCopyExplorerToolWindow : VisualGitToolWindowPane
    {
        public WorkingCopyExplorerToolWindow()
        {
            Caption = Resources.WorkingCopyExplorerToolWindowTitle;
            Control = new WorkingCopyExplorerControl();

            VisualGitToolWindow = VisualGitToolWindow.WorkingCopyExplorer;

            ToolBar = new CommandID(VisualGitId.CommandSetGuid, (int)VisualGitCommandMenu.WorkingCopyExplorerToolBar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }

    /// <summary>
    /// Wrapper for the RepositoryExplorer in the VisualGit assembly
    /// </summary>
    [Guid(VisualGitId.RepositoryExplorerToolWindowId)]
    class RepositoryExplorerToolWindow : VisualGitToolWindowPane
    {
        public RepositoryExplorerToolWindow()
        {
            Caption = Resources.RepositoryExplorerToolWindowTitle;
            Control = new RepositoryExplorerControl();

            VisualGitToolWindow = VisualGitToolWindow.RepositoryExplorer;

            ToolBar = new CommandID(VisualGitId.CommandSetGuid, (int)VisualGitCommandMenu.RepositoryExplorerToolBar);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }

    /// <summary>
    /// Wrapper for the Commit dialog in the VisualGit assembly
    /// </summary>
    [Guid(VisualGitId.PendingChangesToolWindowId)]
    class PendingChangesToolWindow : VisualGitToolWindowPane
    {
        public PendingChangesToolWindow()
        {
            Caption = Resources.PendingChangesToolWindowTitle;
            Control = new VisualGit.UI.PendingChanges.PendingChangesToolControl();

            VisualGitToolWindow = VisualGitToolWindow.PendingChanges;

            ToolBar = new CommandID(VisualGitId.CommandSetGuid, (int)VisualGitToolBar.PendingChanges);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }

    [Guid(VisualGitId.LogToolWindowId)]
    class LogToolWindow : VisualGitToolWindowPane
    {
        public LogToolWindow()
        {
            Caption = Resources.LogToolWindowTitle;
            Control = new LogToolWindowControl();

            VisualGitToolWindow = VisualGitToolWindow.Log;

            ToolBar = new CommandID(VisualGitId.CommandSetGuid, (int)VisualGitToolBar.LogViewer);
            ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }

    [Guid(VisualGitId.DiffToolWindowId)]
    class DiffToolWindow : VisualGitToolWindowPane
    {
        public DiffToolWindow()
        {
            Caption = Resources.DiffToolWindowTitle;
            Control = new DiffToolWindowControl();

            VisualGitToolWindow = VisualGitToolWindow.Diff;
        }
    }
}
