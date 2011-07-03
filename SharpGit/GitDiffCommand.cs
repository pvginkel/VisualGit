// SharpGit\GitDiffCommand.cs
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

                    var revWalk = new RevWalk(repository);

                    try
                    {
                        parser.Reset(GetReader(repository), revWalk.ParseTree(objectId).Id);

                        return parser;
                    }
                    finally
                    {
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
