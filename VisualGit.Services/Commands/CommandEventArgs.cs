// VisualGit.Services\Commands\CommandEventArgs.cs
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
using VisualGit.Selection;

namespace VisualGit.Commands
{
    public class CommandEventArgs : BaseCommandEventArgs
    {
        readonly object _argument;
        object _result;
        bool _promptUser;
        bool _dontPromptUser;

        public CommandEventArgs(VisualGitCommand command, VisualGitContext context)
            : base(command, context)
        {
        }

        public CommandEventArgs(VisualGitCommand command, VisualGitContext context, object argument, bool promptUser, bool dontPromptUser)
            : this(command, context)
        {
            _argument = argument;
            _promptUser = promptUser;
            _dontPromptUser = dontPromptUser;
        }

        public object Argument
        {
            get { return _argument; }
        }

        public object Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public bool DontPrompt
        {
            get { return _dontPromptUser; }
        }

        public bool PromptUser
        {
            get { return _promptUser; }
        }
    }

}
