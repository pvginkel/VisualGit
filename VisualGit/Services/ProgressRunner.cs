using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;


using VisualGit.UI;
using VisualGit.VS;
using System.Text;
using System.IO;
using VisualGit.VS.OutputPane;
using System.Runtime.InteropServices;
using SharpGit;

namespace VisualGit
{
    [GlobalService(typeof(IProgressRunner))]
    sealed class ProgressRunnerService : VisualGitService, IProgressRunner
    {
        public ProgressRunnerService(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        public ProgressRunnerResult RunModal(string caption, EventHandler<ProgressWorkerArgs> action)
        {
            return RunModal(caption, new ProgressRunnerArgs(), action);
        }

        public ProgressRunnerResult RunModal(string caption, ProgressRunnerArgs args, EventHandler<ProgressWorkerArgs> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            else if (args == null)
                throw new ArgumentNullException("args");
            else if (string.IsNullOrEmpty(caption))
                caption = VisualGitId.PlkProduct;

            ProgressRunner pr = new ProgressRunner(this, action);
            pr.CreateUpdateReport = args.CreateLog;
            pr.TransportClientArgs = args.TransportClientArgs;
            pr.Start(caption);

            return new ProgressRunnerResult(!pr.Cancelled);
        }

        public void RunNonModal(string caption, EventHandler<ProgressWorkerArgs> action, EventHandler<ProgressWorkerDoneArgs> completer)
        {
            ProgressRunnerResult r = null;
            // Temporary implementation
            try
            {
                r = RunModal(caption, action);
            }
            catch (Exception e)
            {
                r = new ProgressRunnerResult(false, e);
            }
            finally
            {
                if (completer != null)
                    completer(this, new ProgressWorkerDoneArgs(r));
            }
        }

        /// <summary>
        /// Used to run lengthy operations in a separate thread while 
        /// displaying a modal progress dialog in the main thread.
        /// </summary>
        sealed class ProgressRunner
        {
            readonly IVisualGitServiceProvider _context;
            readonly EventHandler<ProgressWorkerArgs> _action;
            Form _invoker;
            bool _cancelled;
            bool _closed;
            Exception _exception;
            bool _updateReport;
            GitTransportClientArgs _transportClientArgs;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProgressRunner"/> class.
            /// </summary>
            /// <param name="context">The context.</param>
            /// <param name="action">The action.</param>
            public ProgressRunner(IVisualGitServiceProvider context, EventHandler<ProgressWorkerArgs> action)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                else if (action == null)
                    throw new ArgumentNullException("action");

                _context = context;
                _action = action;
            }

            public bool CreateUpdateReport
            {
                get { return _updateReport; }
                set { _updateReport = value; }
            }

            public GitTransportClientArgs TransportClientArgs
            {
                get { return _transportClientArgs; }
                set { _transportClientArgs = value; }
            }

            /// <summary>
            /// Whether the operation was cancelled.
            /// </summary>
            public bool Cancelled
            {
                get { return _cancelled; }
            }

            ISynchronizeInvoke _sync;

            /// <summary>
            /// Call this to start the operation.
            /// </summary>
            /// <param name="caption">The caption to use in the progress dialog.</param>
            public void Start(string caption)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(this.Run));
                IGitClientPool pool = _context.GetService<IGitClientPool>();
                IVisualGitDialogOwner dialogOwner = _context.GetService<IVisualGitDialogOwner>();

                ProgressDialogBase dialog;

                if (TransportClientArgs != null)
                {
                    TransportProgressDialog transportDialog = new TransportProgressDialog();
                    transportDialog.ClientArgs = TransportClientArgs;
                    dialog = transportDialog;
                }
                else
                    dialog = new ProgressDialog();

                using (dialog)
                using (GitClient client = pool.GetClient())
                using (CreateUpdateReport ? BindOutputPane(client) : null)
                using (dialog.Bind(client))
                {
                    _sync = dialog;
                    dialog.Caption = caption;
                    dialog.Context = _context;
                    thread.Name = "VisualGit Worker";
                    bool threadStarted = false;

                    dialog.HandleCreated += delegate
                    {
                        if (!threadStarted)
                        {
                            threadStarted = true;
                            thread.Start(client);
                        }
                    };
                    _invoker = dialog;

                    do
                    {
                        if (!_closed)
                        {
                            dialog.ShowDialog(_context);
                        }
                        else
                            Application.DoEvents();

                        // Show the dialog again if the thread join times out
                        // Do this to handle the acase where the service wants to
                        // pop up a dialog before canceling.

                        // BH: Experienced this 2008-09-29 when our repository server
                        //     accepted http connections but didn't handle them in time
                    }
                    while (!thread.Join(2500));
                }
                if (_cancelled)
                {
                    // NOOP
                }
                else if (_exception != null)
                    throw new ProgressRunnerException(this._exception);
            }

