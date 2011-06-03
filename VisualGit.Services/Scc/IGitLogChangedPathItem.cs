using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using SharpGit;

namespace VisualGit.Scc
{
	public interface IGitLogChangedPathItem : IGitRepositoryItem
	{
		GitChangeAction Action { get; }
		string OldPath { get; }
		string OldRevision { get; }
		string Path { get; }
        new string Revision { get; }

        new GitOrigin Origin { get; }
	}
}
