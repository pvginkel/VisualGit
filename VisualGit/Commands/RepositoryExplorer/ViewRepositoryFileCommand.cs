using System;
using System.Collections;
using VisualGit.Scc;
using System.IO;
using SharpSvn;
using SharpGit;

namespace VisualGit.Commands
{
    /// <summary>
    /// A command that lets you view a repository file.
    /// </summary>
    abstract class ViewRepositoryFileCommand : CommandBase
    {
        public override void OnUpdate(CommandUpdateEventArgs e)
        {
            IGitRepositoryItem single = EnumTools.GetSingle(e.Selection.GetSelection<IGitRepositoryItem>());

            if (single == null || single.NodeKind == GitNodeKind.Directory || single.Origin == null)
                e.Enabled = false;            
        }

        protected static bool SaveFile(CommandEventArgs e, IGitRepositoryItem ri, string toFile)
        {
            ProgressRunnerResult r = e.GetService<IProgressRunner>().RunModal(
                "Saving File",
                delegate(object sender, ProgressWorkerArgs ee)
                {
                    using (FileStream fs = File.Create(toFile))
                    {
                        GitWriteArgs args = new GitWriteArgs();
                        if(ri.Revision != null)
                            args.Revision = ri.Revision;

                        ee.Client.Write(ri.Origin.Target, fs, args);
                    }
                });

            return r.Succeeded;
        }
    }
}



