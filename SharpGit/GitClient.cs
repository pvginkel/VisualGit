using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            if (!args.ThrowOnError && !RepositoryUtil.IsBelowManagedPath(path))
            {
                args.SetError(new GitNoRepositoryException());
                return false;
            }
#endif

            try
            {
                IsCommandRunning = true;

                new GitStatusCommand(this, args).Execute(path, callback);

                return true;
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

        public bool Delete(string path, GitDeleteArgs args)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (args == null)
                throw new ArgumentNullException("args");

            try
            {
                IsCommandRunning = true;

                new GitDeleteCommand(this, args).Execute(path);

                return true;
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
        
        public bool Revert(IEnumerable<string> paths, GitRevertArgs args)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");
            if (args == null)
                throw new ArgumentNullException("args");

            try
            {
                IsCommandRunning = true;

                new GitRevertCommand(this, args).Execute(paths);

                return true;
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

        public bool Add(string path, GitAddArgs args)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (args == null)
                throw new ArgumentNullException("args");

            try
            {
                IsCommandRunning = true;

                new GitAddCommand(this, args).Execute(path);

                return true;
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
    }
}
