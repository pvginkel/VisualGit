using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    internal class GitMoveCommand : GitCommand<GitMoveArgs>
    {
        public GitMoveCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string fromPath, string toPath)
        {
            if (fromPath == null)
                throw new ArgumentNullException("fromPath");
            if (toPath == null)
                throw new ArgumentNullException("toPath");

            // Subversion requires a move operation, but we don't. We just
            // do a delete and an add.

            if (File.Exists(toPath))
            {
                if (Args.Force)
                    File.Delete(toPath);
                else
                    throw new GitException(GitErrorCode.MoveObstructed);
            }

            if (File.Exists(fromPath))
                File.Move(fromPath, toPath);

            Client.Delete(fromPath, new GitDeleteArgs
            {
                KeepLocal = false,
                Force = Args.Force
            });

            Client.Add(toPath, new GitAddArgs
            {
                Force = Args.Force
            });
        }
    }
}
