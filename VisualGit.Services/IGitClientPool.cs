using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using System.Diagnostics;
using SvnRemoteSession = SharpSvn.Remote.SvnRemoteSession;
using SharpSvn.Remote;

namespace VisualGit
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>The default implementation of this service is thread safe</remarks>
    public interface IGitClientPool
    {
        /// <summary>
        /// Gets a free <see cref="SvnClient"/> instance from the pool
        /// </summary>
        /// <returns></returns>
        GitPoolClient GetClient();

        /// <summary>
        /// Gets a free <see cref="SvnClient"/> instance from the pool
        /// </summary>
        /// <returns></returns>
        GitPoolClient GetNoUIClient();

        /// <summary>
        /// Gets a working copy client instance
        /// </summary>
        /// <returns></returns>
        SvnWorkingCopyClient GetWcClient();

        /// <summary>
        /// Gets a SvnRemote session to the specified Uri. Throws an SvnException if
        /// the connection can't be opened
        /// </summary>
        /// <param name="sessionUri">The session URI.</param>
        /// <param name="parentOk">if set to <c>true</c> a session hosted at a parent directory is ok.</param>
        /// <returns></returns>
        GitPoolRemoteSession GetRemoteSession(Uri sessionUri, bool parentOk);

        /// <summary>
        /// Returns the client.
        /// </summary>
        /// <param name="poolClient">The pool client.</param>
        /// <returns>true if the pool accepts the client, otherwise false</returns>
        bool ReturnClient(GitPoolClient poolClient);

        /// <summary>
        /// Returns the client.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        bool ReturnClient(GitPoolRemoteSession session);

        /// <summary>
        /// Flushes all clients to read settings again
        /// </summary>
        void FlushAllClients();
    }

    public sealed class GitClientAction
    {
        readonly string _path;
        bool _recursive;
        bool _addOrRemove;
        long _oldRevision;

        public GitClientAction(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            _path = path;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string FullPath
        {
            get { return _path; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GitClientAction"/> is recursive.
        /// </summary>
        /// <value><c>true</c> if recursive; otherwise, <c>false</c>.</value>
        public bool Recursive
        {
            get { return _recursive; }
            set { _recursive = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [add or remove].
        /// </summary>
        /// <value><c>true</c> if [add or remove]; otherwise, <c>false</c>.</value>
        public bool AddOrRemove
        {
            get { return _addOrRemove; }
            set { _addOrRemove = value; }
        }

        /// <summary>
        /// Gets or sets the old revision.
        /// </summary>
        /// <value>The old revision.</value>
        public long OldRevision
        {
            get { return _oldRevision; }
            set { _oldRevision = value; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class GitPoolClient : SvnClient, IDisposable
    {
        IGitClientPool _pool;
        int _nReturns;

        // Note: We can only implement our own Dispose over the existing
        // As VC++ unconditionally calls GC.SuppressFinalize() just before returning
        // Luckily the using construct uses the last defined or IDisposable methods which we can override
               
        protected GitPoolClient(IGitClientPool pool)
        {
            if (pool == null)
                throw new ArgumentNullException("pool");

            _pool = pool;
        }  

        /// <summary>
        /// Returns the client to the client pool
        /// </summary>
        public new void Dispose()
        {
            ReturnClient();

            // No GC.SuppressFinalize() as SvnClient() really needs to have a finalize if we don't dispose
        }

        void IDisposable.Dispose()
        {
            ReturnClient();

            // No GC.SuppressFinalize() as SvnClient() really needs to have a finalize if we don't dispose
        }

        protected IGitClientPool GitClientPool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        /// <summary>
        /// Returns the client to the threadpool, or disposes the cleint
        /// </summary>
        protected virtual void ReturnClient()
        {
            // In our common implementation this code is not used
            // Update VisualGitSvnClientPool.VisualGitSvnPoolClient

            if (_nReturns++ > 32 || IsCommandRunning || !_pool.ReturnClient(this))
            {
                // Recycle the SvnClient if at least one of the following is true
                // * A command is still active in it (User error)
                // * The pool doesn't accept the client (Pool error)
                // * The client has handled 32 sessions (Garbage collection of apr memory)

                _pool = null;
                InnerDispose();
            }
        }

        /// <summary>
        /// Calls the original dispose method
        /// </summary>
        protected void InnerDispose()
        {
            base.Dispose(); // Includes GC.SuppressFinalize()
        }
    }

    public abstract class GitPoolRemoteSession : SvnRemoteSession, IDisposable
    {
        IGitClientPool _pool;
        int _nReturns;
        Uri _repositoryUri;
        bool _shouldDispose;

        // Note: We can only implement our own Dispose over the existing
        // As VC++ unconditionally calls GC.SuppressFinalize() just before returning
        // Luckily the using construct uses the last defined or IDisposable methods which we can override

        protected GitPoolRemoteSession(IGitClientPool pool)
        {
            if (pool == null)
                throw new ArgumentNullException("pool");

            _pool = pool;
        }  

        /// <summary>
        /// Returns the client to the client pool
        /// </summary>
        public new void Dispose()
        {
            ReturnClient();

            // No GC.SuppressFinalize() as SvnClient() really needs to have a finalize if we don't dispose
        }

        void IDisposable.Dispose()
        {
            ReturnClient();

            // No GC.SuppressFinalize() as SvnClient() really needs to have a finalize if we don't dispose
        }

        protected IGitClientPool GitClientPool
        {
            get { return _pool; }
            set { _pool = value; }
        }

        /// <summary>
        /// Returns the client to the threadpool, or disposes the cleint
        /// </summary>
        protected virtual void ReturnClient()
        {
            // In our common implementation this code is not used
            // Update VisualGitSvnClientPool.VisualGitSvnPoolClient

            if (_shouldDispose || _nReturns++ > 1024 || IsCommandRunning || !_pool.ReturnClient(this))
            {
                // Recycle the GitPoolRemoteSession if at least one of the following is true
                // * A command is still active in it (User error)
                // * The pool doesn't accept the client (Pool error)
                // * The client has handled 1024 sessions (Garbage collection of apr memory)

                _pool = null;
                InnerDispose();
            }
        }

        /// <summary>
        /// Calls the original dispose method
        /// </summary>
        protected void InnerDispose()
        {
            base.Dispose(); // Includes GC.SuppressFinalize()
        }

        /// <summary>
        /// The repository root url, or NULL if unavailable
        /// </summary>
        public Uri RepositoryRootUri
        {
            get
            {
                if (_repositoryUri == null && !IsDisposed)
                {
                    SvnRemoteCommonArgs rca = new SvnRemoteCommonArgs();
                    rca.ThrowOnError = false;
                    if (!GetRepositoryRoot(rca, out _repositoryUri))
                    {
                        _repositoryUri = null;
                        _shouldDispose = true;
                    }
                }

                return _repositoryUri;
            }
        }

        public new string MakeRelativePath(Uri uri)
        {
            string uriNormalized = SvnTools.GetNormalizedUri(uri).AbsoluteUri;
            string sessionNormalized = SvnTools.GetNormalizedUri(SessionUri).AbsoluteUri;

            if (!uriNormalized.StartsWith(sessionNormalized, StringComparison.Ordinal))
                throw new ArgumentException("Url not below session root");

            if (uriNormalized.Length == sessionNormalized.Length)
                return "";
            else if (uriNormalized.Length < sessionNormalized.Length || uriNormalized[sessionNormalized.Length] != '/')
                throw new ArgumentException("Url not below session root");

            string rest = uriNormalized.Substring(sessionNormalized.Length + 1);

            return Uri.UnescapeDataString(rest);
        }
    }
}
