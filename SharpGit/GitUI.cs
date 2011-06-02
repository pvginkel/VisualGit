using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public static class GitUI
    {
        public static void Bind(GitClient client, GitUIBindArgs args)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            client.BindArgs = args;
        }
    }
}
