using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit
{
    public interface IGitStatusManager
    {
        bool InvalidatePath(string path);
        bool GetFileStatus(string path, GitDepth depth, Func<GitFileStatus, bool> callback);
    }
}
