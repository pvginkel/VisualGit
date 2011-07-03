// VisualGit.Services\IVisualGitScheduler.cs
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
    // This class is defined in VisualGit.Ids, because its implementation is in VisualGit.Trigger, which has
    // no other external dependencies than VisualGit.Ids
    public interface IVisualGitScheduler
    {
        /// <summary>
        /// Schedules the specified command at or after the specified time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="command"></param>
        int ScheduleAt(DateTime time, VisualGitCommand command);
        /// <summary>
        /// Schedules the specified command at or after the specified time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="dlg"></param>
        /// <param name="args"></param>
        int ScheduleAt(DateTime time, VisualGitAction action);
        /// <summary>
        /// Schedules the specified command at or after the specified interval
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="command"></param>
        int Schedule(TimeSpan timeSpan, VisualGitCommand command);
        /// <summary>
        /// Schedules the specified command at or after the specified interval
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <param name="command"></param>
        int Schedule(TimeSpan timeSpan, VisualGitAction action);

        /// <summary>
        /// Removes the specified task from the scheduler
        /// </summary>
        /// <param name="taskId"></param>
        bool RemoveTask(int taskId);
    }
}
