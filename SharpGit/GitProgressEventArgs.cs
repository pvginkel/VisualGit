using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SharpGit
{
    public class GitProgressEventArgs : CancelEventArgs
    {
        public GitProgressEventArgs(string currentTask, int taskLength, int taskProgress, int totalTasks)
        {
            CurrentTask = currentTask;
            TaskLength = taskLength;
            TaskProgress = taskProgress;
            TotalTasks = totalTasks;
        }

        public string CurrentTask { get; private set; }

        public int TaskLength { get; private set; }

        public int TaskProgress { get; private set; }

        public int TotalTasks { get; private set; }
    }
}
