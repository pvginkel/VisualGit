using System.Windows.Forms;
using System.ComponentModel;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    public class ControlUtil
    {
        public static bool GetIsInDesignMode(Control control)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return true;

            for (; control != null; control = control.Parent)
            {
                if (control.Site != null && control.Site.DesignMode)
                    return true;
            }

            return false;
        }
    }
}
