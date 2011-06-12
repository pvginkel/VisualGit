using System.Drawing;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal static class Settings
    {
        static Settings()
        {
            MulticolorBranches = true;
            GraphColor = Color.DarkRed;
            BranchBorders = true;
            RevisionGraphLayout = RevisionGridLayout.SmallWithGraph;
            RelativeDate = true;
            RemoteBranchColor = Color.Green;
            OtherTagColor = Color.Gray;
            TagColor = Color.DarkBlue;
            BranchColor = Color.DarkRed;
            ShowRemoteBranches = true;
            RevisionGraphDrawNonRelativesGray = true;
        }

        public static bool RevisionGraphDrawNonRelativesGray { get; set; }
        public static bool MulticolorBranches { get; set; }
        public static Color GraphColor { get; set; }
        public static bool BranchBorders { get; set; }
        public static bool StripedBranchChange { get; set; }
        public static bool ShowAuthorDate { get; set; }
        public static RevisionGridLayout RevisionGraphLayout { get; set; }
        public static bool RevisionGraphDrawNonRelativesTextGray { get; set; }
        public static Color TagColor { get; set; }
        public static Color BranchColor { get; set; }
        public static Color RemoteBranchColor { get; set; }
        public static Color OtherTagColor { get; set; }
        public static bool RelativeDate { get; set; }
        public static bool ShowRemoteBranches { get; set; }
    }
}
