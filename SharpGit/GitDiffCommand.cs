using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NGit.Diff;
using NGit.Patch;
using NGit.Treewalk;
using NGit;
using NGit.Dircache;
using NGit.Revwalk;

namespace SharpGit
{
    internal class GitDiffCommand : GitCommand<GitDiffArgs>
    {
        private ObjectReader _reader;

        public GitDiffCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string fullPath, GitRevisionRange revRange, Stream stream)
        {
            try
            {
                var repositoryEntry = Client.GetRepository(fullPath);

                using (repositoryEntry.Lock())
                {
                    var repository = repositoryEntry.Repository;

                    string result;

                    using (var tmpStream = new MemoryStream())
                    {
                        var diffFormatter = new DiffFormatter(tmpStream);

                        diffFormatter.SetRepository(repository);

                        diffFormatter.SetPathFilter(new CustomPathFilter(
                            repository.GetRepositoryPath(fullPath),
                            Args.Depth
                        ));

                        diffFormatter.Format(
                            GetTree(revRange.StartRevision, repository),
                            GetTree(revRange.EndRevision, repository)
                        );

                        tmpStream.Flush();

                        result = Encoding.UTF8.GetString(tmpStream.ToArray());
                    }

                    var sb = new StringBuilder();

                    for (int i = 0, length = result.Length; i < length; i++)
                    {
                        char c = result[i];

                        switch (c)
                        {
                            case '\r':
                                break;

                            case '\n':
                                sb.Append(Environment.NewLine);
                                break;

                            default:
                                sb.Append(c);
                                break;
                        }
                    }

                    var buffer = Encoding.UTF8.GetBytes(sb.ToString());

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
            finally
            {
                if (_reader != null)
                    _reader.Release();
            }
        }

        private AbstractTreeIterator GetTree(GitRevision revision, Repository repository)
        {
            switch (revision.RevisionType)
            {
                case GitRevisionType.Working:
                    return new FileTreeIterator(repository);

                case GitRevisionType.Base:
                case GitRevisionType.Committed:
                case GitRevisionType.Head:
                    return new DirCacheIterator(repository.ReadDirCache());

                default:
                    var objectId = revision.GetObjectId(repository);

                    if (objectId == null)
                        throw new InvalidOperationException("Could not resolve revision");

                    var parser = new CanonicalTreeParser();

                    RevWalk revWalk = null;

                    try
                    {
                        revWalk = new RevWalk(repository);

                        parser.Reset(GetReader(repository), revWalk.ParseTree(objectId).Id);

                        return parser;
                    }
                    finally
                    {
                        if (revWalk != null)
                            revWalk.Release();
                    }
            }
        }

        private ObjectReader GetReader(Repository repository)
        {
            if (_reader == null)
                _reader = repository.NewObjectReader();

            return _reader;
        }
    }
}
