using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;
using VisualGit.VS;
using System.Runtime.InteropServices;

namespace VisualGit.UI
{
    public partial class TransportProgressDialog : ProgressDialogBase
    {
        GitTransportClientArgs _clientArgs;
        bool _canceling;
        static readonly object _syncLock = new object();
        readonly Dictionary<string, CredentialCacheItem> _runCache = new Dictionary<string, CredentialCacheItem>();

        public TransportProgressDialog()
        {
            InitializeComponent();
        }

        public GitTransportClientArgs ClientArgs
        {
            get { return _clientArgs; }
            set { _clientArgs = value; }
        }

        public override IDisposable Bind(GitClient client)
        {
            if (_clientArgs == null)
                throw new InvalidOperationException();

            _clientArgs.Credentials += new EventHandler<GitCredentialsEventArgs>(_clientArgs_Credentials);
            _clientArgs.CredentialsSupported += new EventHandler<GitCredentialsEventArgs>(_clientArgs_CredentialsSupported);
            _clientArgs.Progress += new EventHandler<GitProgressEventArgs>(_clientArgs_Progress);

            return new UnbindDisposable(this);
        }

        private void Unbind()
        {
            _clientArgs.Credentials -= new EventHandler<GitCredentialsEventArgs>(_clientArgs_Credentials);
            _clientArgs.CredentialsSupported -= new EventHandler<GitCredentialsEventArgs>(_clientArgs_CredentialsSupported);
            _clientArgs.Progress -= new EventHandler<GitProgressEventArgs>(_clientArgs_Progress);

            // If everything went OK, the credentials were accepted and we
            // should re-use them for the next run.

            if (_clientArgs.LastException == null)
            {
                lock (_syncLock)
                {
                    foreach (var item in _runCache)
                    {
                        ConfigurationService.StoreCredentialCacheItem(item.Value);
                    }
                }
            }
        }

        void _clientArgs_Progress(object sender, GitProgressEventArgs e)
        {
            e.Cancel = _canceling;

            BeginInvoke(
                new Action<string, int, int>(UpdateProgress),
                e.CurrentTask, e.TaskLength, e.TaskProgress
            );
        }

        private void UpdateProgress(string currentTask, int taskLength, int taskProgress)
        {
            if (IsDisposed)
                return;

            if (taskLength <= 0 || taskProgress < 0)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                if (progressBar.Style != ProgressBarStyle.Continuous)
                    progressBar.Style = ProgressBarStyle.Continuous;

                if (progressBar.Maximum != taskLength)
                    progressBar.Maximum = taskLength;

                progressBar.Value = Math.Min(taskProgress, taskLength);
            }

            progressLabel.Text = currentTask;
        }

        void _clientArgs_CredentialsSupported(object sender, GitCredentialsEventArgs e)
        {
            Invoke(
                new Action<GitCredentialsEventArgs, bool>(ProcessCredentials),
                e, true
            );
        }

        void _clientArgs_Credentials(object sender, GitCredentialsEventArgs e)
        {
            Invoke(
                new Action<GitCredentialsEventArgs, bool>(ProcessCredentials),
                e, false
            );
        }

