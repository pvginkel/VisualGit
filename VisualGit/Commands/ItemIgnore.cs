using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.UI;
using SharpSvn;
using VisualGit.Scc;
using System.IO;
using SharpGit;
using System.Collections;

namespace VisualGit.Commands
{
    [Command(VisualGitCommand.ItemIgnoreFile)]
    [Command(VisualGitCommand.ItemIgnoreFileType)]
    [Command(VisualGitCommand.ItemIgnoreFilesInFolder)]
    [Command(VisualGitCommand.ItemIgnoreFolder)]
    class ItemIgnore : CommandBase
    {
        static bool Skip(GitItem item)
        {
            return (item.IsVersioned || item.IsIgnored || !item.IsVersionable || !item.Exists);
        }

        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            GitItem foundOne = null;

            foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
            {
                if (Skip(item))
                    continue;

                GitItem parent;

                switch (e.Command)
                {
                    case VisualGitCommand.ItemIgnoreFileType:
                        if (string.IsNullOrEmpty(item.Extension))
                            continue;
                        goto case VisualGitCommand.ItemIgnoreFile;
                    case VisualGitCommand.ItemIgnoreFile:
                    case VisualGitCommand.ItemIgnoreFilesInFolder:
                        parent = item.Parent;
                        if (parent == null || !parent.IsVersioned)
                            continue;
                        break;
                    case VisualGitCommand.ItemIgnoreFolder:
                        parent = item.Parent;
                        if (parent != null && parent.IsVersioned)
                        {
                            e.Enabled = false;
                            return;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (foundOne == null)
                    foundOne = item;
            }

            if (foundOne == null)
            {
                e.Enabled = false;
                return;
            }

            if (e.TextQueryType == TextQueryType.Name)
                switch (e.Command)
                {
                    case VisualGitCommand.ItemIgnoreFile:
                        e.Text = string.Format(foundOne.IsDirectory 
                            ? CommandStrings.IgnoreFolder : CommandStrings.IgnoreFile, foundOne.Name);
                        break;
                    case VisualGitCommand.ItemIgnoreFileType:
                        e.Text = string.Format(CommandStrings.IgnoreFileType, foundOne.Extension);
                        break;
                    case VisualGitCommand.ItemIgnoreFolder:
                        GitItem pp;
                        GitItem p = foundOne.Parent;

                        while (p != null && (pp = p.Parent) != null && !pp.IsVersioned)
                            p = pp;

                        e.Text = string.Format(CommandStrings.IgnoreFolder, (p != null) ? p.Name : "");
                        break;
                }
        }

        public override void OnExecute(CommandEventArgs e)
        {
            Dictionary<string, List<string>> add = new Dictionary<string, List<string>>(FileSystemUtil.StringComparer);
            List<string> refresh = new List<string>();

            foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
            {
                if (Skip(i))
                    continue;
                refresh.Add(i.FullPath);
                switch (e.Command)
                {
                    case VisualGitCommand.ItemIgnoreFile:
                        AddIgnore(add, i.Parent, "/" + i.Name);
                        break;
                    case VisualGitCommand.ItemIgnoreFileType:
                        AddIgnore(add, i.Parent, "/*" + i.Extension);
                        break;
                    case VisualGitCommand.ItemIgnoreFilesInFolder:
                        AddIgnore(add, i.Parent, "/*");
                        break;
                    case VisualGitCommand.ItemIgnoreFolder:
                        GitItem p = i.Parent;
                        GitItem pp = null;

                        while (null != p && null != (pp = p.Parent) && !pp.IsVersioned)
                            p = pp;

                        if (p != null && pp != null)
                            AddIgnore(add, pp, "/" + p.Name + "/");
                        break;
                }
            }

            try
            {

                VisualGitMessageBox mb = new VisualGitMessageBox(e.Context);
                foreach (KeyValuePair<string, List<string>> k in add)
                {
                    if (k.Value.Count == 0)
                        continue;

                    string text;

                    if (k.Value.Count == 1)
                        text = "'" + k.Value[0] + "'";
                    else
                    {
                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < k.Value.Count; i++)
                        {
                            if (i == 0)
                                sb.AppendFormat("'{0}'", k.Value[i]);
                            else if (i == k.Value.Count - 1)
                                sb.AppendFormat(" and '{0}'", k.Value[i]);
                            else
                                sb.AppendFormat(", '{0}'", k.Value[i]);
                        }
                        text = sb.ToString();
                    }

                    switch (mb.Show(string.Format(CommandStrings.WouldYouLikeToAddXToTheIgnorePropertyOnY,
                        text,
                        k.Key), CommandStrings.IgnoreCaption, System.Windows.Forms.MessageBoxButtons.YesNoCancel))
                    {
                        case System.Windows.Forms.DialogResult.Yes:
                            PerformAddIgnores(e, k);
                            break;
                        case System.Windows.Forms.DialogResult.No:
                            continue;
                        default:
                            return;
                    }
                }
            }
            finally
            {
                e.GetService<IFileStatusMonitor>().ScheduleGitStatus(refresh);
            }
        }

