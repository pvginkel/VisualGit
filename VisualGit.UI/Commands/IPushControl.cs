using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.UI.Commands
{
    internal interface IPushControl
    {
        string RepositoryPath { get; set; }

        IVisualGitServiceProvider Context { get; set; }

        GitPushArgs Args { get; set; }

        void LoadFromClient(GitClient client);

        bool FlushArgs(string remote);
    }
}
