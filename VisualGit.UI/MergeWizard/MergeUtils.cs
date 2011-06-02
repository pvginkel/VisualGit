using System;
using System.Collections.Generic;
using SharpSvn;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    internal class MergeUtils
    {
        public static readonly WizardMessage INVALID_FROM_REVISION = new WizardMessage(MergeStrings.InvalidFromRevision,
            WizardMessage.MessageType.Error);
        public static readonly WizardMessage INVALID_TO_REVISION = new WizardMessage(MergeStrings.InvalidToRevision,
            WizardMessage.MessageType.Error);
        public static readonly WizardMessage INVALID_FROM_URL = new WizardMessage(MergeStrings.InvalidFromUrl,
            WizardMessage.MessageType.Error);
        public static readonly WizardMessage INVALID_TO_URL = new WizardMessage(MergeStrings.InvalidToUrl,
            WizardMessage.MessageType.Error);
        public static readonly WizardMessage NO_FROM_URL = new WizardMessage(MergeStrings.NoFromUrl,
            WizardMessage.MessageType.Error);

        private IVisualGitServiceProvider _context;
        private Dictionary<SvnDepth, string> _mergeDepths;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        public MergeUtils(IVisualGitServiceProvider context)
        {
            Context = context;
        }

        /// <summary>
        /// Returns a key/value pairing of <code>SharpSvn.SvnDepth</code> as the key
        /// and a string description of the depth key.
        /// </summary>
        public Dictionary<SvnDepth, string> MergeDepths
        {
            get
            {
                if (_mergeDepths == null)
                {
                    _mergeDepths = new Dictionary<SvnDepth, string>();

                    _mergeDepths.Add(SvnDepth.Unknown, MergeStrings.GitDepthUnknown);
                    _mergeDepths.Add(SvnDepth.Infinity, MergeStrings.GitDepthInfinity);
                    _mergeDepths.Add(SvnDepth.Children, MergeStrings.GitDepthChildren);
                    _mergeDepths.Add(SvnDepth.Files, MergeStrings.GitDepthFiles);
                    _mergeDepths.Add(SvnDepth.Empty, MergeStrings.GitDepthEmpty);
                }

                return _mergeDepths;
            }
        }

        /// <summary>
        /// Returns a list of strings for the suggested merge sources.
        /// </summary>
        internal SvnMergeSourcesCollection GetSuggestedMergeSources(GitItem target)
        {
            using (SvnClient client = GetClient())
            {
                SvnMergeSourcesCollection mergeSources = null;
                SvnGetSuggestedMergeSourcesArgs args = new SvnGetSuggestedMergeSourcesArgs();

                args.ThrowOnError = false;

                client.GetSuggestedMergeSources(target.FullPath, args, out mergeSources);

                return mergeSources ?? new SvnMergeSourcesCollection();
            }
        }
        
    
        internal SvnMergeItemCollection GetAppliedMerges(GitItem target)
        {
            using (SvnClient client = GetClient())
            {
                SvnGetAppliedMergeInfoArgs args = new SvnGetAppliedMergeInfoArgs();
                SvnAppliedMergeInfo mergeInfo;

                args.ThrowOnError = false;

                if (!client.GetAppliedMergeInfo(target.Uri, args, out mergeInfo))
                    return null;

                return mergeInfo.AppliedMerges;
            }
        }

        /// <summary>
        /// Returns an instance of <code>SharpSvn.SvnClient</code> from the pool.
        /// </summary>
        public SvnClient GetClient()
        {
            ISvnClientPool pool = (Context != null) ? Context.GetService<ISvnClientPool>() : null;

            if (pool != null)
                return pool.GetClient();
            else
                return new SvnClient();
        }

        public SvnWorkingCopyClient GetWcClient()
        {
            ISvnClientPool pool = (Context != null) ? Context.GetService<ISvnClientPool>() : null;

            if (pool != null)
                return pool.GetWcClient();
            else
                return new SvnWorkingCopyClient();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>The context.</value>
        public IVisualGitServiceProvider Context
        {
            get { return _context; }
            set { _context = value; }
        }
    }
}
