using System;
using System.Collections.Generic;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class GitHead : IGitItem, IComparable<GitHead>
    {
        private readonly string _mergeSettingName;
        private readonly string _remoteSettingName;

        public GitHead(string hash, string completeName) : this(hash, completeName, string.Empty) { }

        public GitHead(string hash, string completeName, string remote)
        {
            Revision = hash;
            Selected = false;
            CompleteName = completeName;
            Remote = remote;
            IsTag = CompleteName.Contains("refs/tags/");
            IsHead = CompleteName.Contains("refs/heads/");
            IsRemote = CompleteName.Contains("refs/remotes/");
            IsBisect = CompleteName.Contains("refs/bisect/");

            ParseName();

            _remoteSettingName = String.Format("branch.{0}.remote", Name);
            _mergeSettingName = String.Format("branch.{0}.merge", Name);
        }

        public string CompleteName { get; private set; }
        public bool Selected { get; set; }

        public bool IsTag { get; private set; }

        public bool IsHead { get; private set; }

        public bool IsRemote { get; private set; }

        public bool IsBisect { get; private set; }

        public bool IsOther
        {
            get { return !IsHead && !IsRemote && !IsTag; }
        }

        public string LocalName
        {
            get { return IsRemote ? Name.Substring(Remote.Length + 1) : Name; }
        }

        public string Remote { get; private set; }

        public static GitHead NoHead
        {
            get { return new GitHead(null, ""); }
        }

        public static GitHead AllHeads
        {
            get { return new GitHead(null, "*"); }
        }

        #region IGitItem Members

        public string Revision { get; private set; }
        public string Name { get; private set; }

        #endregion

        public override string ToString()
        {
            return CompleteName;
        }

        private void ParseName()
        {
            if (CompleteName.Length == 0 || !CompleteName.Contains("/"))
            {
                Name = CompleteName;
                return;
            }
            if (IsRemote)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("remotes/") + 8);
                return;
            }
            if (IsTag)
            {
                // we need the one containing ^{}, because it contains the reference
                var temp =
                    CompleteName.Contains("^{}")
                        ? CompleteName.Substring(0, CompleteName.Length - 3)
                        : CompleteName;

                Name = temp.Substring(CompleteName.LastIndexOf("tags/") + 5);
                return;
            }
            if (IsHead)
            {
                Name = CompleteName.Substring(CompleteName.LastIndexOf("heads/") + 6);
                return;
            }
            Name = CompleteName.Substring(CompleteName.LastIndexOf("/") + 1);
        }

        public int CompareTo(GitHead other)
        {
            if (this.IsTag != other.IsTag)
                return other.IsTag.CompareTo(this.IsTag);
            if (this.IsRemote != other.IsRemote)
                return other.IsRemote.CompareTo(this.IsRemote);
            return other.Name.CompareTo(this.Name);
        }
    }
}