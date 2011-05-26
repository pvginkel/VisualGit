using System;
using System.Collections.Generic;
using System.Text;
using VisualGit.VS;
using VisualGit.Scc;
using System.Runtime.InteropServices;

namespace VisualGit.UI.WorkingCopyExplorer.Nodes
{
    class WCMyComputerNode : WCTreeNode
    {
        readonly int _imageIndex;
        public WCMyComputerNode(IVisualGitServiceProvider context)
            : base(context, null)
        {
            _imageIndex = context.GetService<IFileIconMapper>().GetSpecialFolderIcon(WindowsSpecialFolder.MyComputer);
        }

        public override string Title
        {
            get { return "My Computer"; }
        }

        public override void GetResources(System.Collections.ObjectModel.Collection<GitItem> list, bool getChildItems, Predicate<GitItem> filter)
        {
        }

        public override IEnumerable<WCTreeNode> GetChildren()
        {
            IFileStatusCache cache = Context.GetService<IFileStatusCache>();

            foreach (string s in Environment.GetLogicalDrives())
            {
                switch (NativeMethods.GetDriveType(s))
                {
                    case NativeMethods.DriveType.Removable:
                        if (s != "A:\\") // We should really filter floppy/disconnected drives 
                            goto case NativeMethods.DriveType.Fixed;
                        break;
                    case NativeMethods.DriveType.Fixed:
                    case NativeMethods.DriveType.Remote:
                    case NativeMethods.DriveType.RAMDisk:
                        yield return new WCDirectoryNode(Context, this, cache[s]);
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void RefreshCore(bool rescan)
        {
        }

        public override int ImageIndex
        {
            get { return _imageIndex; }
        }

        internal override bool ContainsDescendant(string path)
        {
            return true;
        }

        static class NativeMethods
        {
            public enum DriveType : uint
            {
                /// <summary>The drive type cannot be determined.</summary>
                Unknown = 0,    //DRIVE_UNKNOWN
                /// <summary>The root path is invalid, for example, no volume is mounted at the path.</summary>
                Error = 1,        //DRIVE_NO_ROOT_DIR
                /// <summary>The drive is a type that has removable media, for example, a floppy drive or removable hard disk.</summary>
                Removable = 2,    //DRIVE_REMOVABLE
                /// <summary>The drive is a type that cannot be removed, for example, a fixed hard drive.</summary>
                Fixed = 3,        //DRIVE_FIXED
                /// <summary>The drive is a remote (network) drive.</summary>
                Remote = 4,        //DRIVE_REMOTE
                /// <summary>The drive is a CD-ROM drive.</summary>
                CDROM = 5,        //DRIVE_CDROM
                /// <summary>The drive is a RAM disk.</summary>
                RAMDisk = 6        //DRIVE_RAMDISK
            }

            [DllImport("kernel32.dll")]
            public static extern DriveType GetDriveType([MarshalAs(UnmanagedType.LPStr)] string lpRootPathName);
        }
    }
}
