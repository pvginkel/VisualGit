using System.Collections.Generic;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    public interface IGitItem
    {
        string Revision { get; }
        string Name { get; }
    }
}
