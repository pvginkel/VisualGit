// VisualGit.UI\GitLog\LogMessageView.cs
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

using System.ComponentModel;
using System.Windows.Forms;
using VisualGit.Scc;

namespace VisualGit.UI.GitLog
{
    public partial class LogMessageView : UserControl
    {
        ICurrentItemSource<IGitLogItem> logItemSource;

        public LogMessageView()
        {
            InitializeComponent();
        }

        public LogMessageView(IContainer container)
            : this()
        {
            container.Add(this);
        }

        public ICurrentItemSource<IGitLogItem> ItemSource
        {
            get { return logItemSource; }
            set 
            { 
                if(logItemSource != null)
                    logItemSource.FocusChanged -= LogFocusChanged;

                logItemSource = value; 
                
                if(logItemSource != null)
                    logItemSource.FocusChanged += LogFocusChanged;
            }
        }

        void LogFocusChanged(object sender, CurrentItemEventArgs<IGitLogItem> e)
        {
            if (ItemSource != null && ItemSource.FocusedItem != null)
                logMessageEditor.Text = logItemSource.FocusedItem.LogMessage;
            else
                logMessageEditor.Text = "";
        }

        internal void Reset()
        {
            logMessageEditor.Text = "";
        }
    }
}
