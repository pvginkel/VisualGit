using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI.VSSelectionControls;
using VisualGit.VS;
using System.Windows.Forms;
using VisualGit.UI.PendingChanges.Commits;
using System.Drawing;
using VisualGit.Commands;

namespace VisualGit.UI.PendingChanges.Conflicts
{
    class ConflictListView : ListViewWithSelection<ConflictListItem>
    {
        IVisualGitServiceProvider _context;

        public ConflictListView()
        {
            Initialize();
        }

        void Initialize()
        {
            SmartColumn path = new SmartColumn(this, PCStrings.PathColumn, 288, "Path");
            SmartColumn project = new SmartColumn(this, PCStrings.ProjectColumn, 76, "Project");
            SmartColumn conflictType = new SmartColumn(this, PCStrings.ConflictTypeColumn, 92, "ConflictType");
            SmartColumn conflictDescription = new SmartColumn(this, PCStrings.ConflictDescriptionColumn, 288, "ConflictDescription");

            SmartColumn change = new SmartColumn(this, PCStrings.ChangeColumn, 76, "Change");
            SmartColumn fullPath = new SmartColumn(this, PCStrings.FullPathColumn, 327, "FullPath");

            SmartColumn changeList = new SmartColumn(this, PCStrings.ChangeListColumn, 76, "ChangeList");
            SmartColumn folder = new SmartColumn(this, PCStrings.FolderColumn, 196, "Folder");
            SmartColumn locked = new SmartColumn(this, PCStrings.LockedColumn, 38, "Locked");
            SmartColumn modified = new SmartColumn(this, PCStrings.ModifiedColumn, 76, "Modified");
            SmartColumn name = new SmartColumn(this, PCStrings.NameColumn, 76, "Name");
            SmartColumn type = new SmartColumn(this, PCStrings.TypeColumn, 76, "Type");
            SmartColumn workingCopy = new SmartColumn(this, PCStrings.WorkingCopyColumn, 76, "WorkingCopy");

            Columns.AddRange(new ColumnHeader[]
            {
                path,
                project,
                conflictType,
                conflictDescription,
            });

            modified.Sorter = new SortWrapper(
                delegate(ConflictListItem x, ConflictListItem y)
                {
                    return x.PendingChange.GitItem.Modified.CompareTo(y.PendingChange.GitItem.Modified);
                });

            change.Groupable = true;
            changeList.Groupable = true;
            folder.Groupable = true;
            locked.Groupable = true;
            project.Groupable = true;
            type.Groupable = true;
            workingCopy.Groupable = true;

            path.Hideable = false;

            AllColumns.Add(change);
            AllColumns.Add(changeList);
            AllColumns.Add(conflictType);
            AllColumns.Add(conflictDescription);
            AllColumns.Add(folder);
            AllColumns.Add(fullPath);
            AllColumns.Add(locked);
            AllColumns.Add(modified);
            AllColumns.Add(name);
            AllColumns.Add(path);
            AllColumns.Add(project);
            AllColumns.Add(type);
            AllColumns.Add(workingCopy);

            SortColumns.Add(path);

            FinalSortColumn = path;
        }

        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set
            {
                _context = value;
                SelectionPublishServiceProvider = value;
                if (value != null)
                {
                    IFileIconMapper mapper = value.GetService<IFileIconMapper>();
                    SmallImageList = mapper.ImageList;
                }
            }
        }

        protected override string GetCanonicalName(ConflictListItem item)
        {
            return item.PendingChange.FullPath;
        }

        public override void OnShowContextMenu(MouseEventArgs e)
        {
            base.OnShowContextMenu(e);

            Point p = e.Location;
            bool showSort = false;
            if (p != new Point(-1, -1))
            {
                // Mouse context menu
                if (PointToClient(p).Y < HeaderHeight)
                    showSort = true;
            }
            else
            {
                ListViewItem fi = FocusedItem;

                if (fi != null)
                    p = PointToScreen(fi.Position);
            }

            IVisualGitCommandService mcs = Context.GetService<IVisualGitCommandService>();
            if (mcs != null)
            {
                if (showSort)
                    mcs.ShowContextMenu(VisualGitCommandMenu.PendingCommitsHeaderContextMenu, p);
                else
                    mcs.ShowContextMenu(VisualGitCommandMenu.PendingCommitsContextMenu, p);
            }
        }
    }
}
