using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.Scc.UI;
using System.IO;
using System.Diagnostics;
using VisualGit.UI;
using System.Text.RegularExpressions;
using VisualGit.Scc;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using VisualGit.VS;
using SharpGit;

namespace VisualGit.Services
{
    [GlobalService(typeof(IVisualGitDiffHandler))]
    partial class VisualGitDiff : VisualGitService, IVisualGitDiffHandler
    {
        public VisualGitDiff(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        IFileStatusCache _statusCache;
        IFileStatusCache Cache
        {
            get { return _statusCache ?? (_statusCache = GetService<IFileStatusCache>()); }
        }

        /// <summary>
        /// Gets path to the diff executable while taking care of config file settings.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The exe path.</returns>
        protected string GetDiffPath(DiffMode mode)
        {
            IVisualGitConfigurationService cs = GetService<IVisualGitConfigurationService>();

            switch (mode)
            {
                case DiffMode.PreferInternal:
                    return null;
                default:
                    return cs.Instance.DiffExePath;
            }
        }

        public bool RunDiff(VisualGitDiffArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            else if (!args.Validate())
                throw new ArgumentException("Arguments not filled correctly", "args");

            string diffApp = this.GetDiffPath(args.Mode);


            if (string.IsNullOrEmpty(diffApp))
                return RunInternalDiff(args);

            string program;
            string arguments;
            if (!Substitute(diffApp, args, DiffToolMode.Diff, out program, out arguments))
            {
                new VisualGitMessageBox(Context).Show(string.Format("Can't find diff program '{0}'", program ?? diffApp));
                return false;
            }

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(program, arguments);

            string mergedFile = args.MineFile;

            DiffToolMonitor monitor = null;
            if (!string.IsNullOrEmpty(mergedFile))
            {
                monitor = new DiffToolMonitor(Context, mergedFile, false);

                p.EnableRaisingEvents = true;
                monitor.Register(p);
            }

            bool started = false;
            try
            {
                return started = p.Start();
            }
            finally
            {
                if (!started)
                {
                    if (monitor != null)
                        monitor.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets path to the diff executable while taking care of config file settings.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The exe path.</returns>
        protected string GetMergePath(DiffMode mode)
        {
            IVisualGitConfigurationService cs = GetService<IVisualGitConfigurationService>();

            switch (mode)
            {
                case DiffMode.PreferInternal:
                    return null;
                default:
                    return cs.Instance.MergeExePath;
            }
        }

        public bool RunMerge(VisualGitMergeArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            else if (!args.Validate())
                throw new ArgumentException("Arguments not filled correctly", "args");

            string diffApp = this.GetMergePath(args.Mode);

            if (string.IsNullOrEmpty(diffApp))
            {
                new VisualGitMessageBox(Context).Show("Please specify a merge tool in Tools -> Options -> SourceControl -> Git", "VisualGit - No visual merge tool is available");

                return false;
            }

            string program;
            string arguments;
            if (!Substitute(diffApp, args, DiffToolMode.Merge, out program, out arguments))
            {
                new VisualGitMessageBox(Context).Show(string.Format("Can't find merge program '{0}'", program));
                return false;
            }

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(program, arguments);

            string mergedFile = args.MergedFile;

            DiffToolMonitor monitor = null;
            if (!string.IsNullOrEmpty(mergedFile))
            {
                monitor = new DiffToolMonitor(Context, mergedFile, false, args);

                p.EnableRaisingEvents = true;
                monitor.Register(p);
            }

            bool started = false;
            try
            {
                return started = p.Start();
            }
            finally
            {
                if (!started)
                {
                    if (monitor != null)
                        monitor.Dispose();
                }
            }
        }

        protected string GetPatchPath(DiffMode mode)
        {
            IVisualGitConfigurationService cs = GetService<IVisualGitConfigurationService>();

            switch (mode)
            {
                case DiffMode.PreferInternal:
                    return null;
                default:
                    return cs.Instance.PatchExePath;
            }
        }

        public bool RunPatch(VisualGitPatchArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");
            else if (!args.Validate())
                throw new ArgumentException("Arguments not filled correctly", "args");

            string diffApp = GetPatchPath(args.Mode);

            if (string.IsNullOrEmpty(diffApp))
            {
                new VisualGitMessageBox(Context).Show("Please specify a merge tool in Tools->Options->SourceControl->Git", "VisualGit - No visual merge tool is available");

                return false;
            }

            string program;
            string arguments;
            if (!Substitute(diffApp, args, DiffToolMode.Patch, out program, out arguments))
            {
                new VisualGitMessageBox(Context).Show(string.Format("Can't find patch program '{0}'", program));
                return false;
            }

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(program, arguments);

            string applyTo = args.ApplyTo;

            DiffToolMonitor monitor = null;
            if (applyTo != null)
            {
                monitor = new DiffToolMonitor(Context, applyTo, true);

                p.EnableRaisingEvents = true;
                monitor.Register(p);
            }

            bool started = false;
            try
            {
                return started = p.Start();
            }
            finally
            {
                if (!started)
                {
                    if (monitor != null)
                        monitor.Dispose();
                }
            }
        }

        public GitTarget GetCopyOrigin(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            // TODO: We currently do not provide access to copied files.
            // VisualGitStatus.IsCopied/.IsReplaced will never be true, because
            // the copied information currently is only provided through the
            // changed paths interface. This method is only called when
            // .IsCopied/.IsReplaced is true.

            throw new NotSupportedException();
        }

        sealed class DiffToolMonitor : VisualGitService, IVsFileChangeEvents
        {
            uint _cookie;
            readonly string _toMonitor;
            readonly bool _monitorDir;
            readonly VisualGitMergeArgs _args;

            public DiffToolMonitor(IVisualGitServiceProvider context, string monitor, bool monitorDir)
                : this(context, monitor, monitorDir, null)
            {
            }

            public DiffToolMonitor(IVisualGitServiceProvider context, string monitor, bool monitorDir, VisualGitMergeArgs args)
                : base(context)
            {
                if (string.IsNullOrEmpty(monitor))
                    throw new ArgumentNullException("monitor");
                else if (!GitItem.IsValidPath(monitor))
                    throw new ArgumentOutOfRangeException("monitor");

                _monitorDir = monitorDir;
                _args = args;
                _toMonitor = monitor;

                IVsFileChangeEx fx = context.GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));

                _cookie = 0;
                if (fx == null)
                { }
                else if (!_monitorDir)
                {
                    if (!ErrorHandler.Succeeded(fx.AdviseFileChange(monitor,
                            (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size
                            | _VSFILECHANGEFLAGS.VSFILECHG_Add | _VSFILECHANGEFLAGS.VSFILECHG_Del
                            | _VSFILECHANGEFLAGS.VSFILECHG_Attr),
                            this,
                            out _cookie)))
                    {
                        _cookie = 0;
                    }
                }
                else
                {
                    if (!ErrorHandler.Succeeded(fx.AdviseDirChange(monitor, 1, this, out _cookie)))
                    {
                        _cookie = 0;
                    }
                }
            }

            public void Dispose()
            {
                if (_cookie != 0)
                {
                    uint ck = _cookie;
                    _cookie = 0;

                    IVsFileChangeEx fx = GetService<IVsFileChangeEx>(typeof(SVsFileChangeEx));

                    if (fx != null)
                    {
                        if (!_monitorDir)
                            fx.UnadviseFileChange(ck);
                        else
                            fx.UnadviseDirChange(ck);
                    }
                }

                if (_args != null && _args.CleanupFiles != null)
                {
                    foreach (string filename in _args.CleanupFiles)
                    {
                        if (filename != null && File.Exists(filename))
                            File.Delete(filename);
                    }
                }
            }

            public void Register(Process p)
            {
                p.Exited += new EventHandler(OnExited);
            }

            void OnExited(object sender, EventArgs e)
            {
                Dispose();

                if (_monitorDir)
                {
                    // TODO: Schedule status for all changed files
                }
            }

            public int DirectoryChanged(string pszDirectory)
            {
                IFileStatusCache fsc = GetService<IFileStatusCache>();

                if (fsc != null)
                {
                    fsc.MarkDirtyRecursive(GitTools.GetNormalizedFullPath(pszDirectory));
                }

                return VSConstants.S_OK;
            }

            public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
            {
                if (rgpszFile != null)
                {
                    foreach (string file in rgpszFile)
                    {
                        if (string.Equals(file, _toMonitor, StringComparison.OrdinalIgnoreCase))
                        {
                            IFileStatusMonitor m = GetService<IFileStatusMonitor>();

                            if (m != null)
                                m.ExternallyChanged(_toMonitor);

                            break;
                        }
                    }
                }

                return VSConstants.S_OK;
            }
        }


        #region Argument Substitution support

        enum DiffToolMode
        {
            None,
            Diff,
            Merge,
            Patch
        }

        private bool Substitute(string reference, VisualGitDiffToolArgs args, DiffToolMode toolMode, out string program, out string arguments)
        {
            if (string.IsNullOrEmpty(reference))
                throw new ArgumentNullException("reference");
            else if (args == null)
                throw new ArgumentNullException("args");

            // Ok: We received a string with a program and arguments and windows 
            // wants a program and arguments separated. Let's find the program before substituting

            reference = reference.TrimStart();

            program = null;
            arguments = null;

            string app;
            if (!string.IsNullOrEmpty(app = VisualGitDiffTool.GetToolNameFromTemplate(reference)))
            {
                // We have a predefined template. Just use it
                VisualGitDiffTool tool = GetAppItem(app, toolMode);

                if (tool == null)
                    return false;
                else if (!tool.IsAvailable)
                    return false;

                program = SubstituteArguments(tool.Program, args, toolMode);
                arguments = SubstituteArguments(tool.Arguments, args, toolMode);

                return !String.IsNullOrEmpty(program) && File.Exists(program);
            }
            else if (!TrySplitPath(reference, out program, out arguments))
                return false;

            program = SubstituteArguments(program, args, toolMode);
            arguments = SubstituteArguments(arguments, args, toolMode);

            return true;
        }

        static readonly VisualGitDiffArgs EmptyDiffArgs = new VisualGitDiffArgs();
        private bool TrySplitPath(string cmdline, out string program, out string arguments)
        {
            if(cmdline == null)
                throw new ArgumentNullException("cmdline");

            program = arguments = null;

            cmdline = cmdline.TrimStart();

            if (cmdline.StartsWith("\""))
            {
                // Ok: The easy way:
                int nEnd = cmdline.IndexOf('\"', 1);

                if (nEnd < 0)
                    return false; // Invalid string!

                program = cmdline.Substring(1, nEnd - 1);
                arguments = cmdline.Substring(nEnd + 1).TrimStart();
                return true;
            }

            // We use the algorithm as documented by CreateProcess() in MSDN
            // http://msdn2.microsoft.com/en-us/library/ms682425(VS.85).aspx
            char[] spacers = new char[] { ' ', '\t' };
            int nFrom = 0;
            int nTok = -1;

            string file;
            
            while ((nFrom < cmdline.Length) &&
                (0 <= (nTok = cmdline.IndexOfAny(spacers, nFrom))))
            {
                program = cmdline.Substring(0, nTok);

                file = SubstituteArguments(program, EmptyDiffArgs, DiffToolMode.None);

                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    arguments = cmdline.Substring(nTok + 1).TrimStart();
                    return true;
                }
                else
                    nFrom = nTok + 1;
            }

            if (nTok < 0 && nFrom <= cmdline.Length)
            {
                file = SubstituteArguments(cmdline, EmptyDiffArgs, DiffToolMode.None);

                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    program = file;
                    arguments = "";
                    return true;
                }
            }


            return false;
        }

