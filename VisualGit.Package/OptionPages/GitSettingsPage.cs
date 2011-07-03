// VisualGit.Package\OptionPages\GitSettingsPage.cs
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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using VisualGit.UI;
using VisualGit.UI.OptionsPages;

namespace VisualGit.VSPackage
{
    [Guid(VisualGitId.GitSettingsPageGuid)]
    class GitSettingsPage : DialogPage
    {
        GitSettingsControl _control;
        protected override System.Windows.Forms.IWin32Window Window
        {
            get
            {
                return Control;
            }
        }

        GitSettingsControl Control
        {
            get
            {
                return _control ?? (_control = CreateControl());
            }
        }

        GitSettingsControl CreateControl()
        {
            GitSettingsControl control = new GitSettingsControl();
            IVisualGitServiceProvider sp = (IVisualGitServiceProvider)GetService(typeof(IVisualGitServiceProvider));

            if (sp != null)
                control.Context = sp;

            return control;
        }

        public override void LoadSettingsFromStorage()
        {
            base.LoadSettingsFromStorage();

            Control.LoadSettings();
        }

        public override void SaveSettingsToStorage()
        {
            base.SaveSettingsToStorage();

            Control.SaveSettings();

        }

        public override void ResetSettings()
        {
            base.ResetSettings();

            IVisualGitServiceProvider sp = (IVisualGitServiceProvider)GetService(typeof(IVisualGitServiceProvider));
            if (sp != null)
            {
                IVisualGitConfigurationService cfgSvc = sp.GetService<IVisualGitConfigurationService>();
                cfgSvc.LoadDefaultConfig();
            }
        }
    }
}
