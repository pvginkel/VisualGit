// VisualGit.VS\Dialogs\VisualGitDialogOwner.cs
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
