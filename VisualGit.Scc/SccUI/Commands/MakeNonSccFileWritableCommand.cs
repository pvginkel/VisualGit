// VisualGit.Scc\SccUI\Commands\MakeNonSccFileWritableCommand.cs
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
using System.IO;
using VisualGit.Commands;

namespace VisualGit.Scc.SccUI.Commands
{
    [Command(VisualGitCommand.MakeNonSccFileWriteable, AlwaysAvailable=true)]
    class MakeNonSccFileWritableCommand : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        public void OnExecute(CommandEventArgs e)
        {
            GitItem item = e.Argument as GitItem;
            if (item == null)
                return;

            using(EditReadOnlyFileDialog dialog = new EditReadOnlyFileDialog(item))
            {
                switch(dialog.ShowDialog(e.Context))
                {
                    case DialogResult.Yes:
                        // make writable and allow
                        FileAttributes attr = File.GetAttributes(item.FullPath);
                        File.SetAttributes(item.FullPath, attr & ~FileAttributes.ReadOnly);
                        e.Result = true;
                        break;
                    case DialogResult.No:
                        // Don't make writable but allow
                        e.Result = true;
                        break;
                    default:
                        // Don't make writeable and don't allow
                        e.Result = false;
                        break;
                }
            }
        }

        #region ICommandHandler Members

        #endregion
    }
}
