// VisualGit.Services\IVisualGitServiceImplementation.cs
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

namespace VisualGit
{
    public interface IVisualGitServiceImplementation
    {
        /// <summary>
        /// Called when the service is instantiated
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void OnPreInitialize();

        /// <summary>
        /// Called after all modules and services received their OnPreInitialize
        /// </summary>
        /// <param name="e"></param>
        void OnInitialize();
    }
}
