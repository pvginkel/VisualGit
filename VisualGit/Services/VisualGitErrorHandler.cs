#define TEST_ERROR_HANDLING

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
using CrashReporter;

namespace VisualGit.Services
{
    /// <summary>
    /// Encapsulates error handling functionality.
    /// </summary>
    [GlobalService(typeof(IVisualGitErrorHandler), AllowPreRegistered = true)]
    class VisualGitErrorHandler : VisualGitService, IVisualGitErrorHandler
    {
        public VisualGitErrorHandler(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        protected override void OnInitialize()
        {
            Reporter.SetConfiguration(new CrashReporter.Configuration
            {
                AllowComments = true,
                AllowEmailAddress = true,
                AlwaysSubmit = false,
                ApplicationTitle = VisualGitId.PlkProduct,
#if TEST_ERROR_HANDLING
                Application = new Guid("d82540cf-0bbc-4149-bb26-9c212b4ed44b"),
                Url = "http://localhost:8888/submit",
#else
                Application = new Guid("7d62d6b9-893f-471e-a5cb-e5423a836c14"),
                Url = "http://visualgit-bugs.appspot.com/submit",
#endif
                DialogType = ExceptionDialogType.Technical,
                ShowUI = true,
                Version = GetType().Assembly.GetName().Version.ToString()
            });

            Reporter.AddFormatter(new ExceptionFormatter<Exception>(FormatException));
        }

        private string FormatException(Exception ex)
        {
            var sb = new StringBuilder();

            string message = ex.Message.TrimEnd();

            sb.Append(message);

            if (message.Contains("\n"))
                sb.Append("\n");
            else
                sb.Append(" ");

            sb.AppendFormat("({0})", ex.GetType().FullName);

            return sb.ToString();
        }

        public bool IsEnabled(Exception ex)
        {
#if DEBUG && !TEST_ERROR_HANDLING
            return false;
#else
            return true;
#endif
        }

        /// <summary>
        /// Handles an exception.
        /// </summary>
        /// <param name="ex"></param>
        public void OnError(Exception ex)
        {
            if (ex == null)
                return;

            Handle(ex, null);
        }

        public void OnError(Exception ex, BaseCommandEventArgs commandArgs)
        {
            if (ex == null)
                return;
            else if (commandArgs == null)
                OnError(ex);
            else
                Handle(ex, commandArgs);
        }

        public void OnWarning(Exception ex)
        {
            if (ex == null)
                return;

            Handle(ex, null);
        }

        private void Handle(Exception ex, BaseCommandEventArgs exceptionInfo)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");

            // We're not interested in the ProgressRunnerException. Get the inner
            // exception unless we don't have one.

            while (ex is ProgressRunnerException && ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

#if DEBUG && !TEST_ERROR_HANDLING
            GetService<IVisualGitDialogOwner>()
                .MessageBox.Show(
                    ex.Message,
                    CommandResources.OperationNotCompletedSuccessfully,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
#else
            Reporter.Report(ex);
#endif
        }
    }
}