        private void ProcessCredentials(GitCredentialsEventArgs e, bool checkSupports)
        {
            if (checkSupports)
            {
                foreach (var item in e.Items)
                {
                    switch (item.Type)
                    {
                        case GitCredentialsType.Informational:
                        case GitCredentialsType.Password:
                        case GitCredentialsType.String:
                        case GitCredentialsType.Username:
                        case GitCredentialsType.YesNo:
                            break;

                        default:
                            e.Cancel = true;
                            break;
                    }
                }
            }
            else
            {
                // First, see whether:
                // * We are processing a username/password combination;
                // * We can already process the information and yes/no types;
                // * All credentials are already present in cache.

                GitCredentialItem usernameItem = null;
                GitCredentialItem passwordItem = null;
                bool oneMissing = false;

                foreach (var item in e.Items)
                {
                    if (!ProcessCached(e.Uri, item))
                    {
                        oneMissing = true;

                        switch (item.Type)
                        {
                            case GitCredentialsType.Username:
                                usernameItem = item;
                                break;

                            case GitCredentialsType.Password:
                                passwordItem = item;
                                break;

                            case GitCredentialsType.Informational:
                                if (!ShowInformation(item))
                                    _canceling = true;
                                break;

                            case GitCredentialsType.YesNo:
                                if (!ShowYesNo(item))
                                    _canceling = true;
                                break;
                        }

                        if (_canceling)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                if (!oneMissing)
                    return;

                // When we have both a username and password, we display a
                // combined dialog for this one.

                bool hadUsernamePassword = usernameItem != null && passwordItem != null;
                bool rememberPassword = false;

                if (hadUsernamePassword)
                {
                    string description = String.Format(Properties.Resources.TheServerXRequiresAUsernameAndPassword, e.Uri);

                    if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(5, 1))
                    {
                        // If Windows XP/Windows 2003 or higher: Use the windows password dialog
                        GetUserNamePasswordWindows(e, description, usernameItem, passwordItem, ref rememberPassword);

                        if (e.Cancel)
                            return;
                    }
                    else
                    {
                        using (UsernamePasswordCredentialsDialog dialog = new UsernamePasswordCredentialsDialog())
                        {
                            dialog.UsernameItem = usernameItem;
                            dialog.PasswordItem = passwordItem;
                            dialog.RememberPassword = rememberPassword;

                            if (dialog.ShowDialog(Context, this) != DialogResult.OK)
                            {
                                e.Cancel = true;
                                return;
                            }

                            rememberPassword = dialog.RememberPassword;
                        }
                    }
                }

                // Process the rest. These are processed in a generic dialog.
                // Only when the type is Username, will the dialog not be
                // displayed as a password dialog.

                foreach (var item in e.Items)
                {
                    switch (item.Type)
                    {
                        case GitCredentialsType.Informational:
                        case GitCredentialsType.YesNo:
                            // Already processed these above.
                            break;

                        case GitCredentialsType.Username:
                        case GitCredentialsType.Password:
                            rememberPassword = item.Type == GitCredentialsType.Password;

                            if (!hadUsernamePassword)
                            {
                                if (!ShowGeneric(item, ref rememberPassword))
                                    _canceling = true;
                            }
                            break;

                        default:
                            rememberPassword = item.Type != GitCredentialsType.Username;

                            if (!ShowGeneric(item, ref rememberPassword))
                                _canceling = true;
                            break;
                    }

                    if (_canceling)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                if (rememberPassword)
                {
                    foreach (var item in e.Items)
                    {
                        UpdateCache(e.Uri, item);
                    }
                }
            }
        }

        private void GetUserNamePasswordWindows(GitCredentialsEventArgs e, string description, GitCredentialItem usernameItem, GitCredentialItem passwordItem, ref bool rememberPassword)
        {
            NativeMethods.CREDUI_INFO info = new NativeMethods.CREDUI_INFO();
            info.pszCaptionText = Properties.Resources.ConnectToGit;
            info.pszMessageText = description;
            info.hwndParent = IntPtr.Zero;
            info.cbSize = Marshal.SizeOf(typeof(NativeMethods.CREDUI_INFO));

            StringBuilder sbUserName = new StringBuilder("", 1024);
            StringBuilder sbPassword = new StringBuilder("", 1024);

            var flags =
                NativeMethods.CREDUI_FLAGS.GENERIC_CREDENTIALS |
                NativeMethods.CREDUI_FLAGS.ALWAYS_SHOW_UI |
                NativeMethods.CREDUI_FLAGS.DO_NOT_PERSIST |
                NativeMethods.CREDUI_FLAGS.SHOW_SAVE_CHECK_BOX;

            var result = NativeMethods.CredUIPromptForCredentials(
                ref info, e.Uri, IntPtr.Zero, 0, sbUserName, 1024,
                sbPassword, 1024, ref rememberPassword, flags
            );

            switch (result)
            {
                case NativeMethods.CredUIReturnCodes.NO_ERROR:
                    usernameItem.Value = sbUserName.ToString();
                    passwordItem.Value = sbPassword.ToString();
                    break;

                case NativeMethods.CredUIReturnCodes.ERROR_CANCELLED:
                    usernameItem.Value = null;
                    passwordItem.Value = null;
                    e.Cancel = true;
                    break;
            }
        }

        private bool ProcessCached(string uri, GitCredentialItem item)
        {
            lock (_syncLock)
            {
                switch (item.Type)
                {
                    case GitCredentialsType.Password:
                    case GitCredentialsType.String:
                    case GitCredentialsType.Username:
                        CredentialCacheItem result;

                        if (_runCache.TryGetValue(GetCacheKey(uri, item), out result))
                        {
                            item.Value = result.Response;
                            return true;
                        }
                        else
                        {
                            var cacheItem = ConfigurationService.GetCredentialCacheItem(
                                uri, item.Type.ToString(), item.PromptText
                            );

                            if (cacheItem != null)
                            {
                                item.Value = cacheItem.Response;
                                return true;
                            }
                        }

                        break;
                }
            }

            return false;
        }

        private void UpdateCache(string uri, GitCredentialItem item)
        {
            _runCache[GetCacheKey(uri, item)] = new CredentialCacheItem(
                uri, item.Type.ToString(), item.PromptText, item.Value
            );
        }

        private string GetCacheKey(string uri, GitCredentialItem item)
        {
            return (uri ?? "") + ":" + item.Type + ":" + (item.PromptText ?? "");
        }

        private bool ShowGeneric(GitCredentialItem item, ref bool rememberPassword)
        {
            using (var dialog = new GenericCredentialsDialog())
            {
                dialog.Item = item;
                dialog.RememberPassword = rememberPassword;

                bool result = dialog.ShowDialog(Context, this) == DialogResult.OK;

                if (result)
                    rememberPassword = dialog.RememberPassword;

                return result;
            }
        }

        private bool ShowYesNo(GitCredentialItem item)
        {
            var result = Context.GetService<IVisualGitDialogOwner>()
                .MessageBox.Show(item.PromptText,
                "Credentials", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            item.YesNoValue = result == DialogResult.Yes;

            return result != DialogResult.Cancel;
        }

        private bool ShowInformation(GitCredentialItem item)
        {
            Context.GetService<IVisualGitDialogOwner>()
                .MessageBox.Show(item.PromptText,
                "Credentials", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _canceling = true;
            base.OnClosing(e);
            e.Cancel = true;
        }

        private void CancelClick(object sender, System.EventArgs e)
        {
            _canceling = true;

            OnCancel(EventArgs.Empty);

            cancelButton.Text = "Cancelling...";
            cancelButton.Enabled = false;
        }

        private class UnbindDisposable : IDisposable
        {
            private bool _disposed;
            private TransportProgressDialog _dialog;

            public UnbindDisposable(TransportProgressDialog dialog)
            {
                _dialog = dialog;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _dialog.Unbind();

                    _disposed = true;
                }
            }
        }
    }
}
