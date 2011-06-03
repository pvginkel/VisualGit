using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

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
            Args.OnNotify(e);
            Client.OnNotify(e);
        }

        public void RaiseCommitting(GitCommittingEventArgs e)
        {
            var withCommitArgs = Args as GitClientArgsWithCommit;

            if (withCommitArgs != null)
                withCommitArgs.OnCommitting(e);

            Client.OnCommitting(e);
        }

        public bool CancelRequested()
        {
            return CancelRequested(null);
        }

        public bool CancelRequested(CancelEventArgs cancelEventArgs)
        {
            if (cancelEventArgs == null || !cancelEventArgs.Cancel)
            {
                cancelEventArgs = new CancelEventArgs();

                Args.OnCancel(cancelEventArgs);
            }

            if (cancelEventArgs.Cancel && Args.ThrowOnCancel)
                throw new GitOperationCancelledException();

            return cancelEventArgs.Cancel;
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
