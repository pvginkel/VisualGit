// VisualGit.Scc\StatusCache\Commands\FileStatusCleanup.cs
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
using VisualGit.Commands;

namespace VisualGit.Scc.StatusCache.Commands
{
    [Command(VisualGitCommand.FileCacheFinishTasks, AlwaysAvailable = true)]
    [Command(VisualGitCommand.TickRefreshGitItems, AlwaysAvailable = true)]
    public class FileStatusCleanup : ICommandHandler
    {
        public void OnUpdate(CommandUpdateEventArgs e)
        {
        }

        FileStatusCache _fileCache;
        IVisualGitCommandService _commandService;

        public void OnExecute(CommandEventArgs e)
        {
            if (_commandService == null)
                _commandService = e.GetService<IVisualGitCommandService>();
            if (_fileCache == null)
                _fileCache = e.GetService<FileStatusCache>(typeof(IFileStatusCache));

            _commandService.TockCommand(e.Command);

            if (e.Command == VisualGitCommand.FileCacheFinishTasks)
                _fileCache.OnCleanup();
            else
                _fileCache.BroadcastChanges();
        }
    }
}
