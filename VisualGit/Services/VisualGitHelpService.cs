using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI;
using System.Globalization;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using System.Windows.Forms;

namespace VisualGit.Services
{
    [GlobalService(typeof(IVisualGitDialogHelpService))]
    class VisualGitHelpService : VisualGitService, IVisualGitDialogHelpService
    {
        public VisualGitHelpService(IVisualGitServiceProvider context)
            : base(context)
        {
        }
        #region IVisualGitDialogHelpService Members

        public void RunHelp(VSDialogForm form)
        {
            UriBuilder ub = new UriBuilder("http://svc.ankhsvn.net/svc/go/");
            ub.Query = string.Format("t=dlgHelp&v={0}&l={1}&dt={2}", GetService<IVisualGitPackage>().UIVersion, CultureInfo.CurrentUICulture.LCID, Uri.EscapeUriString(form.DialogHelpTypeName));

            try
            {
                bool showHelpInBrowser = true;
                IVsHelpSystem help = GetService<IVsHelpSystem>(typeof(SVsHelpService));
                if (help != null)
                    showHelpInBrowser = !ErrorHandler.Succeeded(help.DisplayTopicFromURL(ub.Uri.AbsoluteUri, (uint)VHS_COMMAND.VHS_Default));

                if (showHelpInBrowser)
                    Help.ShowHelp(form, ub.Uri.AbsoluteUri);
            }
            catch (Exception ex)
            {
                IVisualGitErrorHandler eh = GetService<IVisualGitErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        #endregion
    }
}
