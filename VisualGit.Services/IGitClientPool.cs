// VisualGit.Services\IGitClientPool.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

using System;
using System.Collections.Generic;
using System.Text;
using SharpGit;
using System.Diagnostics;

namespace VisualGit
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>The default implementation of this service is thread safe</remarks>
    public interface IGitClientPool
    {
        /// <summary>
        /// Gets a free <see cref="GitClient"/> instance from the pool
        /// </summary>
        /// <returns></returns>
        GitPoolClient GetClient();

        /// <summary>
        /// Gets a free <see cref="GitClient"/> instance from the pool
        /// </summary>
        /// <returns></returns>
        GitPoolClient GetNoUIClient();

        /// <summary>
        /// Returns the client.
        /// </summary>
        /// <param name="poolClient">The pool client.</param>
        /// <returns>true if the pool accepts the client, otherwise false</returns>
        bool ReturnClient(GitPoolClient poolClient);

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
    public abstract class GitPoolClient : GitClient, IDisposable
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

            // No GC.SuppressFinalize() as GitClient() really needs to have a finalize if we don't dispose
        }

        void IDisposable.Dispose()
        {
            ReturnClient();

            // No GC.SuppressFinalize() as GitClient() really needs to have a finalize if we don't dispose
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
            // Update VisualGitGitClientPool.VisualGitGitPoolClient

            if (_nReturns++ > 32 || IsCommandRunning || !_pool.ReturnClient(this))
            {
                // Recycle the GitClient if at least one of the following is true
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
}