        Regex _re;

        private string SubstituteArguments(string arguments, VisualGitDiffToolArgs diffArgs, DiffToolMode toolMode)
        {
            if (diffArgs == null)
                throw new ArgumentNullException("diffArgs");

            if (_re == null)
            {
                const string ifBody = "\\?(?<tick>['\"])(?<ifbody>([^'\"]|('')|(\"\"))*)\\k<tick>";
                const string elseBody = "(:(?<tick2>['\"])(?<elsebody>([^'\"]|('')|(\"\"))*)\\k<tick2>)?";

                _re = new Regex(@"(\%(?<pc>[a-zA-Z0-9_]+)(\%|\b))|(\$\((?<vs>[a-zA-Z0-9_-]*)(\((?<arg>[a-zA-Z0-9_-]*)\))?\))" +
                "|(\\$\\((?<if>[a-zA-Z0-9_-]+)" + ifBody + elseBody + "\\))");
            }

            return _re.Replace(arguments, new Replacer(this, diffArgs, toolMode).Replace).TrimEnd();
        }

        sealed class Replacer
        {
            readonly VisualGitDiff _context;
            readonly VisualGitDiffToolArgs _toolArgs;
            readonly VisualGitDiffArgs _diffArgs;
            readonly VisualGitMergeArgs _mergeArgs;
            readonly VisualGitPatchArgs _patchArgs;
            readonly DiffToolMode _toolMode;

