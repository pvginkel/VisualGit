// SharpGit\GitRevertCommand.cs
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
using NGit.Api;
using System.Diagnostics;
using NGit.Revwalk;
using NGit;
using NGit.Api.Errors;
using NGit.Merge;
using NGit.Treewalk;
using NGit.Dircache;
using System.IO;

namespace SharpGit
{
    internal class GitRevertCommand : GitCommand<GitRevertArgs>
    {
        public GitRevertCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public void Execute(string repositoryPath, GitRevision revision)
        {
            if (repositoryPath == null)
                throw new ArgumentNullException("repositoryPath");
            if (revision == null)
                throw new ArgumentNullException("revision");

            Debug.Assert(Args.CreateCommit, "RevertCommand currently always create a commit");

            var repositoryEntry = Client.GetRepository(repositoryPath);

            MergeCommandResult mergeResults = null;

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                mergeResults = PerformRevert(revision, repository);

                RaiseNotifyFromDiff(repository);
            }

            if (mergeResults != null)
                RaiseMergeResults(repositoryEntry, mergeResults);
        }

        private MergeCommandResult PerformRevert(GitRevision revision, Repository repository)
        {
            // Below is copied from RevertCommand. There are two changes.
            // Firstly, the commit is now optional. Secondly, we have access
            // to the merge results which we use to report conflicts.

            ObjectId srcObjectId = revision.GetObjectId(repository);
            RevCommit newHead = null;
            RevWalk revWalk = new RevWalk(repository);
            try
            {
                // get the head commit
                Ref headRef = repository.GetRef(Constants.HEAD);
                if (headRef == null)
                {
                    throw new NoHeadException(JGitText.Get().commitOnRepoWithoutHEADCurrentlyNotSupported);
                }
                RevCommit headCommit = revWalk.ParseCommit(headRef.GetObjectId());
                newHead = headCommit;
                // get the commit to be reverted
                RevCommit srcCommit = revWalk.ParseCommit(srcObjectId);
                // get the parent of the commit to revert
                if (srcCommit.ParentCount != 1)
                {
                    throw new MultipleParentsNotAllowedException(JGitText.Get().canOnlyRevertCommitsWithOneParent);
                }
                RevCommit srcParent = srcCommit.GetParent(0);
                revWalk.ParseHeaders(srcParent);
                ResolveMerger merger = (ResolveMerger)((ThreeWayMerger)MergeStrategy.RESOLVE.NewMerger(repository));
                merger.SetWorkingTreeIterator(new FileTreeIterator(repository));
                merger.SetBase(srcCommit.Tree);
                if (merger.Merge(headCommit, srcParent))
                {
                    if (!AnyObjectId.Equals(headCommit.Tree.Id, merger.GetResultTreeId()))
                    {
                        DirCacheCheckout dco = new DirCacheCheckout(repository, headCommit.Tree, repository.LockDirCache
                            (), merger.GetResultTreeId());
                        dco.SetFailOnConflict(true);
                        dco.Checkout();
                        if (Args.CreateCommit)
                        {
                            string newMessage = "Revert \"" + srcCommit.GetShortMessage() + "\"" + "\n\n" + "This reverts commit "
                                + srcCommit.Id.Name + ".\n";
                            newHead = new Git(repository).Commit().SetMessage(newMessage).Call();
                        }
                    }

                    return new MergeCommandResult(newHead.Id, null, new ObjectId[] { headCommit.Id, srcCommit
								.Id }, MergeStatus.MERGED, MergeStrategy.RESOLVE, null, null);
                }
                else
                {
                    var lowLevelResults = merger.GetMergeResults();
                    var failingPaths = merger.GetFailingPaths();
                    var unmergedPaths = merger.GetUnmergedPaths();

                    if (failingPaths != null)
                    {
                        return new MergeCommandResult(null, merger.GetBaseCommit(0, 1), new ObjectId[] { 
									headCommit.Id, srcCommit.Id }, MergeStatus.FAILED, MergeStrategy.RESOLVE, lowLevelResults
                            , failingPaths, null);
                    }
                    else
                    {
                        return new MergeCommandResult(null, merger.GetBaseCommit(0, 1), new ObjectId[] { 
									headCommit.Id, srcCommit.Id }, MergeStatus.CONFLICTING, MergeStrategy.RESOLVE, lowLevelResults
                            , null);
                    }
                }
            }
            catch (IOException e)
            {
                throw new JGitInternalException(String.Format(JGitText.Get().exceptionCaughtDuringExecutionOfRevertCommand, e), e);
            }
            finally
            {
                revWalk.Release();
            }
        }
    }
}
