// VisualGit.Services\Commands\BaseCommandEventArgs.cs
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;

using VisualGit.Selection;

namespace VisualGit.Commands
{
    public class BaseCommandEventArgs : EventArgs
    {
        readonly VisualGitCommand _command;
        readonly VisualGitContext _context;
        CommandMapItem _mapItem;

        public BaseCommandEventArgs(VisualGitCommand command, VisualGitContext context)
        {
            _command = command;
            _context = context;
        }

        public VisualGitCommand Command
        {
            [DebuggerStepThrough]
            get { return _command; }
        }

        public VisualGitContext Context
        {
            [DebuggerStepThrough]
            get { return _context; }
        }

        ISelectionContext _selection;
        IVisualGitCommandStates _state;
        /// <summary>
        /// Gets the Visual Studio selection
        /// </summary>
        /// <value>The selection.</value>        
        public ISelectionContext Selection
        {
            [DebuggerStepThrough]
            get { return _selection ?? (_selection = GetService<ISelectionContext>()); }
        }

        /// <summary>
        /// Gets the command states.
        /// </summary>
        /// <value>The state.</value>
        public IVisualGitCommandStates State
        {
            [DebuggerStepThrough]
            get { return _state ?? (_state = GetService<IVisualGitCommandStates>()); }
        }

        [GuidAttribute("3C536122-57B1-46DE-AB34-ACC524140093")]
        sealed class SVsExtensibility
        {
        }
        /// <summary>
        /// Gets a value indicating whether this instance is in automation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is in automation; otherwise, <c>false</c>.
        /// </value>
        public bool IsInAutomation
        {
            get
            {
                IVsExtensibility3 extensibility = GetService<IVsExtensibility3>(typeof(SVsExtensibility));

                if (extensibility != null)
                {
                    int inAutomation;

                    if (extensibility.IsInAutomationFunction(out inAutomation) == 0)
                        return inAutomation != 0;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return Context.GetService<T>();
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return Context.GetService<T>(serviceType);
        }

        internal void Prepare(CommandMapItem item)
        {
            _mapItem = item;
        }
    }
}
