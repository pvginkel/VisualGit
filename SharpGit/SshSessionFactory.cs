// SharpGit\SshSessionFactory.cs
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
using System.IO;

namespace SharpGit
{
    internal class SshSessionFactory : JschConfigSessionFactory
    {
        private static readonly object _syncLock = new object();
        private static string _knownHostsFilename;

        public static SshSessionFactory Instance = new SshSessionFactory();

        protected override NSch.JSch GetJSch(OpenSshConfig.Host hc, NGit.Util.FS fs)
        {
            var jsch = base.GetJSch(hc, fs);

            foreach (var certificate in GitCertificates.GetAllCertificates())
            {
                jsch.AddIdentity(certificate.Path);
            }

            // Set the known hosts file.

            jsch.SetKnownHosts(GetKnownHostsFilename());

            return jsch;
        }

        private string GetKnownHostsFilename()
        {
            lock (_syncLock)
            {
                if (_knownHostsFilename == null)
                {
                    string userHome = Environment.GetFolderPath(
                        Environment.SpecialFolder.UserProfile
                    );

                    string sshFolder = Path.Combine(userHome, ".ssh");

                    if (!Directory.Exists(sshFolder))
                    {
                        DirectoryInfo directoryInfo = Directory.CreateDirectory(sshFolder);
                        directoryInfo.Attributes |= FileAttributes.Hidden;
                    }

                    _knownHostsFilename = Path.Combine(sshFolder, "known_hosts");

                    if (!File.Exists(_knownHostsFilename))
                        File.WriteAllBytes(_knownHostsFilename, new byte[0]);
                }

                return _knownHostsFilename;
            }
        }

        protected override void Configure(OpenSshConfig.Host hc, NSch.Session session)
        {
        }
    }
}
