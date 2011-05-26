using System;
using VisualGit.UI;
using System.Windows.Forms;
using VisualGit.Scc;

namespace VisualGit
{
    /// <summary>
    /// Represents the UI of the addin.
    /// </summary>
    public interface IUIShell
    {
        /// <summary>
        /// Show a "path selector dialog".
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        PathSelectorResult ShowPathSelector(PathSelectorInfo info);
    }
}
