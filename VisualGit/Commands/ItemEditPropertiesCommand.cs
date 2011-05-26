using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpSvn;
using VisualGit.Scc;
using VisualGit.UI.PropertyEditors;
using VisualGit.Selection;
using VisualGit.VS;


namespace VisualGit.Commands
{
    /// <remarks>
    /// If project/solution (logical) node is selected, target for this command is the project/solution (physical) folder.
    /// </remarks>
    [Command(VisualGitCommand.ItemEditProperties)]
    [Command(VisualGitCommand.ProjectEditProperties)]
    [Command(VisualGitCommand.SolutionEditProperties)]
    [Command(VisualGitCommand.ItemShowPropertyChanges)]
    class ItemEditPropertiesCommand : CommandBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e">The <see cref="VisualGit.Commands.CommandUpdateEventArgs"/> instance containing the event data.</param>
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IFileStatusCache cache;

            int count = 0;
            switch (e.Command)
            {
                case VisualGitCommand.ItemEditProperties:
                case VisualGitCommand.ItemShowPropertyChanges:
                    foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
                    {
                        if (i.IsVersioned)
                        {
                            count++;

                            if (e.Command == VisualGitCommand.ItemShowPropertyChanges
                                && !i.IsPropertyModified)
                            {
                                e.Enabled = false;
                                return;
                            }

                            if (e.Selection.IsSingleNodeSelection)
                                break;
                            if (count > 1)
                            {
                                e.Enabled = false;
                                return;
                            }
                        }
                    }
                    break;
                case VisualGitCommand.ProjectEditProperties:
                    IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();
                    cache = e.GetService<IFileStatusCache>();
                    foreach (GitProject project in e.Selection.GetSelectedProjects(false))
                    {
                        IGitProjectInfo info = pfm.GetProjectInfo(project);
                        if (info == null || string.IsNullOrEmpty(info.ProjectDirectory))
                        {
                            e.Enabled = false;
                            return;
                        }
                        GitItem projectFolder = cache[info.ProjectDirectory];

                        if (projectFolder.IsVersioned)
                            count++;

                        if (count > 1)
                            break;
                    }
                    break;
                case VisualGitCommand.SolutionEditProperties:
                    cache = e.GetService<IFileStatusCache>();
                    IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();
                    if (solutionSettings == null || string.IsNullOrEmpty(solutionSettings.ProjectRoot))
                    {
                        e.Enabled = false;
                        return;
                    }
                    GitItem solutionItem = cache[solutionSettings.ProjectRoot];
                    if (solutionItem.IsVersioned)
                        count = 1;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            if (count == 0 || (count > 1 && !e.Selection.IsSingleNodeSelection))
                e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            GitItem firstVersioned = null;
            IFileStatusCache cache = e.GetService<IFileStatusCache>();

            switch (e.Command)
            {
                case VisualGitCommand.ItemEditProperties:
                case VisualGitCommand.ItemShowPropertyChanges:
                    foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
                    {
                        if (i.IsVersioned)
                        {
                            firstVersioned = i;
                            break;
                        }
                    }
                    break;
                case VisualGitCommand.ProjectEditProperties: // use project folder
                    foreach (GitProject p in e.Selection.GetSelectedProjects(false))
                    {
                        IProjectFileMapper pfm = e.GetService<IProjectFileMapper>();
                        if (pfm != null)
                        {
                            IGitProjectInfo info = pfm.GetProjectInfo(p);
                            if (info != null && info.ProjectDirectory != null)
                            {
                                firstVersioned = cache[info.ProjectDirectory];
                            }
                            if (firstVersioned != null)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case VisualGitCommand.SolutionEditProperties: // use solution folder
                    IVisualGitSolutionSettings solutionSettings = e.GetService<IVisualGitSolutionSettings>();
                    if (solutionSettings != null)
                    {
                        firstVersioned = cache[solutionSettings.ProjectRoot];
                    }
                    break;
            }
            if (firstVersioned == null)
                return; // exceptional case

            //using (SvnClient client = e.GetService<IGitClientPool>().GetNoUIClient())
            using (PropertyEditorDialog dialog = new PropertyEditorDialog(firstVersioned))
            {
                dialog.Context = e.Context;

                SortedList<string, PropertyEditItem> editItems = new SortedList<string, PropertyEditItem>();
                if (!e.GetService<IProgressRunner>().RunModal("Retrieving Properties",
                    delegate(object Sender, ProgressWorkerArgs wa)
                    {
                        // Retrieve base properties
                        wa.Client.PropertyList(new SvnPathTarget(firstVersioned.FullPath, SvnRevision.Base),
                            delegate(object s, SvnPropertyListEventArgs la)
                            {
                                foreach (SvnPropertyValue pv in la.Properties)
                                {
                                    PropertyEditItem ei;
                                    if (!editItems.TryGetValue(pv.Key, out ei))
                                        editItems.Add(pv.Key, ei = new PropertyEditItem(dialog.ListView, pv.Key));

                                    ei.BaseValue = pv;
                                }
                            });
                        //

                        wa.Client.PropertyList(firstVersioned.FullPath,
                            delegate(object s, SvnPropertyListEventArgs la)
                            {
                                foreach (SvnPropertyValue pv in la.Properties)
                                {
                                    PropertyEditItem ei;
                                    if (!editItems.TryGetValue(pv.Key, out ei))
                                        editItems.Add(pv.Key, ei = new PropertyEditItem(dialog.ListView, pv.Key));

                                    ei.OriginalValue = ei.Value = pv;
                                }
                            });


                    }).Succeeded)
                {
                    return; // Canceled
                }

                PropertyEditItem[] items = new PropertyEditItem[editItems.Count];
                editItems.Values.CopyTo(items, 0);
                dialog.PropertyValues = items;

                if (dialog.ShowDialog(e.Context) == DialogResult.OK)
                {
                    // Hack: Currently we save all properties, not only the in memory changed ones

                    items = dialog.PropertyValues;

                    bool hasChanges = false;
                    foreach (PropertyEditItem i in items)
                    {
                        if (i.ShouldPersist)
                        {
                            hasChanges = true;
                            break;
                        }
                    }

                    if (!hasChanges)
                        return;

                    e.GetService<IProgressRunner>().RunModal("Applying property changes",
                        delegate(object sender, ProgressWorkerArgs wa)
                        {
                            foreach (PropertyEditItem ei in items)
                            {
                                if (!ei.ShouldPersist)
                                    continue;

                                if (ei.Value == null)
                                {
                                    if (ei.OriginalValue != null)
                                        wa.Client.DeleteProperty(firstVersioned.FullPath, ei.PropertyName);
                                }
                                else if (!ei.Value.ValueEquals(ei.OriginalValue))
                                {
                                    if (ei.Value.StringValue != null)
                                        wa.Client.SetProperty(firstVersioned.FullPath, ei.PropertyName, ei.Value.StringValue);
                                    else
                                        wa.Client.SetProperty(firstVersioned.FullPath, ei.PropertyName, ei.Value.RawValue);
                                }
                            }
                        });

                } // if

            }
        }
    }
}
