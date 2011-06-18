using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

using VisualGit.UI;
using VisualGit.VS;
using VisualGit.Commands;
using System.Text;
using System.Runtime.InteropServices;
using SharpGit;

namespace VisualGit.Services
{
    /// <summary>
    /// Encapsulates error handling functionality.
    /// </summary>
    [GlobalService(typeof(IVisualGitErrorHandler), AllowPreRegistered = true)]
    class VisualGitErrorHandler : VisualGitService, IVisualGitErrorHandler
    {
        const string _errorReportMailAddress = "";
        const string _errorReportSubject = "Exception";
        readonly HandlerDelegator Handler;

        public VisualGitErrorHandler(IVisualGitServiceProvider context)
            : base(context)
        {
            Handler = new HandlerDelegator(this);
        }

        public bool IsEnabled(Exception ex)
        {
            // TODO: Exception processing is currently disabled. This will be
            // replaced by NBug later on.

            return false;
        }

        /// <summary>
        /// Handles an exception.
        /// </summary>
        /// <param name="ex"></param>
        public void OnError(Exception ex)
        {
            if (ex == null)
                return;

            Handler.Invoke(ex, null);
        }

        public void OnError(Exception ex, BaseCommandEventArgs commandArgs)
        {
            if (ex == null)
                return;
            else if (commandArgs == null)
                OnError(ex);
            else
                Handler.Invoke(ex, new ExceptionInfo(commandArgs));
        }

        sealed class ExceptionInfo
        {
            readonly BaseCommandEventArgs _commandArgs;
            public ExceptionInfo(BaseCommandEventArgs e)
            {
                _commandArgs = e;
            }

            public BaseCommandEventArgs CommandArgs
            {
                get { return _commandArgs; }
            }
        }

        sealed class HandlerDelegator : VisualGitService
        {
            VisualGitErrorHandler _handler;
            public HandlerDelegator(VisualGitErrorHandler context)
                : base(context)
            {
                _handler = context;
            }

            IWin32Window Owner
            {
                get { return GetService<IUIService>().GetDialogOwnerWindow(); }
            }

            public void Invoke(Exception ex, ExceptionInfo info)
            {
                try
                {
                    // BH: Uses reflection to find the best match based on the exception??

                    Type t = typeof(HandlerDelegator);
                    MethodInfo method = t.GetMethod("DoHandle", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { ex.GetType(), typeof(ExceptionInfo) }, null);

                    if (method != null)
                        method.Invoke(this, new object[] { ex, info });
                    else
                        DoHandle(ex, info);
                }
                catch (Exception x)
                {
                    Debug.WriteLine(x);
                }
            }

            private void DoHandle(ProgressRunnerException ex, ExceptionInfo info)
            {
                // we're only interested in the inner exception - we know where the 
                // outer one comes from
                Invoke(ex.InnerException, info);
            }

            private void DoHandle(Exception ex, ExceptionInfo info)
            {
                _handler.ShowErrorDialog(ex, true, true, info);
            }
        }

        private void ShowErrorDialog(Exception ex, bool showStackTrace, bool internalError, ExceptionInfo info)
        {
            throw new NotSupportedException();

#if NOT_IMPLEMENTED
            string stackTrace = ex.ToString();
            string message = GetNestedMessages(ex);
            System.Collections.Specialized.StringDictionary additionalInfo =
                new System.Collections.Specialized.StringDictionary();

            IVisualGitSolutionSettings ss = GetService<IVisualGitSolutionSettings>();
            if (ss != null)
                additionalInfo.Add("VS-Version", ss.VisualStudioVersion.ToString());

            if (info != null && info.CommandArgs != null)
                additionalInfo.Add("Command", info.CommandArgs.Command.ToString());

            IVisualGitPackage pkg = GetService<IVisualGitPackage>();
            if (pkg != null)
                additionalInfo.Add("VisualGit-Version", pkg.UIVersion.ToString());

            additionalInfo.Add("SharpSvn-Version", GitClient.SharpGitVersion.ToString());
            additionalInfo.Add("Svn-Version", GitClient.NGitVersion.ToString());
            additionalInfo.Add("OS-Version", Environment.OSVersion.Version.ToString());

            using (ErrorDialog dlg = new ErrorDialog())
            {
                dlg.ErrorMessage = message;
                dlg.ShowStackTrace = showStackTrace;
                dlg.StackTrace = stackTrace;
                dlg.InternalError = internalError;

                if (dlg.ShowDialog(Context) == DialogResult.Retry)
                {
                    string subject = _errorReportSubject;

                    if (info != null && info.CommandArgs != null)
                        subject = string.Format("Error handling {0}", info.CommandArgs.Command);
                    
                    VisualGitErrorMessage.SendByMail(_errorReportMailAddress,
                        subject, ex, typeof(VisualGitErrorHandler).Assembly, additionalInfo);
                }
            }
#endif
        }

        private static string GetNestedMessages(Exception ex)
        {
            StringBuilder sb = new StringBuilder();

            while (ex != null)
            {
                sb.AppendLine(ex.Message.Trim());
                ex = ex.InnerException;
            }

            return sb.ToString();
        }
    }
}
