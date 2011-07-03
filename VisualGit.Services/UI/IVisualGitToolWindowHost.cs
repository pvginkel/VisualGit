// VisualGit.Services\UI\IVisualGitToolWindowHost.cs
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
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using Microsoft.VisualStudio.Shell.Interop;
using VisualGit.UI.Services;

namespace VisualGit.UI
{
    [CLSCompliant(false)]
    public interface IVisualGitToolWindowHost : IVisualGitUISite
    {
        IVsWindowFrame Frame { get; }
        IVsWindowPane Pane { get; }

        /// <summary>
        /// Gets or sets the keyboard context of the frame window
        /// </summary>
        Guid KeyboardContext { get; set; }
        /// <summary>
        /// Gets or sets the command context of the frame window
        /// </summary>
        Guid CommandContext { get; set; }

		/// <summary>
		/// Gets a boolean indicating whether the toolwindow is on screen
		/// </summary>
		bool IsOnScreen { get; }
	}
}
