using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    internal class CredentialsProviderScope : IDisposable
    {
        [ThreadStatic]
        private static CredentialsProvider _currentProvider;

        private bool _disposed;
        private CredentialsProvider _lastProvider;

        public CredentialsProviderScope(CredentialsProvider credentialsProvider)
        {
            if (credentialsProvider == null)
                throw new ArgumentNullException("credentialsProvider");

            _lastProvider = _currentProvider;
            _currentProvider = credentialsProvider;
        }

        public static CredentialsProvider Current { get { return _currentProvider; } }

        public void Dispose()
        {
            if (!_disposed)
            {
                _currentProvider = _lastProvider;

                _disposed = true;
            }
        }
    }
}
