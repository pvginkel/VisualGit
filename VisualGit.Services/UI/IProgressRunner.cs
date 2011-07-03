// VisualGit.Services\UI\IProgressRunner.cs
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
using System.ComponentModel;
using SharpGit;

namespace VisualGit
{
    public class ProgressWorkerArgs : EventArgs
    {
        readonly IVisualGitServiceProvider _context;
        readonly GitClient _client;
        readonly ISynchronizeInvoke _sync;
        Exception _exception;

        public ProgressWorkerArgs(IVisualGitServiceProvider context, GitClient client, ISynchronizeInvoke sync)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _context = context;
            _client = client;
            _sync = sync;
        }

        public GitClient Client
        {
            get { return _client; }
        }

        public IVisualGitServiceProvider Context
        {
            get { return _context; }
        }

        public Exception Exception
        {
            get { return _exception; }
            set { _exception = value; }
        }

        public ISynchronizeInvoke Synchronizer
        {
            get { return _sync; }
        }
    }

    public class ProgressRunnerResult
    {
        readonly bool _succeeded;
        readonly Exception _ex;

        public ProgressRunnerResult(bool succeeded)
        {
            _succeeded = succeeded;
        }

        public ProgressRunnerResult(bool succeeded, Exception e)
        {
            _succeeded = succeeded;
            _ex = e;
        }

        public bool Succeeded
        {
            get { return _succeeded; }
        }

        public Exception Exception
        {
            get { return _ex; }
        }
    }

    public class ProgressWorkerDoneArgs : EventArgs
    {
        readonly ProgressRunnerResult _result;
        public ProgressWorkerDoneArgs(ProgressRunnerResult result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            _result = result;
        }

        public ProgressRunnerResult Result
        {
            get { return _result; }
        }
    }

    public class ProgressRunnerArgs
    {
        bool _createLog;
        GitTransportClientArgs _transportClientArgs;

        public bool CreateLog
        {
            get { return _createLog; }
            set { _createLog = value; }
        }

        public GitTransportClientArgs TransportClientArgs
        {
            get { return _transportClientArgs; }
            set { _transportClientArgs = value; }
        }
    }

    public interface IProgressRunner
    {
        /// <summary>
        /// Runs the specified action.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        ProgressRunnerResult RunModal(string caption, EventHandler<ProgressWorkerArgs> action);

        /// <summary>
        /// Runs the specified action.
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="args">The args.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        ProgressRunnerResult RunModal(string caption, ProgressRunnerArgs args, EventHandler<ProgressWorkerArgs> action);

        /// <summary>
        /// Runs the specified action and when the action completes completer. (Currently implemented synchronously!)
        /// </summary>
        /// <param name="caption">The caption.</param>
        /// <param name="action">The action.</param>
        /// <param name="completer">The completer.</param>
        void RunNonModal(string caption, EventHandler<ProgressWorkerArgs> action, EventHandler<ProgressWorkerDoneArgs> completer);
    }
}
