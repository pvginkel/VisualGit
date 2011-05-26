using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.IO;

using SharpSvn;

using VisualGit.UI;
using VisualGit.VS;
using VisualGit.UI.SccManagement;
using VisualGit.UI.PathSelector;

namespace VisualGit
{
    /// <summary>
    /// Summary description for UIShell.
    /// </summary>
    [GlobalService(typeof(IUIShell))]
    sealed class UIShell : VisualGitService, IUIShell
    {
        public UIShell(IVisualGitServiceProvider context)
            : base(context)
        {

        }

        public PathSelectorResult ShowPathSelector(PathSelectorInfo info)
        {
            using (PathSelector selector = new PathSelector(info))
            {
                selector.Context = Context;

                bool succeeded = selector.ShowDialog(Context) == DialogResult.OK;
                PathSelectorResult result = new PathSelectorResult(succeeded, selector.CheckedItems);
                result.Depth = selector.Recursive ? SvnDepth.Infinity : SvnDepth.Empty;
                result.RevisionStart = selector.RevisionStart;
                result.RevisionEnd = selector.RevisionEnd;
                return result;
            }
        }

    }
}
