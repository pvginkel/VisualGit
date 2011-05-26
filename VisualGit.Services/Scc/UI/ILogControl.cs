using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Scc.UI
{
    public interface ILogControl
    {
        bool ShowChangedPaths { get; set; }
        bool ShowLogMessage { get; set; }
        bool StrictNodeHistory { get; set; }
        bool IncludeMergedRevisions { get; set; }
        void FetchAll();
        void Restart();

        IList<GitOrigin> Origins { get; }        
    }
}
