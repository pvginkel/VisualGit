// VisualGit.UI\GitLog\RevisionGrid\LineRun.cs
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