            private IDisposable BindOutputPane(GitClient client)
            {
                return new OutputPaneReporter(_context, client);
            }

            private void Run(object arg)
            {
                try
                {
                    ProgressWorkerArgs awa = new ProgressWorkerArgs(_context, (GitClient)arg, _sync);
                    _action(null, awa);

                    if (_exception == null && awa.Exception != null)
                        _exception = awa.Exception;

                    if (_exception is GitOperationCancelledException)
                        _cancelled = true;
                }
                catch (GitOperationCancelledException)
                {
                    _cancelled = true;
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
                finally
                {
                    _closed = true;
                    try
                    {
                        OnDone(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        if (_exception == null)
                            _exception = ex;
                    }
                }
            }

            IVisualGitDialogOwner _dialogOwner;
            IVisualGitDialogOwner DialogOwner
            {
                get { return _dialogOwner ?? (_dialogOwner = _context.GetService<IVisualGitDialogOwner>()); }
            }
            

            IVisualGitConfigurationService _configService;
            IVisualGitConfigurationService ConfigService
            {
                get { return _configService ?? (_configService = _context.GetService<IVisualGitConfigurationService>()); }
            }

            private void OnDone(object sender, EventArgs e)
            {
                Form si = _invoker;

                if (si != null && si.InvokeRequired)
                {
                    EventHandler eh = new EventHandler(OnDone);
                    try
                    {
                        si.BeginInvoke(eh, new object[] { sender, e });
                    }
                    catch(Exception ex)
                    { 
                        /* Not Catching this exception kills VS */
                        GC.KeepAlive(ex);
                    }
                    return;
                }

                if (si.Visible)
                {
                    si.Dispose();

                    if (ConfigService != null && ConfigService.Instance.FlashWindowWhenOperationCompletes)
                    {
                        if (DialogOwner != null)
                        {
                            IWin32Window window = DialogOwner.DialogOwner;
                            NativeMethods.FLASHWINFO fw = new NativeMethods.FLASHWINFO();
                            fw.cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(NativeMethods.FLASHWINFO)));
                            fw.hwnd = window.Handle;
                            fw.dwFlags = (Int32)(NativeMethods.FLASHWINFOFLAGS.FLASHW_ALL | NativeMethods.FLASHWINFOFLAGS.FLASHW_TIMERNOFG);
                            fw.dwTimeout = 0;

                            NativeMethods.FlashWindowEx(ref fw);
                        }
                    }
                }
            }
        }
        
        sealed class OutputPaneReporter : IDisposable
        {
            readonly IOutputPaneManager _mgr;
#if false
            readonly SvnClientReporter _reporter;
#endif
            readonly StringBuilder _sb;

            public OutputPaneReporter(IVisualGitServiceProvider context, GitClient client)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                else if (client == null)
                    throw new ArgumentNullException("client");

                _mgr = context.GetService<IOutputPaneManager>();
                _sb = new StringBuilder();

#if false
                _reporter = new SvnClientReporter(client, _sb);
#endif
            }

            public void Dispose()
            {
                try
                {
                    _sb.AppendLine();
                    _mgr.WriteToPane(_sb.ToString());
                }
                finally
                {
#if false
                    _reporter.Dispose();
#endif
                }
            }
        }
    }


    
    static class NativeMethods
    {
        [Flags]
        public enum FLASHWINFOFLAGS
        {
            FLASHW_STOP = 0,
            FLASHW_CAPTION = 0x00000001,
            FLASHW_TRAY = 0x00000002,
            FLASHW_ALL = (FLASHW_CAPTION | FLASHW_TRAY),
            FLASHW_TIMER = 0x00000004,
            FLASHW_TIMERNOFG = 0x0000000C
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public Int32 dwFlags;
            public UInt32 uCount;
            public Int32 dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pfwi);
    }

    /// <summary>
    /// To be used to wrap exceptions thrown from the other thread.
    /// </summary>
    public class ProgressRunnerException : Exception
    {
        public ProgressRunnerException(Exception realException) :
            base("Exception thrown in progress runner thread", realException)
        { }
    }
}
