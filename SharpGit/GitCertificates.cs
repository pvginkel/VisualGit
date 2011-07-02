using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpGit
{
    public static class GitCertificates
    {
        private static readonly object _syncLock = new object();
        private static readonly Dictionary<string, GitCertificate> _certificates = new Dictionary<string, GitCertificate>(FileSystemUtil.StringComparer);

        public static IEnumerable<GitCertificate> GetAllCertificates()
        {
            return new List<GitCertificate>(_certificates.Values);
        }

        public static void AddCertificate(GitCertificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            _certificates[certificate.Path] = certificate;
        }

        public static void RemoveCertificate(GitCertificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            _certificates.Remove(certificate.Path);
        }
    }
}
