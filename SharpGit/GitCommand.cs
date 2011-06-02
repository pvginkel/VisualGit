using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    internal abstract class GitCommand
    {
        protected GitCommand(GitClient client, GitClientArgs args)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (args == null)
                throw new ArgumentNullException("args");

            Client = client;
            Args = args;
        }

        public GitClient Client { get; private set; }
        public GitClientArgs Args { get; private set; }

        public void RaiseNotify(GitNotifyEventArgs e)
        {
            Args.RaiseNotify(e);
            Client.OnNotify(e);
        }
    }

    internal abstract class GitCommand<T> : GitCommand
        where T : GitClientArgs
    {
        protected GitCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public new T Args
        {
            get { return (T)base.Args; }
        }
    }
}
