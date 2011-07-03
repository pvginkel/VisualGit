// SharpGit\GitInternalStatus.cs
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

namespace SharpGit
{
    [Flags]
    public enum GitInternalStatus
    {
        /// <summary>
        /// Unknown state.
        /// </summary>
        Unset = 0,
        /// <summary>
        /// New, staged file.
        /// </summary>
        Added = 1,
        AssumeUnchanged = 2,
        /// <summary>
        /// Modified, staged file.
        /// </summary>
        Changed = 4,
        /// <summary>
        /// Modified, unstaged file.
        /// </summary>
        Modified = 8,
        /// <summary>
        /// Deleted, unstaged file.
        /// </summary>
        Missing = 16,
        /// <summary>
        /// Deleted, staged file.
        /// </summary>
        Removed = 32,
        /// <summary>
        /// New, unstaged file.
        /// </summary>
        Untracked = 64,
        /// <summary>
        /// Ignored file.
        /// </summary>
        Ignored = 128,
        /// <summary>
        /// Conflicted.
        /// </summary>
        Conflicted = 256
    }
}
