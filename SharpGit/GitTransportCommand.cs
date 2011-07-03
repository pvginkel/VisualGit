// SharpGit\GitTransportCommand.cs
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
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    internal abstract class GitTransportCommand<T> : GitCommand<T>
        where T : GitTransportClientArgs
    {
        protected GitTransportCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        protected class CredentialsProvider : NGit.Transport.CredentialsProvider
        {
            private readonly GitTransportCommand<T> _command;

            public CredentialsProvider(GitTransportCommand<T> command)
            {
                _command = command;
            }

            public override bool Get(URIish uri, params CredentialItem[] items)
            {
                var e = new GitCredentialsEventArgs(uri.ToString(), items);

                _command.Args.OnCredentials(e);

                return !e.Cancel;
            }

            public override bool IsInteractive()
            {
                return true;
            }

            public override bool Supports(params CredentialItem[] items)
            {
                var e = new GitCredentialsEventArgs(null, items);

                _command.Args.OnCredentialsSupported(e);

                return !e.Cancel;
            }
        }

        protected class ProgressMonitor : NGit.ProgressMonitor
        {
            private readonly GitTransportCommand<T> _command;
            private string _currentTask;
            private int _totalTasks;
            private int _currentTaskLength;
            private int _currentTaskProgress;
            private bool _cancelled;

            public ProgressMonitor(GitTransportCommand<T> command)
            {
                _command = command;
                _totalTasks = -1;
                _currentTaskLength = -1;
                _currentTaskProgress = -1;
            }

            public override void BeginTask(string title, int totalWork)
            {
                _currentTask = title;
                _currentTaskLength = totalWork;
                _currentTaskProgress = -1;

                RaiseUpdate();
            }

            public override void Update(int completed)
            {
                if (_currentTaskProgress == -1)
                    _currentTaskProgress = completed;
                else
                    _currentTaskProgress += completed;

                RaiseUpdate();
            }

            public override void EndTask()
            {
                _currentTask = null;
                _currentTaskLength = -1;
                _currentTaskProgress = -1;

                RaiseUpdate();
            }

            public override bool IsCancelled()
            {
                return _cancelled;
            }

            public override void Start(int totalTasks)
            {
                _totalTasks = totalTasks;

                RaiseUpdate();
            }

            private void RaiseUpdate()
            {
                var e = new GitProgressEventArgs(_currentTask, _currentTaskLength, _currentTaskProgress, _totalTasks);

                _command.Args.OnProgress(e);

                if (e.Cancel)
                    _cancelled = e.Cancel;
            }
        }
    }
}
