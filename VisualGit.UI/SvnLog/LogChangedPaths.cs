using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.Scc;
using VisualGit.Commands;

namespace VisualGit.UI.SvnLog
{
    partial class LogChangedPaths : UserControl, ICurrentItemDestination<ISvnLogItem>
    {
        public LogChangedPaths()
        {
            InitializeComponent();
        }

        public LogChangedPaths(IContainer container)
            : this()
        {
            container.Add(this);
        }

        LogDataSource _dataSource;
        public LogDataSource LogSource
        {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        IVisualGitServiceProvider _context;
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set
            {
                _context = value;
                changedPaths.SelectionPublishServiceProvider = value;
            }
        }

        #region ICurrentItemDestination<ISvnLogItem> Members
        ICurrentItemSource<ISvnLogItem> itemSource;
        public ICurrentItemSource<ISvnLogItem> ItemSource
        {
            get { return itemSource; }
            set
            {
                if (itemSource != null)
                {
                    itemSource.SelectionChanged -= new EventHandler<CurrentItemEventArgs<ISvnLogItem>>(SelectionChanged);
                    itemSource.FocusChanged -= new EventHandler<CurrentItemEventArgs<ISvnLogItem>>(FocusChanged);
                }
                itemSource = value;
                if (itemSource != null)
                {
                    itemSource.SelectionChanged += new EventHandler<CurrentItemEventArgs<ISvnLogItem>>(SelectionChanged);
                    itemSource.FocusChanged += new EventHandler<CurrentItemEventArgs<ISvnLogItem>>(FocusChanged);
                }

            }
        }

        #endregion



        void SelectionChanged(object sender, CurrentItemEventArgs<ISvnLogItem> e)
        {
        }

        void FocusChanged(object sender, CurrentItemEventArgs<ISvnLogItem> e)
        {
            changedPaths.Items.Clear();

            ISvnLogItem item = e.Source.FocusedItem;

            if (item != null && item.ChangedPaths != null)
            {
                List<PathListViewItem> paths = new List<PathListViewItem>();

                List<string> origins = new List<string>();
                foreach (SvnOrigin o in LogSource.Targets)
                {
                    string origin = SvnTools.UriPartToPath(o.RepositoryRoot.MakeRelativeUri(o.Uri).ToString()).Replace('\\', '/');
                    if (origin.Length == 0 || origin[0] != '/')
                        origin = "/" + origin;

                    origins.Add(origin.TrimEnd('/'));
                }

                foreach (SvnChangeItem i in item.ChangedPaths)
                {
                    paths.Add(new PathListViewItem(changedPaths, item, i, item.RepositoryRoot, HasFocus(origins, i.Path)));
                }

                changedPaths.Items.AddRange(paths.ToArray());
            }
        }

        static bool HasFocus(IEnumerable<string> originPaths, string itemPath)
        {
            foreach (string origin in originPaths)
            {
                if (!itemPath.StartsWith(origin))
                    continue;

                int n = itemPath.Length - origin.Length;

                if (n == 0)
                    return true;

                if (n > 0)
                {
                    if (itemPath[origin.Length] == '/')
                        return true;
                }
            }

            return false;
        }

        internal void Reset()
        {
            changedPaths.Items.Clear();
        }



        private void changedPaths_ShowContextMenu(object sender, MouseEventArgs e)
        {
            if (Context == null)
                return;

            Point screen;
            bool isHeaderContextMenu = false;

            if (e.X == -1 && e.Y == -1)
            {
                if (changedPaths.SelectedItems.Count > 0)
                {
                    screen = changedPaths.PointToScreen(changedPaths.SelectedItems[changedPaths.SelectedItems.Count - 1].Position);
                }
                else
                {
                    screen = changedPaths.PointToScreen(new Point(1, 1));
                    isHeaderContextMenu = true;
                }
            }
            else
            {
                isHeaderContextMenu = changedPaths.PointToClient(e.Location).Y < changedPaths.HeaderHeight;
                screen = e.Location;
            }

            IVisualGitCommandService cs = Context.GetService<IVisualGitCommandService>();
            cs.ShowContextMenu(isHeaderContextMenu ? VisualGitCommandMenu.ListViewHeader : VisualGitCommandMenu.LogChangedPathsContextMenu, screen);
        }

        private void changedPaths_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point mp = changedPaths.PointToClient(MousePosition);
            ListViewHitTestInfo info = changedPaths.HitTest(mp);
            PathListViewItem lvi = info.Item as PathListViewItem;
            if (lvi != null && Context != null)
            {
                IVisualGitCommandService cmdSvc = Context.GetService<IVisualGitCommandService>();
                cmdSvc.PostExecCommand(VisualGitCommand.LogShowChanges);
            }
        }
    }
}
