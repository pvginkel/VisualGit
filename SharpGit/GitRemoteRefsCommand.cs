using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Api;
using NGit.Api.Errors;
using NGit.Transport;
using NGit;
using NGit.Errors;

namespace SharpGit
{
    internal class GitRemoteRefsCommand : GitTransportCommand<GitRemoteRefsArgs>
    {
        public GitRemoteRefsCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitRemoteRefsResult Execute(string remote, GitRemoteRefType types)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (types == 0)
                throw new ArgumentOutOfRangeException("types", "Select at least one type");

            var repository = CreateDummyRepository();

            try
            {
                var command = new Git(repository).LsRemote();

                command.SetRemote(remote);

                var result = new GitRemoteRefsResult();

                ICollection<Ref> refs;

                using (new CredentialsProviderScope(new CredentialsProvider(this)))
                {
                    refs = command.Call();
                }

                foreach (var item in refs)
                {
                    result.Items.Add(new GitRef(item));
                }

                return result;
            }
            finally
            {
                repository.Close();
            }
        }
    }
}
