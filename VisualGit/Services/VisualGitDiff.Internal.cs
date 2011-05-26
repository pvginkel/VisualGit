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
