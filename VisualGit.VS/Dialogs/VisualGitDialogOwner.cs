using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

using VisualGit.UI;

namespace VisualGit.VS.Dialogs
{
    [GlobalService(typeof(IVisualGitDialogOwner))]
    sealed class VisualGitDialogOwner : VisualGitService, IVisualGitDialogOwner
    {
        IVsUIShell _shell;
        IUIService _uiService;

        public VisualGitDialogOwner(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        IVsUIShell Shell
        {
            get { return _shell ?? (_shell = GetService<IVsUIShell>(typeof(SVsUIShell))); }
        }

        IUIService UIService
        {
            get { return _uiService ?? (_uiService = GetService<IUIService>()); }
        }

        #region IVisualGitDialogOwner Members

        public IWin32Window DialogOwner
        {
            get
            {
                if (UIService != null)
                    return UIService.GetDialogOwnerWindow();
                else
                    return null;
            }
        }

        #region IVisualGitDialogOwner Members

        public IDisposable InstallFormRouting(VisualGit.UI.VSContainerForm container, EventArgs eventArgs)
        {
            return new VSCommandRouting(Context, container);
        }

        public void OnContainerCreated(VisualGit.UI.VSContainerForm form)
        {
            VSCommandRouting routing = VSCommandRouting.FromForm(form);

            if (routing != null)
                routing.OnHandleCreated();
        }
        #endregion

        #endregion

        #region IVisualGitDialogOwner Members

        public VisualGitMessageBox MessageBox
        {
            get { return new VisualGitMessageBox(this); }
        }

        #endregion

        #region IVisualGitDialogOwner Members


        public void AddCommandTarget(VisualGit.UI.VSContainerForm form, IOleCommandTarget commandTarget)
        {
            VSCommandRouting routing = VSCommandRouting.FromForm(form);

            if (routing != null)
                routing.AddCommandTarget(commandTarget);
            else
                throw new InvalidOperationException("Command routing not initialized yet");
        }

        public void AddWindowPane(VisualGit.UI.VSContainerForm form, IVsWindowPane pane)
        {
            VSCommandRouting routing = VSCommandRouting.FromForm(form);

            if (routing != null)
                routing.AddWindowPane(pane);
            else
                throw new InvalidOperationException("Command routing not initialized yet");
        }

        #endregion
        
    }
}
