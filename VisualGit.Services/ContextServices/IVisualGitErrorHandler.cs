using System;
using VisualGit.Commands;

namespace VisualGit
{
    /// <summary>
    /// Represents a class that handles an VisualGit error.
    /// </summary>
    public interface IVisualGitErrorHandler
    {
        bool IsEnabled(Exception ex);

        void OnError(Exception ex);

        void OnError(Exception ex, BaseCommandEventArgs commandInfo);

        void OnWarning(Exception ex);
    }
}
