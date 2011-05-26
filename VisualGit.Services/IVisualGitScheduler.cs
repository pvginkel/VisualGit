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
