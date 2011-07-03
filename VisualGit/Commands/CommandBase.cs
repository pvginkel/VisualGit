// VisualGit\Commands\CommandBase.cs
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
using VisualGit.Selection;
using VisualGit.Scc;

namespace VisualGit.Commands
{
    /// <summary>
    /// Base class for ICommand instances
    /// </summary>
    public abstract class CommandBase : ICommandHandler
    {
        public virtual void OnUpdate(CommandUpdateEventArgs e)
        {
            // Just leave the defaults: Enabled          
        }

        public abstract void OnExecute(CommandEventArgs e);

        /// <summary>
        /// Gets whether the Shift key was down when the current window message was send
        /// </summary>
        protected static bool Shift
        {
            get
            {
                return (0 != (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift));
            }
        }

        /// <summary>
        /// Saves all dirty documents within the provided selection
        /// </summary>
        /// <param name="selection">The selection.</param>
        /// <param name="context">The context.</param>
        protected static void SaveAllDirtyDocuments(ISelectionContext selection, IVisualGitServiceProvider context)
        {
            if (selection == null)
                throw new ArgumentNullException("selection");
            if (context == null)
                throw new ArgumentNullException("context");

            IVisualGitOpenDocumentTracker tracker = context.GetService<IVisualGitOpenDocumentTracker>();
            if (tracker != null)
                tracker.SaveDocuments(selection.GetSelectedFiles(true));
        }
    }
}
