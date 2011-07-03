// VisualGit.UI\OptionsPages\GitSettingsControl.cs
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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpGit;

namespace VisualGit.UI.OptionsPages
{
    public partial class GitSettingsControl : VisualGitOptionsPageControl
    {
        public GitSettingsControl()
        {
            InitializeComponent();
        }

        private void authenticationEdit_Click(object sender, EventArgs e)
        {
            using (GitAuthenticationCacheEditor editor = new GitAuthenticationCacheEditor())
            {
                editor.ShowDialog(Context);
            }
        }

        protected override void SaveSettingsCore()
        {
            using (GitPoolClient client = Context.GetService<IGitClientPool>().GetNoUIClient())
            {
                IGitConfig config = client.GetUserConfig();

                if (String.IsNullOrEmpty(nameBox.Text))
                    config.Unset("user", null, "name");
                else
                    config.SetString("user", null, "name", nameBox.Text);

                if (String.IsNullOrEmpty(emailBox.Text))
                    config.Unset("user", null, "email");
                else
                    config.SetString("user", null, "email", emailBox.Text);

                config.SetString("core", null, "autocrlf", ((NewlineSetting)newlineBox.SelectedItem).Code);
                
                config.Save();
            }
        }

        protected override void LoadSettingsCore()
        {
            using (GitPoolClient client = Context.GetService<IGitClientPool>().GetNoUIClient())
            {
                IGitConfig config = client.GetUserConfig();

                nameBox.Text = config.GetString("user", null, "name");
                emailBox.Text = config.GetString("user", null, "email");

                newlineBox.Items.Clear();

                string newlineSetting = config.GetString("core", null, "autocrlf");

                foreach (NewlineSetting item in NewlineSetting.All)
                {
                    newlineBox.Items.Add(item);

                    if (String.Equals(item.Code, newlineSetting, StringComparison.OrdinalIgnoreCase))
                        newlineBox.SelectedItem = item;
                }

                if (newlineBox.SelectedItem == null)
                    newlineBox.SelectedItem = NewlineSetting.Default;
            }
        }

        private class NewlineSetting
        {
            public static readonly NewlineSetting Yes = new NewlineSetting("Convert to Windows newline", "true");
            public static readonly NewlineSetting No = new NewlineSetting("Leave newlines unchanged", "false");
            public static readonly NewlineSetting Input = new NewlineSetting("Only convert incoming files", "input");
            public static readonly NewlineSetting[] All = new NewlineSetting[] { Yes, No, Input };
            public static readonly NewlineSetting Default = No;

            public string Code { get; private set; }

            private readonly string _label;

            public NewlineSetting(string label, string code)
            {
                Code = code;
                _label = label;
            }

            public override string ToString()
            {
                return _label;
            }
        }
    }
}
