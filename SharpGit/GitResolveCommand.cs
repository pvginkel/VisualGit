using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Dircache;
using NGit.Api;
using System.Diagnostics;
using NGit;
using System.IO;

namespace SharpGit
{
    internal class GitResolveCommand : GitCommand<GitResolveArgs>
    {
        public GitResolveCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string fullPath, GitAccept accept)
        {
            Debug.Assert(Args.Depth == GitDepth.Empty);

            if (accept == GitAccept.Merged)
            {
                MarkMerged(fullPath);
            }
            else
            {
                var repositoryEntry = Client.GetRepository(fullPath);

                GitConflictEventArgs args = Args.ConflictArgs;
                bool requireCleanup = false;

                if (args == null)
                {
                    args = new GitConflictEventArgs
                    {
                        MergedFile = fullPath,
                        Path = repositoryEntry.Repository.GetRepositoryPath(fullPath),
                        ConflictReason = GitConflictReason.Edited
                    };

                    requireCleanup = true;
                }

                try
                {
                    using (repositoryEntry.Lock())
                    {
                        var repository = repositoryEntry.Repository;

                        switch (accept)
                        {
                            case GitAccept.Base:
                                SelectAndMarkMerged(repository, args, args.BaseFile);
                                break;

                            case GitAccept.MineFull:
                                SelectAndMarkMerged(repository, args, args.MyFile);
                                break;

                            case GitAccept.TheirsFull:
                                SelectAndMarkMerged(repository, args, args.TheirFile);
                                break;
                        }
                    }
                }
                finally
                {
                    if (requireCleanup)
                        args.Cleanup();
                }
            }
        }

        private void SelectAndMarkMerged(Repository repository, GitConflictEventArgs args, string path)
        {
            File.Copy(path, args.MergedFile, true);

            MarkMerged(args.MergedFile);
        }

        private void MarkMerged(string fullPath)
        {
            Client.Add(fullPath, new GitAddArgs { Update = true });
        }
    }
}