            public Replacer(VisualGitDiff context, VisualGitDiffToolArgs args, DiffToolMode toolMode)
            {
                if (context == null)
                    throw new ArgumentNullException("context");
                else if (args == null)
                    throw new ArgumentNullException("args");

                _context = context;
                _toolArgs = args;
                _diffArgs = args as VisualGitDiffArgs;
                _mergeArgs = args as VisualGitMergeArgs;
                _patchArgs = args as VisualGitPatchArgs;
                _toolMode = toolMode;
            }

            VisualGitDiffArgs DiffArgs
            {
                get { return _diffArgs; }
            }

            VisualGitMergeArgs MergeArgs
            {
                get { return _mergeArgs; }
            }

            VisualGitPatchArgs PatchArgs
            {
                get { return _patchArgs; }
            }

            public string Replace(Match match)
            {
                string key;
                string value;
                bool vsStyle = true;

                if (match.Groups["pc"].Length > 1)
                {
                    vsStyle = false;
                    key = match.Groups["pc"].Value;
                }
                else if (match.Groups["vs"].Length > 1)
                    key = match.Groups["vs"].Value;
                else if (match.Groups["if"].Length > 1)
                {
                    string kk = match.Groups["if"].Value;

                    bool isTrue = false;
                    if (TryGetValue(kk, true, "", out value))
                        isTrue = !string.IsNullOrEmpty(value);

                    value = match.Groups[isTrue ? "ifbody" : "elsebody"].Value ?? "";
                    
                    value = value.Replace("''", "'").Replace("\"\"", "\"");

                    return _context.SubstituteArguments(value, _diffArgs, _toolMode);
                }
                else
                    return match.Value; // Don't replace if not matched

                string arg = match.Groups["arg"].Value ?? "";
                TryGetValue(key, vsStyle, arg, out value);

                return value ?? "";
            }

