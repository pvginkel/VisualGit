// VisualGit\Services\VisualGitHelpService.cs
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
            throw new NotSupportedException("Help should not be retrieved from ankhsvn.net");
            
            /*
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
            */
        }

        #endregion
    }
}
