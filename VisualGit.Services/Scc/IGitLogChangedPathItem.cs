using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace VisualGit.Scc
{
	public interface IGitLogChangedPathItem : IGitRepositoryItem
	{
		SvnChangeAction Action { get; }
		string CopyFromPath { get; }
		long CopyFromRevision { get; }
		string Path { get; }
        new long Revision { get; }

        new GitOrigin Origin { get; }
	}
}
