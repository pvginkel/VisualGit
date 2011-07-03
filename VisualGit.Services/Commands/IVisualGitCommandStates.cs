// VisualGit.Services\Commands\IVisualGitCommandStates.cs
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

namespace VisualGit.Commands
{
    public interface IVisualGitCommandStates
    {
        bool UIShellAvailable { get; }

        bool CodeWindow { get; }

        bool Debugging { get; }

        bool DesignMode { get; }

        bool Dragging { get; }

        bool EmptySolution { get; }

        bool FullScreenMode { get; }

        bool NoSolution { get; }

        bool SolutionBuilding { get; }

        bool SolutionExists { get; }

        bool SolutionHasMultipleProjects { get; }

        bool SolutionHasSingleProject { get; }

        bool SccProviderActive { get; }

        bool OtherSccProviderActive { get; }

        bool GetRawOtherSccProviderActive();
    }
}
