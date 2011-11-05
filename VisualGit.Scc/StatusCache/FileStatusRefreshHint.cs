using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGit;

namespace VisualGit.Scc.StatusCache
{
    public class FileStatusRefreshHint : IDisposable
    {
        public GitDepth Depth { get; private set; }

        [ThreadStatic]
        private static List<FileStatusRefreshHint> _stack;

        public static FileStatusRefreshHint Current
        {
            get
            {
                if (_stack == null || _stack.Count == 0)
                    return null;
                else
                    return _stack[_stack.Count - 1];
            }
        }

        private bool _disposed;

        public FileStatusRefreshHint(GitDepth depth)
        {
            Depth = depth;

            if (_stack == null)
                _stack = new List<FileStatusRefreshHint>();

            _stack.Add(this);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stack.Remove(this);

                _disposed = true;
            }
        }
    }
}
