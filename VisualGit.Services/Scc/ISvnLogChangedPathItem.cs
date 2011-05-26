using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;

namespace VisualGit.Scc
{
	public interface ISvnLogChangedPathItem : ISvnRepositoryItem
	{
		SvnChangeAction Action { get; }
		string CopyFromPath { get; }
		long CopyFromRevision { get; }
		string Path { get; }
        new long Revision { get; }

        new SvnOrigin Origin { get; }
	}
}
