// SharpGit\GitPullCommand.cs
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
using NGit;
using NGit.Api;
using NGit.Transport;
using NGit.Api.Errors;
using NGit.Merge;

namespace SharpGit
{
    internal class GitPullCommand : GitTransportCommand<GitPullArgs>
    {
        public GitPullCommand(GitClient client, GitClientArgs args)
            : base(client, args)
        {
        }

        public GitPullResult Execute(string repositoryPath)
        {
            MergeCommandResult mergeResult = null;
            var result = new GitPullResult();

            var monitor = new ProgressMonitor(this);

            monitor.BeginTask(JGitText.Get().pullTaskName, 2);

            var repositoryEntry = Client.GetRepository(repositoryPath);

            using (repositoryEntry.Lock())
            {
                var repository = repositoryEntry.Repository;

                var fetchCommand = new Git(repository).Fetch();

                fetchCommand.SetCheckFetchedObjects(Args.CheckFetchedObjects);

                // Verify the current branch.

                string currentBranch = repository.GetFullBranch();

                if (!currentBranch.StartsWith(Constants.R_HEADS))
                    throw new InvalidOperationException("Current branch is not a HEAD");

                currentBranch = currentBranch.Substring(Constants.R_HEADS.Length);

                // Validate the format of the remote branch.

                if (Args.RemoteBranch != null)
                {
                    if (Args.RemoteBranch.Type != GitRefType.Branch)
                        throw new InvalidOperationException("RemoteBranch must be in the format of a local branch");
                }

                // Get or detect the remote branch.

                var config = repository.GetConfig();

                string remoteUri = null;

                if (Args.RemoteUri != null)
                {
                    remoteUri = Args.RemoteUri;

                    fetchCommand.SetRemote(remoteUri);

                    if (Args.RemoteBranch != null)
                        fetchCommand.SetRefSpecs(new RefSpec("+" + Args.RemoteBranch.ShortName));
                }
                else
                {
                    string remote = Args.Remote;

                    if (remote == null)
                    {
                        remote = config.GetString(
                            ConfigConstants.CONFIG_BRANCH_SECTION,
                            currentBranch,
                            ConfigConstants.CONFIG_KEY_REMOTE
                        );
                    }

                    if (remote == null)
                        throw new InvalidOperationException("Remote was not specified and the current branch does not specify a remote");
                    if (remote == ".")
                        throw new InvalidOperationException("Please specify a remote other than the local");

                    remoteUri = config.GetString(
                        ConfigConstants.CONFIG_REMOTE_SECTION,
                        remote,
                        ConfigConstants.CONFIG_KEY_URL
                    );

                    if (remoteUri == null)
                    {
                        throw new InvalidOperationException(String.Format(
                            "Remote {0} does not specify an url in its configuration", remote
                        ));
                    }

                    fetchCommand.SetRemote(remote);

                    // Create a valid refspec if a remote branch was specified.

                    if (Args.RemoteBranch != null)
                    {
                        var remoteBranch = GitRef.Create(remote + "/" + currentBranch, GitRefType.RemoteBranch);

                        fetchCommand.SetRefSpecs(new RefSpec("+" + Args.RemoteBranch.Name + ":" + remoteBranch.Name));
                    }
                }

                // Verify we have a remote branch name.

                string remoteBranchName = config.GetString(
                    ConfigConstants.CONFIG_BRANCH_SECTION,
                    currentBranch,
                    ConfigConstants.CONFIG_KEY_MERGE
                );

                if (remoteBranchName == null)
                    throw new InvalidOperationException("Current branch does not specify a merge branch");

                // Flush the remainder of the options.

                fetchCommand.SetRemoveDeletedRefs(Args.RemoveDeletedRefs);

                switch (Args.TagOption)
                {
                    case GitPullTagOption.AutoFollow:
                        fetchCommand.SetTagOpt(TagOpt.AUTO_FOLLOW);
                        break;

                    case GitPullTagOption.FetchTags:
                        fetchCommand.SetTagOpt(TagOpt.FETCH_TAGS);
                        break;

                    case GitPullTagOption.NoTags:
                        fetchCommand.SetTagOpt(TagOpt.NO_TAGS);
                        break;
                }

                fetchCommand.SetProgressMonitor(monitor);

                if (monitor.IsCancelled())
                    throw new GitOperationCancelledException();

                FetchResult fetchResult;

                using (new CredentialsProviderScope(new CredentialsProvider(this)))
                {
                    fetchResult = fetchCommand.Call();
                }

                monitor.Update(1);

                if (monitor.IsCancelled())
                    throw new GitOperationCancelledException();

                var commitId = GetCommitToMerge(fetchResult, remoteBranchName);

                if (commitId != null)
                {
                    result.Commit = commitId.Name;
                }

                var mergeStrategy = Args.MergeStrategy;

                if (mergeStrategy == GitMergeStrategy.DefaultForBranch)
                {
                    bool doRebase = config.GetBoolean(
                        ConfigConstants.CONFIG_BRANCH_SECTION,
                        currentBranch,
                        ConfigConstants.CONFIG_KEY_REBASE,
                        false
                    );

                    mergeStrategy = doRebase ? GitMergeStrategy.Rebase : GitMergeStrategy.Merge;
                }

                if (mergeStrategy != GitMergeStrategy.Unset && commitId == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "Could not get advertised refs for branch {0}", remoteBranchName
                    ));
                }

