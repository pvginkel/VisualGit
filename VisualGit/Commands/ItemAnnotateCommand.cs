using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using VisualGit.UI;
using VisualGit.UI.Annotate;
using VisualGit.VS;
using System.Collections.Generic;
using VisualGit.UI.Commands;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Command to identify which users to blame for which lines.
    /// </summary>
    [Command(VisualGitCommand.ItemAnnotate)]
    [Command(VisualGitCommand.LogAnnotateRevision)]
    [Command(VisualGitCommand.GitNodeAnnotate)]
    [Command(VisualGitCommand.DocumentAnnotate)]
    class ItemAnnotateCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            switch (e.Command)
            {
                case VisualGitCommand.GitNodeAnnotate:
                    IGitRepositoryItem ri = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());
                    if (ri != null && ri.Origin != null && ri.NodeKind != GitNodeKind.Directory)
                        return;
                    break;
                case VisualGitCommand.ItemAnnotate:
                    foreach (GitItem item in e.Selection.GetSelectedGitItems(false))
                    {
                        if (item.IsVersioned && item.IsFile)
                            return;
                    }
                    break;
                case VisualGitCommand.DocumentAnnotate:
                    if (e.Selection.ActiveDocumentItem != null && e.Selection.ActiveDocumentItem.HasCopyableHistory)
                        return;
                    break;
                case VisualGitCommand.LogAnnotateRevision:
                    ILogControl logControl = e.Selection.GetActiveControl<ILogControl>();
                    if (logControl == null || logControl.Origins == null)
                    {
                        e.Visible = e.Enabled = false;
                        return;
                    }

                    if (!EnumTools.IsEmpty(e.Selection.GetSelection<IGitLogChangedPathItem>()))
                        return;
                    break;
            }
            e.Enabled = false;
        }

        public override void OnExecute(CommandEventArgs e)
        {
            List<GitOrigin> targets = new List<GitOrigin>();
            GitRevision endRev = null;
            switch (e.Command)
            {
                case VisualGitCommand.ItemAnnotate:
                    endRev = GitRevision.Base;
                    foreach (GitItem i in e.Selection.GetSelectedGitItems(false))
                    {
                        if (i.IsVersionable)
                            targets.Add(new GitOrigin(i));
                    }
                    break;
                case VisualGitCommand.LogAnnotateRevision:
                    foreach (IGitLogChangedPathItem logItem in e.Selection.GetSelection<IGitLogChangedPathItem>())
                    {
                        targets.Add(logItem.Origin);
                        endRev = logItem.Revision;
                    }
                    break;
                case VisualGitCommand.GitNodeAnnotate:
                    foreach (IGitRepositoryItem item in e.Selection.GetSelection<IGitRepositoryItem>())
                    {
                        targets.Add(item.Origin);
                        endRev = item.Revision;
                    }
                    break;
                case VisualGitCommand.DocumentAnnotate:
                    targets.Add(new GitOrigin(e.GetService<IFileStatusCache>()[e.Selection.ActiveDocumentFilename]));
                    endRev = GitRevision.Base;
                    break;
            }

            if (targets.Count == 0)
                return;

            GitIgnoreSpacing ignoreSpacing = GitIgnoreSpacing.IgnoreSpace;
            GitOrigin target;

            if ((!e.DontPrompt && !Shift) || e.PromptUser)
                using (AnnotateDialog dlg = new AnnotateDialog())
                {
                    dlg.SetTargets(targets);

                    dlg.EndRevision = endRev;

                    if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    target = dlg.SelectedTarget;
                    endRev = dlg.EndRevision;
                    ignoreSpacing = dlg.IgnoreSpacing;
                }
            else
            {
                GitItem one = EnumTools.GetFirst(e.Selection.GetSelectedGitItems(false));

                if (one == null)
                    return;

                target = new GitOrigin(one);
            }

            DoBlame(e, target, endRev, ignoreSpacing);
        }

        static void DoBlame(CommandEventArgs e, GitOrigin item, GitRevision revisionEnd, GitIgnoreSpacing ignoreSpacing)
        {
            GitWriteArgs wa = new GitWriteArgs();
            wa.Revision = revisionEnd;

            GitBlameArgs ba = new GitBlameArgs();
            ba.End = revisionEnd;
            ba.IgnoreSpacing = ignoreSpacing;

            GitTarget target = item.Target;

            IVisualGitTempFileManager tempMgr = e.GetService<IVisualGitTempFileManager>();
            string tempFile = tempMgr.GetTempFileNamed(target.FileName);

            Collection<GitBlameEventArgs> blameResult = null;
            Dictionary<string, string> logMessages = new Dictionary<string, string>();

            bool retry = false;
            ProgressRunnerResult r = e.GetService<IProgressRunner>().RunModal(CommandStrings.Annotating, delegate(object sender, ProgressWorkerArgs ee)
            {
                using (FileStream fs = File.Create(tempFile))
                {
                    ee.Client.Write(target, fs, wa);
                }
                try
                {
                    ee.Client.GetBlame(target, ba, out blameResult);
                }
                catch (GitClientBinaryFileException)
                {
                    retry = true;
                }
            });

            if (retry)
            {
                using (VisualGitMessageBox mb = new VisualGitMessageBox(e.Context))
                {
                    if (DialogResult.Yes == mb.Show(
                                                CommandStrings.AnnotateBinaryFileContinueAnywayText,
                                                CommandStrings.AnnotateBinaryFileContinueAnywayTitle,
                                                MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                    {
                        r = e.GetService<IProgressRunner>()
                            .RunModal(CommandStrings.Annotating,
                                      delegate(object sender, ProgressWorkerArgs ee)
                                      {
                                          ee.Client.GetBlame(target, ba, out blameResult);
                                      });
                    }
                }
            }

            if (!r.Succeeded)
                return;

            AnnotateEditorControl annEditor = new AnnotateEditorControl();
            IVisualGitEditorResolver er = e.GetService<IVisualGitEditorResolver>();

            annEditor.Create(e.Context, tempFile);
            annEditor.LoadFile(tempFile);
            annEditor.AddLines(item, blameResult, logMessages);

            // Detect and set the language service
            Guid language;
            if (er.TryGetLanguageService(Path.GetExtension(target.FileName), out language))
            {
                // Extension is mapped -> user
                annEditor.SetLanguageService(language);
            }
            else if (blameResult != null && blameResult.Count > 0 && blameResult[0].Line != null)
            {
                // Extension is not mapped -> Check if this is xml (like project files)
                string line = blameResult[0].Line.Trim();

                if (line.StartsWith("<?xml")
                    || (line.StartsWith("<") && line.Contains("xmlns=\"http://schemas.microsoft.com/developer/msbuild/")))
                {
                    if (er.TryGetLanguageService(".xml", out language))
                    {
                        annEditor.SetLanguageService(language);
                    }
                }
            }
        }
    }
}
