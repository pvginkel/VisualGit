using System;
using VisualGit.Commands;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using System.Windows.Forms.Design;
using VisualGit.UI.PropertyEditors;
using System.Collections.Generic;
using SharpSvn;
using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace VisualGit.UI.SvnLog.Commands
{
    [Command(VisualGitCommand.LogShowRevisionProperties, AlwaysAvailable = true)]
    class ShowRevisionProperties : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
            if (null == EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>()))
                e.Enabled = false;
        }

        public void OnExecute(CommandEventArgs e)
        {
            IGitLogItem selectedLog = EnumTools.GetSingle(e.Selection.GetSelection<IGitLogItem>());

            if (selectedLog == null)
                return;

            using (PropertyEditorDialog dialog = new PropertyEditorDialog(selectedLog.RepositoryRoot, selectedLog.Revision, true))
            {
                SvnRevisionPropertyListArgs args = new SvnRevisionPropertyListArgs();
                args.ThrowOnError = false;
                SvnPropertyCollection properties = null;

                if (!e.GetService<IProgressRunner>().RunModal(LogStrings.RetrievingRevisionProperties,
                    delegate(object sender, ProgressWorkerArgs wa)
                    {
                        if (!wa.Client.GetRevisionPropertyList(selectedLog.RepositoryRoot, selectedLog.Revision, args, out properties))
                            properties = null;
                    }).Succeeded)
                {
                    return;
                }
                else if (properties != null)
                {
                    List<PropertyEditItem> propItems = new List<PropertyEditItem>();
                    foreach (SvnPropertyValue prop in properties)
                    {
                        PropertyEditItem pi = new PropertyEditItem(dialog.ListView, prop.Key);
                        pi.OriginalValue = pi.Value = pi.BaseValue = prop;

                        propItems.Add(pi);
                    }
                    dialog.PropertyValues = propItems.ToArray();
                }

                if (dialog.ShowDialog(e.Context) != DialogResult.OK)
                    return;

                PropertyEditItem[] finalItems = dialog.PropertyValues;

                bool hasChanges = false;

                foreach (PropertyEditItem ei in finalItems)
                {
                    if (ei.ShouldPersist)
                    {
                        hasChanges = true;
                        break;
                    }
                }
                if (!hasChanges)
                    return;

                IProgressRunner progressRunner = e.GetService<IProgressRunner>();

                ProgressRunnerResult result = progressRunner.RunModal(LogStrings.UpdatingRevisionProperties,
                    delegate(object sender, ProgressWorkerArgs ee)
                    {
                        foreach (PropertyEditItem ei in finalItems)
                        {
                            if (!ei.ShouldPersist)
                                continue;

                            if (ei.IsDeleted)
                                ee.Client.DeleteRevisionProperty(selectedLog.RepositoryRoot, selectedLog.Revision, ei.PropertyName);
                            else if (ei.Value.StringValue != null)
                                ee.Client.SetRevisionProperty(selectedLog.RepositoryRoot, selectedLog.Revision, ei.PropertyName, ei.Value.StringValue);
                            else
                                ee.Client.SetRevisionProperty(selectedLog.RepositoryRoot, selectedLog.Revision, ei.PropertyName, ei.Value.RawValue);
                        }
                    });

                if (result.Succeeded)
                {
                    ILogControl logWindow = e.Selection.GetActiveControl<ILogControl>();

                    if (logWindow != null)
                        logWindow.Restart();
                }

            } // using
        }
    }
}
