// VisualGit.VS\Dialogs\VSFormContainerPane.cs
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
using Microsoft.VisualStudio.Shell;
using VisualGit.UI;
using System.Windows.Forms;

namespace VisualGit.VS.Dialogs
{
    sealed class VSFormContainerPane : WindowPane
    {
        readonly VSCommandRouting _routing;
        readonly Panel _panel;

        public VSFormContainerPane(IVisualGitServiceProvider context, VSCommandRouting routing, Panel panel)
            : base(context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (routing == null)
                throw new ArgumentNullException("routing");
            else if (panel == null)
                throw new ArgumentNullException("panel");

            _routing = routing;
            _panel = panel;
        }

        public override System.Windows.Forms.IWin32Window Window
        {
            get { return _panel; }
        }

        protected override bool PreProcessMessage(ref Message m)
        {
            return base.PreProcessMessage(ref m);
        }
    }
}
