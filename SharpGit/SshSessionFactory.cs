using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    internal class SshSessionFactory : JschConfigSessionFactory
    {
        public static SshSessionFactory Instance = new SshSessionFactory();

        protected override NSch.JSch GetJSch(OpenSshConfig.Host hc, NGit.Util.FS fs)
        {
            var jsch = base.GetJSch(hc, fs);

            foreach (var certificate in GitCertificates.GetAllCertificates())
            {
                jsch.AddIdentity(certificate.Path);
            }

            return jsch;
        }

        protected override void Configure(OpenSshConfig.Host hc, NSch.Session session)
        {
        }
    }
}
