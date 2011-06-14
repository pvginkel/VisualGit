using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NGit.Api.Errors;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public class GitClient : IDisposable
    {
        private static readonly Dictionary<string, Version> _clients = new Dictionary<string, Version>(StringComparer.OrdinalIgnoreCase);
        private bool _disposed;

        internal GitUIBindArgs BindArgs { get; set; }

        public bool IsCommandRunning { get; private set; }

        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public bool Status(string path, GitStatusArgs args, EventHandler<GitStatusEventArgs> callback)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (args == null)
                throw new ArgumentNullException("args");
            if (callback == null)
                throw new ArgumentNullException("callback");

#if DEBUG
            // We cheat here to aid debugging.

            if (!args.ThrowOnError && !GitTools.IsBelowManagedPath(path))
            {
                args.SetError(new GitNoRepositoryException());
                return false;
            }
#endif
            return ExecuteCommand<GitStatusCommand>(args, p => p.Execute(path, callback));
        }

        public bool Delete(string path, GitDeleteArgs args)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitDeleteCommand>(args, p => p.Execute(path));
        }
        
        public bool Revert(IEnumerable<string> paths, GitRevertArgs args)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitRevertCommand>(args, p => p.Execute(paths));
        }

        public bool Add(string path, GitAddArgs args)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitAddCommand>(args, p => p.Execute(path));
        }

        public bool Commit(IEnumerable<string> paths, GitCommitArgs args, out GitCommitResult result)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitCommitCommand, GitCommitResult>(args, p => p.Execute(paths), out result);
        }

        public bool Write(GitTarget path, Stream stream, GitWriteArgs args)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitWriteCommand>(args, p => p.Execute(path, stream));
        }

        public bool Log(string repositoryPath, GitLogArgs args)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitLogCommand>(args, p => p.Execute(repositoryPath));
        }

        public bool Log(IEnumerable<Uri> uris, GitLogArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitLogCommand>(args, p => p.Execute(uris));
        }

        public bool Switch(string repositoryPath, GitRef target, GitSwitchArgs args, out GitSwitchResult result)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (target == null)
                throw new ArgumentNullException("target");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitSwitchCommand, GitSwitchResult>(args, p => p.Execute(target, repositoryPath), out result);
        }

        public bool Push(string repositoryPath, GitPushArgs args, out GitPushResult result)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitPushCommand, GitPushResult>(args, p => p.Execute(repositoryPath), out result);
        }

        public bool Pull(string repositoryPath, GitPullArgs args, out GitPullResult result)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitPullCommand, GitPullResult>(args, p => p.Execute(repositoryPath), out result);
        }

        public bool Diff(string fullPath, GitRevisionRange revRange, GitDiffArgs args, Stream stream)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");
            if (revRange == null)
                throw new ArgumentNullException("revRange");
            if (args == null)
                throw new ArgumentNullException("args");
            if (stream == null)
                throw new ArgumentNullException("stream");

            return ExecuteCommand<GitDiffCommand>(args, p => p.Execute(fullPath, revRange, stream));
        }

        public bool Move(string fromPath, string toPath, GitMoveArgs args)
        {
            if (fromPath == null)
                throw new ArgumentNullException("fromPath");
            if (toPath == null)
                throw new ArgumentNullException("toPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitMoveCommand>(args, p => p.Execute(fromPath, toPath));
        }

        public bool Merge(string repositoryPath, GitRef branch, GitMergeArgs args)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (branch == null)
                throw new ArgumentNullException("branch");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitMergeCommand>(args, p => p.Execute(repositoryPath, branch));
        }

        public bool GetInfo(string fullPath, GitInfoArgs args, out GitInfoEventArgs info)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitInfoCommand, GitInfoEventArgs>(args, p => p.Execute(fullPath), out info);
        }

        public bool Resolve(string fullPath, GitAccept accept, GitResolveArgs args)
        {
            if (fullPath == null)
                throw new ArgumentNullException("fullPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitResolveCommand>(args, p => p.Execute(fullPath, accept));
        }

        public bool GetBlame(GitTarget target, GitBlameArgs args, out Collection<GitBlameEventArgs> result)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (args == null)
                throw new ArgumentNullException("args");

            GitBlameResult commandResult;

            bool success = ExecuteCommand<GitBlameCommand, GitBlameResult>(args, p => p.Execute(target), out commandResult);

            if (commandResult == null)
                result = null;
            else
                result = commandResult.Items;

            return success;
        }

        public bool GetRemoteRefs(string remote, GitRemoteRefType types, GitRemoteRefsArgs args, out GitRemoteRefsResult result)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (types == 0)
                throw new ArgumentOutOfRangeException("types", "Select at least one type");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitRemoteRefsCommand, GitRemoteRefsResult>(args, p => p.Execute(remote, types), out result);
        }

        public bool Clone(string remote, GitRef @ref, string destination, GitCloneArgs args, out GitCloneResult result)
        {
            if (remote == null)
                throw new ArgumentNullException("remote");
            if (@ref == null)
                throw new ArgumentNullException("ref");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitCloneCommand, GitCloneResult>(args, p => p.Execute(remote, @ref, destination), out result);
        }

        public bool CreateRepository(string repositoryPath, GitCreateRepositoryArgs args)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitCreateRepositoryCommand>(args, p => p.Execute(repositoryPath));
        }

        public bool Branch(string repositoryPath, string branchName, GitBranchArgs args)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (branchName == null)
                throw new ArgumentNullException("branchName");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitBranchCommand>(args, p => p.Execute(repositoryPath, branchName));
        }

        public bool Tag(string repositoryPath, string tagName, GitTagArgs args)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (tagName == null)
                throw new ArgumentNullException("tagName");
            if (args == null)
                throw new ArgumentNullException("args");

            return ExecuteCommand<GitTagCommand>(args, p => p.Execute(repositoryPath, tagName));
        }

        private bool ExecuteCommand<T>(GitClientArgs args, Action<T> action)
            where T : GitCommand
        {
            try
            {
                IsCommandRunning = true;

                T command = (T)Activator.CreateInstance(typeof(T), new object[] { this, args });

                action(command);

                return args.LastException == null;
            }
            catch (CanceledException)
            {
                var exception = new GitOperationCancelledException();

                args.LastException = exception;

                if (args.ThrowOnCancel)
                    throw exception;

                return false;
            }
            catch (GitException ex)
            {
                args.SetError(ex);

                if (args.ShouldThrow(ex.ErrorCode))
                    throw;

                return false;
            }
            finally
            {
                IsCommandRunning = false;
            }
        }

        private bool ExecuteCommand<TCommand, TResult>(GitClientArgs args, Func<TCommand, TResult> action, out TResult result)
            where TCommand : GitCommand
            where TResult : GitCommandResult
        {
            try
            {
                IsCommandRunning = true;

                TCommand command = (TCommand)Activator.CreateInstance(typeof(TCommand), new object[] { this, args });

                result = action(command);

                return args.LastException == null;
            }
            catch (CanceledException)
            {
                var exception = new GitOperationCancelledException();

                args.LastException = exception;

                if (args.ThrowOnCancel)
                    throw exception;

                result = null;

                return false;
            }
            catch (GitException ex)
            {
                args.SetError(ex);

                if (args.ShouldThrow(ex.ErrorCode))
                    throw;

                result = null;

                return false;
            }
            finally
            {
                IsCommandRunning = false;
            }
        }

        public event EventHandler<GitNotifyEventArgs> Notify;
        public event EventHandler<GitCommittingEventArgs> Committing;

        internal protected virtual void OnNotify(GitNotifyEventArgs e)
        {
            var ev = Notify;

            if (ev != null)
                ev(this, e);
        }

        internal protected virtual void OnCommitting(GitCommittingEventArgs e)
        {
            var ev = Committing;

            if (ev != null)
                ev(this, e);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                _disposed = true;
            }
        }

        public static void AddClientName(string client, Version version)
        {
            lock (_clients)
            {
                _clients.Add(client, version);
            }
        }

        public GitRef GetCurrentBranch(string repositoryPath)
        {
            var repositoryEntry = GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                return new GitRef(repositoryEntry.Repository.GetFullBranch());
            }
        }

        internal RepositoryEntry GetRepository(string repositoryPath)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");

            var repositoryEntry = RepositoryManager.GetRepository(repositoryPath);

            if (repositoryEntry == null)
                throw new GitNoRepositoryException();

            return repositoryEntry;
        }

        public ICollection<GitRef> GetRefs(string repositoryPath)
        {
            var repositoryEntry = GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var result = new List<GitRef>();

                foreach (var @ref in repositoryEntry.Repository.GetAllRefs())
                {
                    result.Add(new GitRef(@ref.Value));
                }

                return result;
            }
        }

        public IGitConfig GetConfig(string repositoryPath)
        {
            var repositoryEntry = GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                return new GitConfigWrapper(repositoryEntry.Repository.GetConfig());
            }
        }

        public string ResolveReference(string repositoryPath, GitRevision revision)
        {
            if (revision == null)
                throw new ArgumentNullException("revision");

            var repositoryEntry = GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                return revision.GetObjectId(repositoryEntry.Repository).Name;
            }
        }
    }
}
