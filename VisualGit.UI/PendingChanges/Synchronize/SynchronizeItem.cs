using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc;
using System.ComponentModel;
using SharpSvn;

namespace VisualGit.UI.PendingChanges.Synchronize
{
    sealed class SynchronizeItem : VisualGitPropertyGridItem
    {
        readonly IVisualGitServiceProvider _context;
        readonly SynchronizeListItem _listItem;

        GitItem GitItem
        {
            get { return _listItem.GitItem; }
        }

        public SynchronizeItem(IVisualGitServiceProvider context, SynchronizeListItem listItem)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            else if (listItem == null)
                throw new ArgumentNullException("listItem");

            _context = context;
            _listItem = listItem;
        }

        protected override string ClassName
        {
            get { return "Recent Change"; }
        }

        protected override string ComponentName
        {
            get { return GitItem.Name; }
        }

        internal SynchronizeListItem ListItem
        {
            get { return _listItem; }
        }

        [DisplayName("Full Path")]
        public string FullPath
        {
            get { return _listItem.GitItem.FullPath; }
        }

        [DisplayName("File Name")]
        public string Name
        {
            get { return GitItem.Name; }
        }

        [DisplayName("Change List"), Category("Git")]
        public string ChangeList
        {
            get { return GitItem.Status.ChangeList; }
            set
            {
                string cl = string.IsNullOrEmpty(value) ? null : value.Trim();

                if (GitItem.IsVersioned && GitItem.Status != null && GitItem.IsFile)
                {
                    if (value != GitItem.Status.ChangeList)
                    {
                        using (SvnClient client = _context.GetService<IGitClientPool>().GetNoUIClient())
                        {
                            if (cl != null)
                            {
                                SvnAddToChangeListArgs ca = new SvnAddToChangeListArgs();
                                ca.ThrowOnError = false;
                                client.AddToChangeList(GitItem.FullPath, cl);
                            }
                            else
                            {
                                SvnRemoveFromChangeListArgs ca = new SvnRemoveFromChangeListArgs();
                                ca.ThrowOnError = false;
                                client.RemoveFromChangeList(GitItem.FullPath, ca);
                            }
                        }
                    }
                }
            }
        }
    }
}
