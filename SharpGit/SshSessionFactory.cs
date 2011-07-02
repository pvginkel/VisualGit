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
