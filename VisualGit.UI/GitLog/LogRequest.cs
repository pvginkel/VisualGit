using System;
using System.Collections.Generic;
using System.Text;
using SharpSvn;
using SharpGit;
using System.ComponentModel;

namespace VisualGit.UI.GitLog
{
    class LogRequest
    {
        bool _cancel;
        LogRequest(SvnClientArgs args)
        {
            args.Cancel += new EventHandler<SvnCancelEventArgs>(OnLogCancel);
        }

        LogRequest(GitClientArgs args)
        {
            args.Cancel += new EventHandler<CancelEventArgs>(OnLogCancel);
        }

        public LogRequest(GitLogArgs args, EventHandler<GitLoggingEventArgs> receivedItem)
            : this(args)
        {
            args.Log += ReceiveItem;
            ReceivedItem += receivedItem;
        }

        public LogRequest(SvnMergesMergedArgs args, EventHandler<SvnLoggingEventArgs> receivedItem)
            : this(args)
        {
            args.MergesMerged += ReceiveItem;
            SvnReceivedItem += receivedItem;
        }

        public LogRequest(SvnMergesEligibleArgs args, EventHandler<SvnLoggingEventArgs> receivedItem)
            : this(args)
        {
            args.MergesEligible += ReceiveItem;
            SvnReceivedItem += receivedItem;
        }

        public event EventHandler<SvnLoggingEventArgs> SvnReceivedItem;
        public event EventHandler<GitLoggingEventArgs> ReceivedItem;

        public bool Cancel
        {
            get { return _cancel; }
            set { _cancel = value; }
        }

        void OnLogCancel(object sender, SvnCancelEventArgs e)
        {
            if (_cancel)
                e.Cancel = true;
        }

        void OnLogCancel(object sender, CancelEventArgs e)
        {
            if (_cancel)
                e.Cancel = true;
        }

        void ReceiveItem(object sender, SvnLogEventArgs e)
        {
            if (!_cancel)
                OnReceivedItem(e);
            else
                e.Cancel = true;
        }

        void ReceiveItem(object sender, GitLogEventArgs e)
        {
            if (!_cancel)
                OnReceivedItem(e);
            else
                e.Cancel = true;
        }

        void ReceiveItem(object sender, SvnMergesMergedEventArgs e)
        {
            if (!_cancel)
                OnReceivedItem(e);
            else
                e.Cancel = true;
        }

        void ReceiveItem(object sender, SvnMergesEligibleEventArgs e)
        {
            if (!_cancel)
                OnReceivedItem(e);
            else
                e.Cancel = true;
        }

        void OnReceivedItem(SvnLoggingEventArgs e)
        {
            if (!_cancel && SvnReceivedItem != null)
                SvnReceivedItem(this, e);
        }

        void OnReceivedItem(GitLoggingEventArgs e)
        {
            if (!_cancel && ReceivedItem != null)
                ReceivedItem(this, e);
        }
    }
}
