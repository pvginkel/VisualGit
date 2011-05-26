using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SharpSvn;

namespace VisualGit.Scc
{
    partial class ProjectTracker
    {
        public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments, VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult, VSQUERYREMOVEFILERESULTS[] rgResults)
        {
            if (rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            if (rgResults != null)
                for (int i = 0; i < cFiles; i++)
                {
                    rgResults[i] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveOK;
                }

            if (pSummaryResult != null)
                pSummaryResult[0] = VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveOK;

            return VSConstants.S_OK;
        }

        public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
        {
            if (rgpProjects == null || rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            for (int i = 0; i < cFiles; i++)
            {
                string s = rgpszMkDocuments[i];

                if (!string.IsNullOrEmpty(s) && SvnItem.IsValidPath(s))
                    StatusCache.MarkDirty(SvnTools.GetNormalizedFullPath(s));
            }

            int iFile = 0;

            for (int iProject = 0; (iProject < cProjects) && (iFile < cFiles); iProject++)
            {
                int iLastFileThisProject = (iProject < cProjects - 1) ? rgFirstIndices[iProject + 1] : cFiles;

                IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;
                bool track = SccProvider.TrackProjectChanges(sccProject);

                for (; iFile < iLastFileThisProject; iFile++)
                {
                    if (sccProject == null || !track)
                        continue; // Not handled by our provider

                    string file = rgpszMkDocuments[iFile];

                    if (string.IsNullOrEmpty(file) || !SvnItem.IsValidPath(file))
                        continue;

                    file = SvnTools.GetNormalizedFullPath(file);

                    SccProvider.OnProjectFileRemoved(sccProject, file, rgFlags[iFile]);
                }
            }
            return VSConstants.S_OK;
        }

        public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories, string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags, VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
        {
            if (rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            bool allOk = true;

            IVsSccProject2 sccProject = pProject as IVsSccProject2;

            for (int i = 0; i < cDirectories; i++)
            {
                bool ok = true;

                string dir = rgpszMkDocuments[i];

                if (string.IsNullOrEmpty(dir) || !SvnItem.IsValidPath(dir))
                    continue;

                dir = SvnTools.GetNormalizedFullPath(dir);


                if (SccProvider.TrackProjectChanges(sccProject))
                    SccProvider.OnBeforeRemoveDirectory(sccProject, dir, out ok);

                if (rgResults != null)
                {
                    rgResults[i] = ok ? VSQUERYREMOVEDIRECTORYRESULTS.VSQUERYREMOVEDIRECTORYRESULTS_RemoveOK : VSQUERYREMOVEDIRECTORYRESULTS.VSQUERYREMOVEDIRECTORYRESULTS_RemoveNotOK;
                }

                if (!ok)
                    allOk = false;
            }

            if (pSummaryResult != null)
                pSummaryResult[0] = allOk ? VSQUERYREMOVEDIRECTORYRESULTS.VSQUERYREMOVEDIRECTORYRESULTS_RemoveOK : VSQUERYREMOVEDIRECTORYRESULTS.VSQUERYREMOVEDIRECTORYRESULTS_RemoveNotOK;

            return VSConstants.S_OK;
        }

        public int OnAfterRemoveDirectories(int cProjects, int cDirectories, IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEDIRECTORYFLAGS[] rgFlags)
        {
            if (rgpProjects == null || rgpszMkDocuments == null)
                return VSConstants.E_POINTER;

            int iDirectory = 0;

            for (int i = 0; i < cDirectories; i++)
            {
                string s = rgpszMkDocuments[i];

                if (!string.IsNullOrEmpty(s) && SvnItem.IsValidPath(s))
                    StatusCache.MarkDirty(SvnTools.GetNormalizedFullPath(s));
            }

            for (int iProject = 0; (iProject < cProjects) && (iDirectory < cDirectories); iProject++)
            {
                int iLastDirectoryThisProject = (iProject < cProjects - 1) ? rgFirstIndices[iProject + 1] : cDirectories;

                IVsSccProject2 sccProject = rgpProjects[iProject] as IVsSccProject2;
                bool track = SccProvider.TrackProjectChanges(sccProject);

                for (; iDirectory < iLastDirectoryThisProject; iDirectory++)
                {
                    if (sccProject == null || !track)
                        continue; // Not handled by our provider

                    string dir = rgpszMkDocuments[iDirectory];

                    if (string.IsNullOrEmpty(dir) || !SvnItem.IsValidPath(dir))
                        continue;

                    dir = SvnTools.GetNormalizedFullPath(dir);

                    SccProvider.OnProjectDirectoryRemoved(sccProject, rgpszMkDocuments[iDirectory], rgFlags[iDirectory]);
                }
            }

            return VSConstants.S_OK;
        }
    }
}
