using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class LoadingEventArgs : EventArgs
    {
        public LoadingEventArgs(bool isLoading)
        {
            IsLoading = isLoading;
        }

        public bool IsLoading { get; private set; }
    }
}
