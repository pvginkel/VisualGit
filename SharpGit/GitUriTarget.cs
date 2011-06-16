using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpGit
{
    public sealed class GitUriTarget : GitTarget
    {
        readonly Uri _uri;

        public GitUriTarget(Uri uri, GitRevision revision)
            : base(revision)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            else if (!uri.IsAbsoluteUri)
                throw new ArgumentException("Uri is not absolute", "uri");
            else if (!IsValidReposUri(uri))
                throw new ArgumentException("Uri is not a valid repository uri", "uri");

            _uri = CanonicalizeUri(uri);
        }

        public GitUriTarget(Uri uri)
            : this(uri, GitRevision.None)
        {
        }

        public GitUriTarget(string uriString, GitRevision revision)
            : this(new Uri(uriString), revision)
        {
        }

        public GitUriTarget(string uriString)
            : this(uriString, GitRevision.None)
        {
        }

        public GitUriTarget(Uri uri, string revision)
            : this(uri, new GitRevision(revision))
        {
        }

        public GitUriTarget(Uri uri, DateTime date)
            : this(uri, new GitRevision(date))
        {
        }

        public static new GitUriTarget FromUri(Uri value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return new GitUriTarget(value);
        }

        public static new GitUriTarget FromString(string value)
        {
            return new GitUriTarget(value);
        }

#if false
        public static GitUriTarget FromString(string value, bool allowOperationalRevision);
#endif

        public static implicit operator GitUriTarget(Uri value)
        {
            return value != null ? FromUri(value) : null;
        }

        public static explicit operator GitUriTarget(string value)
        {
            return value != null ? FromString(value) : null;
        }

        public Uri Uri { get { return _uri; } }

        public override string TargetName { get { return _uri.ToString(); } }

        public override string FileName { get { return GetFileName(_uri); } }

        private string GetFileName(Uri target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            target = CanonicalizeUri(target);

            string path = target.AbsolutePath;

            int nEnd = path.LastIndexOf('?');

            if (nEnd < 0)
                nEnd = path.Length - 1;

            if (path.Length > 0 && nEnd <= path.Length && path[nEnd] == '/')
                nEnd--;

            if (nEnd <= 0)
                return "";

            int nStart = path.LastIndexOf('/', nEnd);

            if (nStart >= 0)
                nStart++;
            else
                nStart = 0;

            path = path.Substring(nStart, nEnd - nStart + 1);

            return UriPartToPath(path);
        }

        public static string UriPartToPath(string uriPath)
        {
            if (uriPath == null)
                throw new ArgumentNullException("uriPath");

            return Uri.UnescapeDataString(uriPath).Replace('/', Path.DirectorySeparatorChar);
        }

        internal override string GitTargetName
        {
            get { return UriToCanonicalString(_uri); }
        }

#if false
        public static bool TryParse(string path, out GitUriTarget pegUri)
        {
            return TryParse(path, false, out pegUri);
        }

        public static bool TryParse(string path, bool allowOperationalRevision, out GitUriTarget pegUri)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            throw new NotImplementedException();
        }
#endif

        public static ICollection<GitUriTarget> Map(IEnumerable<Uri> uris)
        {
            var result = new List<GitUriTarget>();

            foreach (var uri in uris)
            {
                result.Add(uri);
            }

            return result;
        }

        internal void VerifyBelowRoot(Uri repositoryRoot)
        {
            // TODO: Throw exception if the current value is not below the repository root
        }

        internal static Uri CanonicalizeUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            else if (!uri.IsAbsoluteUri)
                throw new ArgumentException("Uri is not absolute", "uri");

            string path = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);

            bool schemeOk = !ContainsUpper(uri.Scheme) && !ContainsUpper(uri.Host);

            if (schemeOk && (path.Length == 0 || (path[path.Length - 1] != '/' && path.IndexOf('\\') < 0) && !path.Contains("//")))
                return uri;

            string components = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.SafeUnescaped);
            if (!schemeOk)
            {
                int nStart = components.IndexOf(':');

                if (nStart > 0)
                {
                    // Subversion 1.6 will require scheme and hostname in lowercase
                    for (int i = 0; i < nStart; i++)
                        if (!Char.IsLower(components, i))
                        {
                            components = components.Substring(0, nStart).ToLowerInvariant() + components.Substring(nStart);
                            break;
                        }

                    int nAt = components.IndexOf('@', nStart);

                    if (nAt < 0)
                        nAt = nStart + 2;
                    else
                        nAt++;

                    for (int i = nAt; i < components.Length; i++)
                        if (!Char.IsLower(components, i))
                        {
                            components = components.Substring(0, nAt) + components.Substring(nAt) + 1;
                            break;
                        }
                }
            }

            // Create a new uri with all / and \ characters at the end removed
            Uri root;
            Uri suffix;

            if (!Uri.TryCreate(components, UriKind.Absolute, out root))
                throw new ArgumentException("Invalid Uri value in scheme or server", "uri");

            string part = RemoveDoubleSlashes("/" + path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));

            if (root.IsFile)
            {
                if (part.Length >= 2 && part[1] == ':' && part[0] >= 'a' && part[0] <= 'z')
                {
                    part = Char.ToUpperInvariant(part[0]) + part.Substring(1);

                    if (part.Length == 2)
                        part += '/';
                }
                else if (uri.Host != null)
                {
                    part = part.TrimStart('/');
                }
            }

            if (!Uri.TryCreate(part, UriKind.Relative, out suffix))
                throw new ArgumentException("Invalid Uri value in path", "uri");

            Uri result;
            if (Uri.TryCreate(root, suffix, out result))
                return result;
            else
                return uri;
        }

        private static string RemoveDoubleSlashes(string input)
        {
            int n;

            while (0 <= (n = input.IndexOf("//", StringComparison.Ordinal)))
                input = input.Remove(n, 1);

            return input;
        }

        private static bool ContainsUpper(string value)
        {
            foreach (char c in value)
            {
                if (Char.IsUpper(c))
                    return true;
            }

            return false;
        }

        private bool IsValidReposUri(System.Uri uri)
        {
            return uri.IsAbsoluteUri && !string.IsNullOrEmpty(uri.Scheme);
        }

        string UriToCanonicalString(Uri value)
        {
            if (value == null)
                return null;

            string name = UriToString(CanonicalizeUri(value));

            if (name != null && name.Length > 0 && (name[name.Length - 1] == '/'))
                return name.TrimEnd('/'); // "svn://host:port" is canoncialized to "svn://host:port/" by the .Net Uri class
            else
                return name;
        }


        string UriToString(Uri value)
        {
            if (value == null)
                return null;

            if (value.IsAbsoluteUri)
                return value.GetComponents(
                UriComponents.SchemeAndServer |
                UriComponents.UserInfo |
                UriComponents.Path, UriFormat.UriEscaped);
            else
                return Uri.EscapeUriString(value.ToString()); // Escape back to valid uri form
        }


        internal override GitRevision GetGitRevision(GitRevision fileNoneValue, GitRevision uriNoneValue)
        {
            if (Revision.RevisionType != GitRevisionType.None)
                return Revision;
            else
                return uriNoneValue;
        }
    }
}