        private void PerformAddIgnores(CommandEventArgs e, KeyValuePair<string, List<string>> k)
        {
            string ignoreFilename = Path.Combine(k.Key, GitConstants.IgnoreFilename);

            using (DocumentLock dl = e.GetService<IVisualGitOpenDocumentTracker>().LockDocuments(new[] { ignoreFilename }, DocumentLockType.NoReload))
            using (dl.MonitorChangesForReload()) // Monitor files that are changed by keyword expansion
            {
                AddIgnores(e.Context, ignoreFilename, k.Key, k.Value);
            }
        }

        private static void AddIgnores(IVisualGitServiceProvider context, string ignoreFilename, string path, List<string> ignores)
        {
            try
            {
                context.GetService<IProgressRunner>().RunModal(CommandStrings.IgnoreCaption,
                    delegate(object sender, ProgressWorkerArgs e)
                    {
                        if (File.Exists(ignoreFilename))
                        {
                            int n = 0;
                            foreach (string oldItem in File.ReadAllText(ignoreFilename).Split('\n'))
                            {
                                string item = oldItem.TrimEnd('\r');

                                if (item.Trim().Length == 0)
                                    continue;

                                // Don't add duplicates
                                while (n < ignores.Count && ignores.IndexOf(item, n) >= 0)
                                    ignores.RemoveAt(ignores.IndexOf(item, n));

                                if (ignores.Contains(item))
                                    continue;

                                ignores.Insert(n++, item);
                            }
                        }

                        StringBuilder sb = new StringBuilder();
                        bool next = false;
                        foreach (string item in ignores)
                        {
                            if (next)
                                sb.Append('\n'); // Git wants only newlines
                            else
                                next = true;

                            sb.Append(item);
                        }

                        File.WriteAllText(ignoreFilename, sb.ToString());

                        // .gitignore files need to be in the project, otherwise
                        // they won't be committed.

                        EnsureProjectItemPresent(context, ignoreFilename);
                    });

                // Make sure a changed directory is visible in the PC Window
                context.GetService<IFileStatusMonitor>().ScheduleMonitor(path); 
            }
            finally
            {
                // Ignore doesn't bubble
                context.GetService<IFileStatusCache>().MarkDirtyRecursive(path);
            }
        }

        private static void AddIgnore(Dictionary<string, List<string>> add, GitItem item, string name)
        {
            if (item == null)
                return;
            if (!item.IsVersioned)
                return;
            List<string> toAdd;

            if (!add.TryGetValue(item.FullPath, out toAdd))
            {
                toAdd = new List<string>();
                add.Add(item.FullPath, toAdd);
            }

            if (!toAdd.Contains(name))
                toAdd.Add(name);
        }

        private static void EnsureProjectItemPresent(IVisualGitServiceProvider context, string ignoreFilename)
        {
            var dte = context.GetService<IVisualGitServiceProvider>().GetService<EnvDTE.DTE>();

            foreach (EnvDTE.Project project in (IEnumerable)dte.ActiveSolutionProjects)
            {
                // FullName is the name of the project file; need to get the
                // containing directory.

                string projectPath = Path.GetDirectoryName(project.FullName);

                if (!projectPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    projectPath += Path.DirectorySeparatorChar;

                // We add the ignore file to all projects that have the ignore
                // file located under its root. If the file is already part
                // of the project, this is a no-op.

                if (ignoreFilename.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase))
                    project.ProjectItems.AddFromFile(ignoreFilename);
            }
        }
    }
}
