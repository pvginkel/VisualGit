using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Dircache;
using NGit.Api;

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
            Client.Add(fullPath, new GitAddArgs { Update = true });
        }
    }
}
