// SharpGit\TransportWrapper.cs
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
using NGit;

namespace SharpGit
{
    internal class TransportWrapper : TransportProtocol
    {
        private readonly TransportProtocol _outerProtocol;

        public TransportWrapper(TransportProtocol outerProtocol)
        {
            if (outerProtocol == null)
                throw new ArgumentNullException("outerProtocol");

            _outerProtocol = outerProtocol;
        }

        public override string GetName()
        {
            return _outerProtocol.GetName();
        }

        public override Transport Open(URIish uri, Repository local, string remoteName)
        {
            var transport = _outerProtocol.Open(uri, local, remoteName);

            // Replace the session factory when we've got an SshTransport.

            var sshTransport = transport as SshTransport;

            if (sshTransport != null)
                sshTransport.SetSshSessionFactory(SshSessionFactory.Instance);

            var currentCredentialsProvider = CredentialsProviderScope.Current;

            if (currentCredentialsProvider != null)
                transport.SetCredentialsProvider(currentCredentialsProvider);

            return transport;
        }

        public override bool CanHandle(URIish uri)
        {
            return _outerProtocol.CanHandle(uri);
        }

        public override bool CanHandle(URIish uri, Repository local, string remoteName)
        {
            return _outerProtocol.CanHandle(uri, local, remoteName);
        }

        public override bool Equals(object obj)
        {
            return _outerProtocol.Equals(obj);
        }

        public override int GetDefaultPort()
        {
            return _outerProtocol.GetDefaultPort();
        }

        public override int GetHashCode()
        {
            return _outerProtocol.GetHashCode();
        }

        public override ICollection<URIishField> GetOptionalFields()
        {
            return _outerProtocol.GetOptionalFields();
        }

        public override ICollection<URIishField> GetRequiredFields()
        {
            return _outerProtocol.GetRequiredFields();
        }

        public override ICollection<string> GetSchemes()
        {
            return _outerProtocol.GetSchemes();
        }

        public override string ToString()
        {
            return _outerProtocol.ToString();
        }
    }
}
