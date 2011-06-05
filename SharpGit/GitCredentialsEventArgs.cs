using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace SharpGit
{
    public class GitCredentialsEventArgs : CancelEventArgs
    {
        internal GitCredentialsEventArgs(string uri, CredentialItem[] credentialItems)
        {
            if (credentialItems == null)
                throw new ArgumentNullException("credentialItems");

            var items = new List<GitCredentialItem>();

            foreach (var item in credentialItems)
            {
                items.Add(new GitCredentialItem(item));
            }

            Items = new ReadOnlyCollection<GitCredentialItem>(items);

            Uri = uri;
        }

        public string Uri { get; private set; }

        public ICollection<GitCredentialItem> Items { get; private set; }
    }
}
