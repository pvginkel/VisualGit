// VisualGit\Commands\ExportCommand.cs
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

using System.Windows.Forms;

using VisualGit.UI.Commands;
using System;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to export a Git repository or local folder.
    /// </summary>
    [Command(VisualGitCommand.Export, HideWhenDisabled = false)]
    class ExportCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            GitItem i = EnumTools.GetSingle(e.Selection.GetSelectedGitItems(false));

            if (i == null)
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            using (ExportDialog dlg = new ExportDialog(e.Context))
            {
                dlg.OriginPath = EnumTools.GetSingle(e.Selection.GetSelectedGitItems(false)).FullPath;

                if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                GitDepth depth = dlg.NonRecursive ? GitDepth.Empty : GitDepth.Infinity;

                e.GetService<IProgressRunner>().RunModal(CommandStrings.Exporting,
                    delegate(object sender, ProgressWorkerArgs wa)
                    {
                        GitExportArgs args = new GitExportArgs();

                        args.Depth = depth;
                        args.Revision = dlg.Revision;

                        wa.Client.Export(dlg.ExportSource, dlg.LocalPath, args);
                    });
            }
        }
    }
}
