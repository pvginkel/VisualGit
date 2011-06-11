using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace VisualGit.UI.GitLog.RevisionGrid
{
    internal class LineRun
    {
        private const TextFormatFlags DrawFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine;

        public LineRun()
        {
            Items = new List<LineRunItem>();
        }

        public IList<LineRunItem> Items { get; private set; }

        public void Draw(Graphics graphics, Rectangle bounds)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            int offset = 0;

            foreach (var item in Items)
            {
                string text = Regex.Replace(item.Text.TrimEnd(), "\\r?\\n", " \u00B6 ");

                var renderSize = TextRenderer.MeasureText(graphics, text, item.Font, new Size(bounds.Width - offset, bounds.Height), DrawFlags);

                TextRenderer.DrawText(graphics, text, item.Font, new Rectangle(bounds.Left + offset, bounds.Top, renderSize.Width, renderSize.Height), item.Color, DrawFlags);

                offset += renderSize.Width;

                if (offset >= bounds.Width)
                    break;
            }
        }
    }
}