            bool TryGetValue(string key, bool vsStyle, string arg, out string value)
            {
                if (key == null)
                    throw new ArgumentNullException("key");

                key = key.ToUpperInvariant();
                value = null;

                string v;
                switch (key)
                {
                    case "BASE":
                        if (DiffArgs != null)
                            value = DiffArgs.BaseFile;
                        else
                            return false;
                        break;
                    case "BNAME":
                    case "BASENAME":
                        if (DiffArgs != null)
                            value = DiffArgs.BaseTitle ?? Path.GetFileName(DiffArgs.BaseFile);
                        else
                            return false;
                        break;
                    case "MINE":
                        if (DiffArgs != null)
                            value = DiffArgs.MineFile;
                        else
                            return false;
                        break;
                    case "YNAME":
                    case "MINENAME":
                        if (DiffArgs != null)
                            value = DiffArgs.MineTitle ?? Path.GetFileName(DiffArgs.MineFile);
                        else
                            return false;
                        break;

                    case "THEIRS":
                        if (MergeArgs != null)
                            value = MergeArgs.TheirsFile;
                        else
                            return false;
                        break;
                    case "TNAME":
                    case "THEIRNAME":
                    case "THEIRSNAME":
                        if (MergeArgs != null)
                            value = MergeArgs.TheirsTitle ?? Path.GetFileName(MergeArgs.TheirsFile);
                        else
                            return false;
                        break;
                    case "MERGED":
                        if (MergeArgs != null)
                            value = MergeArgs.MergedFile;
                        else
                            return false;
                        break;
                    case "MERGEDNAME":
                    case "MNAME":
                        if (MergeArgs != null)
                            value = MergeArgs.MergedTitle ?? Path.GetFileName(MergeArgs.MergedFile);
                        else
                            return false;
                        break;

                    case "PATCHFILE":
                        if (PatchArgs != null)
                            value = PatchArgs.PatchFile;
                        else
                            return false;
                        break;
                    case "APPLYTODIR":
                        if (PatchArgs != null)
                            value = PatchArgs.ApplyTo;
                        else
                            return false;
                        break;
                    case "APPPATH":
                        v = _context.GetAppPath(arg, _toolMode);
                        value = _context.SubstituteArguments(v ?? "", DiffArgs, _toolMode);
                        break;
                    case "APPTEMPLATE":
                        v = _context.GetAppTemplate(arg, _toolMode);
                        value = _context.SubstituteArguments(v ?? "", DiffArgs, _toolMode);
                        break;
                    case "PROGRAMFILES":
                        // Return the environment variable if using environment variable style
                        value = (vsStyle ? null : Environment.GetEnvironmentVariable(key)) ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        break;
                    case "COMMONPROGRAMFILES":
                        // Return the environment variable if using environment variable style
                        value = (vsStyle ? null : Environment.GetEnvironmentVariable(key)) ?? Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                        break;
                    case "HOSTPROGRAMFILES":
                        // Use the WOW64 program files directory if available, otherwise just program files
                        value = Environment.GetEnvironmentVariable("PROGRAMW6432") ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                        break;
                    case "READONLY":
                        if (DiffArgs != null && DiffArgs.ReadOnly)
                            value = "1";
                        else
                            value = "";
                        break;
                    case "VSHOME":
                        IVsSolution sol = _context.GetService<IVsSolution>(typeof(SVsSolution));
                        if (sol == null)
                            return false;
                        object val;
                        if (ErrorHandler.Succeeded(sol.GetProperty((int)__VSSPROPID.VSSPROPID_InstallDirectory, out val)))
                            value = val as string;
                        return true;
                    default:
                        // Just replace with "" if unknown
                        v = Environment.GetEnvironmentVariable(key);
                        if (!string.IsNullOrEmpty(v))
                            value = v;
                        return (value != null);
                }

                return true;
            }
        }

