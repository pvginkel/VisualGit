using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public interface IGitOrigin
    {
        GitNodeKind NodeKind { get; }
        Uri RepositoryRoot { get; }
        GitTarget Target { get; }
        Uri Uri { get; }
    }
}
