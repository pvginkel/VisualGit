// VisualGit.UI\VSSelectionControls\Commands\ListViewSortOrder.cs
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
using VisualGit.Commands;

namespace VisualGit.UI.VSSelectionControls.Commands
{
    [Command(VisualGitCommand.ListViewSortAscending, AlwaysAvailable = true, HideWhenDisabled = false)]
    [Command(VisualGitCommand.ListViewSortDescending, AlwaysAvailable = true, HideWhenDisabled = false)]
    class ListViewSortOrder : ListViewCommandBase
    {
        protected override void OnUpdate(SmartListView list, VisualGit.Commands.CommandUpdateEventArgs e)
        {
            bool foundOne = false;

            e.Checked = true;

            foreach (SmartColumn sc in list.SortColumns)
            {
                foundOne = true;

                switch (e.Command)
                {
                    case VisualGitCommand.ListViewSortAscending:
                        if (sc.ReverseSort)
                        {
                            e.Checked = false;
                            return;
                        }
                        break;
                    case VisualGitCommand.ListViewSortDescending:
                        if (!sc.ReverseSort)
                        {
                            e.Checked = false;
                            return;
                        }
                        break;
                }
            }
            if (!foundOne)
            {
                e.Checked = e.Enabled = false;
            }
        }

        protected override void OnExecute(SmartListView list, CommandEventArgs e)
        {
            bool value = (e.Command == VisualGitCommand.ListViewSortDescending);

            foreach (SmartColumn sc in list.SortColumns)
            {
                sc.ReverseSort = value;
            }
            list.UpdateSortGlyphs();
            list.Sort();
        }
    }
}