        #endregion

        public string GetTitle(GitItem target, GitRevision revision)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (revision == null)
                throw new ArgumentNullException("revision");

            return GetTitle(target.Name, revision);
        }

        public string GetTitle(GitTarget target, GitRevision revision)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (revision == null)
                throw new ArgumentNullException("revision");

            return GetTitle(target.FileName, revision);
        }

        static string GetTitle(string fileName, GitRevision revision)
        {
            string strRev = revision.RevisionType == GitRevisionType.Time ?
                revision.Time.ToLocalTime().ToString("g") : revision.ToString();

            return fileName + " - " + strRev;
        }

        static string PathSafeRevision(GitRevision revision)
        {
            if (revision.RevisionType == GitRevisionType.Time)
                return revision.Time.ToLocalTime().ToString("yyyyMMdd_hhmmss");
            return revision.ToString();
        }

        string GetName(string filename, GitRevision rev)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            else if (rev == null)
                throw new ArgumentNullException("rev");

            return (Path.GetFileNameWithoutExtension(filename) + "." + PathSafeRevision(rev) + Path.GetExtension(filename)).Trim('.');
        }

        string GetTempPath(string filename, GitRevision rev)
        {
            string name = GetName(filename, rev);
            string file;
            if (_lastDir == null || !Directory.Exists(_lastDir) || File.Exists(file = Path.Combine(_lastDir, name)))
            {
                _lastDir = GetService<IVisualGitTempDirManager>().GetTempDir();

                file = Path.Combine(_lastDir, name);
            }

            return file;
        }

        string _lastDir;
        public string GetTempFile(GitItem target, GitRevision revision, bool withProgress)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            else if (revision == null)
                throw new ArgumentNullException("revision");

            string file = GetTempPath(target.Name, revision);

            if (target.NodeKind != GitNodeKind.File)
                throw new InvalidOperationException("Can't create a tempfile from a directory");

            ProgressRunnerResult r = GetService<IProgressRunner>().RunModal("Getting file",
                delegate(object sender, ProgressWorkerArgs aa)
                {
                    GitWriteArgs wa = new GitWriteArgs();
                    wa.Revision = revision;

                    using (Stream s = File.Create(file))
                        aa.Client.Write(new GitTarget(target.FullPath), s, wa);
                });

            if (!r.Succeeded)
                return null; // User canceled

            if (File.Exists(file))
                File.SetAttributes(file, FileAttributes.ReadOnly); // A readonly file does not allow editting from many diff tools

            return file;
        }

        public string GetTempFile(GitTarget target, GitRevision revision, bool withProgress)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            else if (revision == null)
                throw new ArgumentNullException("revision");

            string file = GetTempPath(target.FileName, revision);
            bool unrelated = false;

            ProgressRunnerResult r = GetService<IProgressRunner>().RunModal("Getting file",
                delegate(object sender, ProgressWorkerArgs aa)
                {
                    GitWriteArgs wa = new GitWriteArgs();
                    wa.Revision = revision;

                    using (Stream s = File.Create(file))
                        aa.Client.Write(target, s, wa);
                });

            if (!r.Succeeded || unrelated)
                return null; // User canceled

            if (File.Exists(file))
                File.SetAttributes(file, FileAttributes.ReadOnly); // A readonly file does not allow editting from many diff tools

            return file;
        }

        public string[] GetTempFiles(GitTarget target, GitRevision from, GitRevision to, bool withProgress)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            else if (from == null)
                throw new ArgumentNullException("from");
            else if (to == null)
                throw new ArgumentNullException("to");

            string f1 = GetTempFile(target, from, withProgress);
            if (f1 == null)
                return null; // Canceled
            string f2 = GetTempFile(target, to, withProgress);

            if (string.IsNullOrEmpty(f1) || string.IsNullOrEmpty(f2))
                return null;

            string[] files = new string[] { f1, f2 };

            foreach (string f in files)
            {
                if (File.Exists(f))
                    File.SetAttributes(f, FileAttributes.ReadOnly);
            }

            return files;
        }
    }
}
