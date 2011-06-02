using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using VisualGit.Scc;
using VisualGit.Scc.UI;
using VisualGit.UI;
using VisualGit.UI.Annotate;
using VisualGit.VS;
using SharpSvn;
using System.Collections.Generic;
using VisualGit.UI.Commands;

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
                    if (ri != null && ri.Origin != null && ri.NodeKind != SvnNodeKind.Directory)
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
            SvnRevision startRev = SvnRevision.Zero;
            SvnRevision endRev = null;
            switch (e.Command)
            {
                case VisualGitCommand.ItemAnnotate:
                    endRev = SvnRevision.Base;
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
                    endRev = SvnRevision.Base;
                    break;
            }

            if (targets.Count == 0)
                return;

            bool ignoreEols = true;
            SvnIgnoreSpacing ignoreSpacing = SvnIgnoreSpacing.IgnoreSpace;
            bool retrieveMergeInfo = false;
            GitOrigin target;

            if ((!e.DontPrompt && !Shift) || e.PromptUser)
                using (AnnotateDialog dlg = new AnnotateDialog())
                {
                    dlg.SetTargets(targets);

                    throw new NotImplementedException();
#if false
                    dlg.StartRevision = startRev;
                    dlg.EndRevision = endRev;

                    if (dlg.ShowDialog(e.Context) != DialogResult.OK)
                        return;

                    target = dlg.SelectedTarget;
                    startRev = dlg.StartRevision;
                    endRev = dlg.EndRevision;
                    ignoreEols = dlg.IgnoreEols;
                    ignoreSpacing = dlg.IgnoreSpacing;
                    retrieveMergeInfo = dlg.RetrieveMergeInfo;
#endif
                }
            else
            {
                GitItem one = EnumTools.GetFirst(e.Selection.GetSelectedGitItems(false));

                if (one == null)
                    return;

                target = new GitOrigin(one);
            }

            DoBlame(e, target, startRev, endRev, ignoreEols, ignoreSpacing, retrieveMergeInfo);
        }

        static void DoBlame(CommandEventArgs e, GitOrigin item, SvnRevision revisionStart, SvnRevision revisionEnd, bool ignoreEols, SvnIgnoreSpacing ignoreSpacing, bool retrieveMergeInfo)
        {
            SvnWriteArgs wa = new SvnWriteArgs();
            wa.Revision = revisionEnd;

            SvnBlameArgs ba = new SvnBlameArgs();
            ba.Start = revisionStart;
            ba.End = revisionEnd;
            ba.IgnoreLineEndings = ignoreEols;
            ba.IgnoreSpacing = ignoreSpacing;
            ba.RetrieveMergedRevisions = retrieveMergeInfo;

            SvnTarget target = item.Target;

            IVisualGitTempFileManager tempMgr = e.GetService<IVisualGitTempFileManager>();
            string tempFile = tempMgr.GetTempFileNamed(target.FileName);

            Collection<SvnBlameEventArgs> blameResult = null;
            Dictionary<long, string> logMessages = new Dictionary<long, string>();

            ba.Notify += delegate(object sender, SvnNotifyEventArgs ee)
            {
                if (ee.Action == SvnNotifyAction.BlameRevision && ee.RevisionProperties != null)
                {
                    if (ee.RevisionProperties.Contains(SvnPropertyNames.SvnLog))
                        logMessages[ee.Revision] = ee.RevisionProperties[SvnPropertyNames.SvnLog].StringValue;
                }
            };

            bool retry = false;
            ProgressRunnerResult r = e.GetService<IProgressRunner>().RunModal(CommandStrings.Annotating, delegate(object sender, ProgressWorkerArgs ee)
            {
                using (FileStream fs = File.Create(tempFile))
                {
                    ee.SvnClient.Write(target, fs, wa);
                }

                ba.SvnError +=
                    delegate(object errorSender, SvnErrorEventArgs errorEventArgs)
                    {
                        if (errorEventArgs.Exception is SvnClientBinaryFileException)
                        {
                            retry = true;
                            errorEventArgs.Cancel = true;
                        }
                    };
                ee.SvnClient.GetBlame(target, ba, out blameResult);
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
                                          ba.IgnoreMimeType = true;
                                          ee.SvnClient.GetBlame(target, ba, out blameResult);
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
