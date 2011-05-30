using System;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using VisualGit.Scc;
using SharpSvn;
using System.IO;
using System.Diagnostics;

namespace VisualGit.VS.SolutionExplorer
{
    [GlobalService(typeof(IStatusImageMapper))]
    sealed class StatusImageMapper : VisualGitService, IStatusImageMapper
    {
        public StatusImageMapper(IVisualGitServiceProvider context)
            : base(context)
        {
        }

        ImageList _statusImageList;
        public ImageList StatusImageList
        {
            get { return _statusImageList ?? (_statusImageList = CreateStatusImageList()); }
        }

        public ImageList CreateStatusImageList()
        {
            using (Stream images = typeof(StatusImageMapper).Assembly.GetManifestResourceStream(typeof(StatusImageMapper).Namespace + ".StatusGlyphs.bmp"))
            {
                if (images == null)
                    return null;

                Bitmap bitmap = (Bitmap)Image.FromStream(images, true);

                ImageList imageList = new ImageList();
                imageList.ImageSize = new Size(8, bitmap.Height);
                bitmap.MakeTransparent(bitmap.GetPixel(0, 0));

                imageList.Images.AddStrip(bitmap);

                return imageList;
            }
        }

        public VisualGitGlyph GetStatusImageForGitItem(GitItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (item.IsConflicted || item.IsObstructed || item.IsTreeConflicted)
                return VisualGitGlyph.InConflict;
            else if (!item.IsVersioned)
            {
                if (!item.Exists)
                    return VisualGitGlyph.FileMissing;
                else if (item.IsIgnored)
                    return VisualGitGlyph.Ignored;
                else if (item.IsVersionable)
                    return item.InSolution ? VisualGitGlyph.ShouldBeAdded : VisualGitGlyph.Blank;
                else
                    return VisualGitGlyph.None;
            }
            
			switch (item.Status.State)
            {
                case SvnStatus.Normal:
                    if (item.IsDocumentDirty)
                        return VisualGitGlyph.FileDirty;
                    else
                        return VisualGitGlyph.Normal;
                case SvnStatus.Modified:
                    return VisualGitGlyph.Modified;
                case SvnStatus.Replaced:
                    return VisualGitGlyph.CopiedOrMoved;
                case SvnStatus.Added:
                    return item.Status.IsCopied ? VisualGitGlyph.CopiedOrMoved : VisualGitGlyph.Added;

                case SvnStatus.Missing:
                    if (item.IsCasingConflicted)
                        return VisualGitGlyph.InConflict;
                    else
                        goto case SvnStatus.Deleted;
                case SvnStatus.Deleted:
                    return VisualGitGlyph.Deleted;

                case SvnStatus.Conflicted: // Should have been handled above
                case SvnStatus.Obstructed:
                    return VisualGitGlyph.InConflict;

                case SvnStatus.Ignored: // Should have been handled above
                    return VisualGitGlyph.Ignored;

                case SvnStatus.External:
                case SvnStatus.Incomplete:
                    return VisualGitGlyph.InConflict;

                case SvnStatus.Zero:
                default:
                    return VisualGitGlyph.None;
            }
        }
    }
}
