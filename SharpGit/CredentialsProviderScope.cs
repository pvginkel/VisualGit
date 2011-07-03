// SharpGit\CredentialsProviderScope.cs
//
// Copyright 2008-2011 The AnkhSVN Project
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//
// Changes and additions made for VisualGit Copyright 2011 Pieter van Ginkel.

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
