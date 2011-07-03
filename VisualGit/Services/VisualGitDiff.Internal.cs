// VisualGit\Services\VisualGitDiff.Internal.cs
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
using VisualGit.Scc.UI;
using VisualGit.UI;
using VisualGit.UI.DiffWindow;
using VisualGit.Selection;

namespace VisualGit.Services
{
    partial class VisualGitDiff
    {
        readonly List<int> _freeDiffs = new List<int>();
        int _nNext;
        
        private bool RunInternalDiff(VisualGitDiffArgs args)
        {
            IVisualGitPackage pkg = GetService<IVisualGitPackage>();

            int nWnd;

            if (_freeDiffs.Count > 0)
            {
                nWnd = _freeDiffs[0];
                _freeDiffs.RemoveAt(0);
            }
            else
                nWnd = _nNext++;

            pkg.ShowToolWindow(VisualGitToolWindow.Diff, nWnd, true);

            DiffToolWindowControl twc = GetService<ISelectionContext>().ActiveFrameControl as DiffToolWindowControl;

            if (twc != null)
                twc.Reset(nWnd, args);

            return false;
        }

        void IVisualGitDiffHandler.ReleaseDiff(int frame)
        {
            if(!_freeDiffs.Contains(frame))
                _freeDiffs.Add(frame);
        }
    }
}