                switch (mergeStrategy)
                {
                    case GitMergeStrategy.Merge:
                        monitor.Update(1);

                        mergeResult = PerformMerge(repository, fetchResult, result, commitId, remoteBranchName, remoteUri, monitor);
                        break;

                    case GitMergeStrategy.Rebase:
                        monitor.Update(1);

                        PerformRebase(repository, fetchResult, result, commitId, monitor);
                        break;
                }

                monitor.EndTask();
            }

            if (mergeResult != null)
                RaiseMergeResults(repositoryEntry, mergeResult);

            return result;
        }

        private ObjectId GetCommitToMerge(FetchResult fetchResult, string remoteBranchName)
        {
            Ref advertisedRef = null;

            if (fetchResult != null)
            {
                advertisedRef = fetchResult.GetAdvertisedRef(remoteBranchName);

                if (advertisedRef == null)
                    advertisedRef = fetchResult.GetAdvertisedRef(Constants.R_HEADS + remoteBranchName);
            }

            if (advertisedRef == null)
                return null;
            else
                return advertisedRef.GetObjectId();
        }

        private MergeCommandResult PerformMerge(Repository repository, FetchResult fetchResult, GitPullResult result, ObjectId commitId, string remoteBranchName, string remoteUri, ProgressMonitor monitor)
        {
            var mergeCommand = new Git(repository).Merge();

            string name = "branch \'" + Repository.ShortenRefName(remoteBranchName) + "\' of " + remoteUri;

            mergeCommand.Include(name, commitId);

            var mergeResult = mergeCommand.Call();

            monitor.Update(1);

            result.MergeResult = PackMergeStatus(mergeResult.GetMergeStatus());

            var mergeCommit = mergeResult.GetNewHead();

            if (mergeCommit != null)
                result.Commit = mergeCommit.Name;

            ConvertFailingPaths(result, mergeResult.GetFailingPaths());

            result.Conflicts = mergeResult.GetConflicts();

            return mergeResult;
        }

        private GitMergeResult PackMergeStatus(MergeStatus value)
        {
            switch (value)
            {
                case MergeStatus.ALREADY_UP_TO_DATE: return GitMergeResult.UpToDate;
                case MergeStatus.CONFLICTING: return GitMergeResult.Conflicting;
                case MergeStatus.FAILED: return GitMergeResult.Failed;
                case MergeStatus.FAST_FORWARD: return GitMergeResult.FastForward;
                case MergeStatus.MERGED: return GitMergeResult.Merged;
                case MergeStatus.NOT_SUPPORTED: return GitMergeResult.NotSupported;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        private void PerformRebase(Repository repository, FetchResult fetchResult, GitPullResult result, ObjectId commitId, ProgressMonitor monitor)
        {
            var rebaseCommand = new Git(repository).Rebase();

            rebaseCommand.SetUpstream(commitId);
            rebaseCommand.SetProgressMonitor(monitor);
            rebaseCommand.SetOperation(RebaseCommand.Operation.BEGIN);

            var rebaseResult = rebaseCommand.Call();

            result.MergeResult = PackRebaseResult(rebaseResult.GetStatus());

            ConvertFailingPaths(result, rebaseResult.GetFailingPaths());
        }

        private void ConvertFailingPaths(GitPullResult result, IDictionary<string, ResolveMerger.MergeFailureReason> failingPaths)
        {
            if (failingPaths != null)
            {
                foreach (var item in failingPaths)
                {
                    result.FailedMergePaths[item.Key] = PackMergeFailureReason(item.Value);
                }
            }
        }

        private GitMergeFailureReason PackMergeFailureReason(ResolveMerger.MergeFailureReason value)
        {
            switch (value)
            {
                case NGit.Merge.ResolveMerger.MergeFailureReason.COULD_NOT_DELETE: return GitMergeFailureReason.CouldNotDelete;
                case NGit.Merge.ResolveMerger.MergeFailureReason.DIRTY_INDEX: return GitMergeFailureReason.DirtyIndex;
                case NGit.Merge.ResolveMerger.MergeFailureReason.DIRTY_WORKTREE: return GitMergeFailureReason.DirtyWorktree;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        private GitMergeResult PackRebaseResult(RebaseResult.Status value)
        {
            switch (value)
            {
                case RebaseResult.Status.ABORTED: return GitMergeResult.Aborted;
                case RebaseResult.Status.FAILED: return GitMergeResult.Failed;
                case RebaseResult.Status.FAST_FORWARD: return GitMergeResult.FastForward;
                case RebaseResult.Status.OK: return GitMergeResult.Success;
                case RebaseResult.Status.STOPPED: return GitMergeResult.Stopped;
                case RebaseResult.Status.UP_TO_DATE: return GitMergeResult.UpToDate;

                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
