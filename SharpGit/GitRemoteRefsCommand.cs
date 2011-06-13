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
                // LsRemoteCommand does not have the option of providing a
                // CredentialsProvider. Below is the code from LsRemoteCommand
                // copied/pasted with the added SetCredentialsProvider.

                var result = new GitRemoteRefsResult();

                try
                {
                    Transport transport = Transport.Open(repository, remote);

                    transport.SetCredentialsProvider(new CredentialsProvider(this));

                    try
                    {
                        var refSpecs = new List<RefSpec>();

                        if (types.HasFlag(GitRemoteRefType.Tags))
                        {
                            refSpecs.Add(new RefSpec("refs/tags/*:refs/remotes/origin/tags/*"));
                        }
                        if (types.HasFlag(GitRemoteRefType.Branches))
                        {
                            refSpecs.Add(new RefSpec("refs/heads/*:refs/remotes/origin/*"));
                        }
                        ICollection<Ref> refs;
                        IDictionary<string, Ref> refmap = new Dictionary<string, Ref>();
                        FetchConnection fc = transport.OpenFetch();
                        try
                        {
                            refs = fc.GetRefs();
                            foreach (Ref r in refs)
                            {
                                bool found = refSpecs.Count == 0;
                                foreach (RefSpec rs in refSpecs)
                                {
                                    if (rs.MatchSource(r))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                {
                                    refmap.Add(r.GetName(), r);
                                }
                            }
                        }
                        finally
                        {
                            fc.Close();
                        }

                        foreach (var @ref in refmap.Values)
                        {
                            result.Items.Add(new GitRef(@ref));
                        }
                    }
                    catch (TransportException e)
                    {
                        throw new JGitInternalException(JGitText.Get().exceptionCaughtDuringExecutionOfLsRemoteCommand, e);
                    }
                    finally
                    {
                        transport.Close();
                    }
                }
                catch (JGitInternalException ex)
                {
                    var exception = new GitException(GitErrorCode.GetRemoteRefsFailed, ex);

                    Args.SetError(exception);

                    result.PostGetRemoteRefsError = ex.Message;

                    if (Args.ShouldThrow(exception.ErrorCode))
                        throw exception;
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
