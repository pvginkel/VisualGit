using System;
using System.CodeDom.Compiler;
using System.IO;
using VisualGit.Scc.UI;
using VisualGit.Selection;
using VisualGit.UI;
using VisualGit.VS;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// Base class for the DiffLocalItem and CreatePatch commands
    /// </summary>
    public abstract class LocalDiffCommandBase : CommandBase
    {
        readonly TempFileCollection _tempFileCollection = new TempFileCollection();

        /// <summary>
        /// Gets the temp file collection.
        /// </summary>
        /// <value>The temp file collection.</value>
        protected TempFileCollection TempFileCollection
        {
            get { return _tempFileCollection; }
        }        

#if false
        protected virtual string GetDiff(IVisualGitServiceProvider context, ISelectionContext selection)
        {
            return GetDiff(
                context, 
                selection, 
                null);
        }
        /// <summary>
        /// Generates the diff from the current selection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selection"></param>
        /// <param name="revisions"></param>
        /// <returns>The diff as a string.</returns>
        protected virtual string GetDiff(IVisualGitServiceProvider context, ISelectionContext selection, SvnRevisionRange revisions)
        {
            return GetDiff(
                context, 
                selection, 
                revisions, 
                delegate(GitItem item) 
                { 
                    return item.IsVersioned; 
                });
        }
        /// <summary>
        /// Generates the diff from the current selection.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="selection"></param>
        /// <param name="revisions"></param>
        /// <param name="visibleFilter"></param>
        /// <returns>The diff as a string.</returns>
        protected virtual string GetDiff(IVisualGitServiceProvider context, ISelectionContext selection, SvnRevisionRange revisions, Predicate<GitItem> visibleFilter)
        {
            if (selection == null)
                throw new ArgumentNullException("selection");
            if (context == null)
                throw new ArgumentNullException("context");

            IUIShell uiShell = context.GetService<IUIShell>();

            bool foundModified = false;
            foreach (GitItem item in selection.GetSelectedGitItems(true))
            {
                if (item.IsModified || item.IsDocumentDirty)
                {
                    foundModified = true;
                    break; // no need (yet) to keep searching
                }
            }

            PathSelectorInfo info = new PathSelectorInfo("Select items for diffing", selection.GetSelectedGitItems(true));
            info.VisibleFilter += visibleFilter;
            if (foundModified)
                info.CheckedFilter += delegate(GitItem item) { return item.IsFile && (item.IsModified || item.IsDocumentDirty); };

            info.RevisionStart = revisions == null ? GitRevision.Base : revisions.StartRevision;
            info.RevisionEnd = revisions == null ? GitRevision.Working : revisions.EndRevision;

            PathSelectorResult result;
            // should we show the path selector?
            if (!Shift && (revisions == null || !foundModified))
            {
                result = uiShell.ShowPathSelector(info);
                if (!result.Succeeded)
                    return null;
            }
            else
                result = info.DefaultResult;

            if (!result.Succeeded)
                return null;

            SaveAllDirtyDocuments(selection, context);

            return DoExternalDiff(context, result);
        }
#endif

        private static string DoExternalDiff(IVisualGitServiceProvider context, PathSelectorResult info)
        {
            foreach (GitItem item in info.Selection)
            {
                // skip unmodified for a diff against the textbase
                if (info.RevisionStart == GitRevision.Base &&
                    info.RevisionEnd == GitRevision.Working && !item.IsModified)
                    continue;

                string tempDir = context.GetService<IVisualGitTempDirManager>().GetTempDir();

                VisualGitDiffArgs da = new VisualGitDiffArgs();

                da.BaseFile = GetPath(context, info.RevisionStart, item, tempDir);
                da.MineFile = GetPath(context, info.RevisionEnd, item, tempDir);


                context.GetService<IVisualGitDiffHandler>().RunDiff(da);
            }

            return null;
        }

        private static string GetPath(IVisualGitServiceProvider context, GitRevision revision, GitItem item, string tempDir)
        {
            if (revision == GitRevision.Working)
            {
                return item.FullPath;
            }

            string strRevision;
            if (revision.RevisionType == GitRevisionType.Time)
                strRevision = revision.Time.ToLocalTime().ToString("yyyyMMdd_hhmmss");
            else
                strRevision = revision.ToString();
            string tempFile = Path.GetFileNameWithoutExtension(item.Name) + "." + strRevision + Path.GetExtension(item.Name);
            tempFile = Path.Combine(tempDir, tempFile);
            // we need to get it from the repos
            context.GetService<IProgressRunner>().RunModal("Retrieving file for diffing", delegate(object o, ProgressWorkerArgs ee)
            { 
                GitTarget target;

                switch(revision.RevisionType)
                {
                    case GitRevisionType.Head:
                    case GitRevisionType.Hash:
                    case GitRevisionType.Time:
                        target = new GitUriTarget(item.Uri);
                        break;
                    default:
                        target = new GitPathTarget(item.FullPath);
                        break;
                }
                GitWriteArgs args = new GitWriteArgs();
                args.Revision = revision;
                
                using (FileStream stream = File.Create(tempFile))
                {
                    ee.Client.Write(target, stream, args);
                }
            });

            return tempFile;
        }
    }
}
