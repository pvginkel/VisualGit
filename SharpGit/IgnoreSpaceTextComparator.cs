using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Diff;
using NGit.Util;
using System.Reflection;

namespace SharpGit
{
    internal class IgnoreSpaceTextComparator : RawTextComparator
    {
        public static readonly IgnoreSpaceTextComparator Instance = new IgnoreSpaceTextComparator();

        private IgnoreSpaceTextComparator()
        {
        }

        public override bool Equals(RawText a, int ai, RawText b, int bi)
        {
            ai++;
            bi++;
            IntList a_lines = RawTextAccessor.GetLines(a);
            IntList b_lines = RawTextAccessor.GetLines(b);
            byte[] a_content = RawTextAccessor.GetContent(a);
            byte[] b_content = RawTextAccessor.GetContent(b);
            int @as = a_lines.Get(ai);
            int bs = b_lines.Get(bi);
            int ae = a_lines.Get(ai + 1);
            int be = b_lines.Get(bi + 1);
            ae = TrimTrailingSpace(a_content, @as, ae);
            be = TrimTrailingSpace(b_content, bs, be);
            while (@as < ae && bs < be)
            {
                byte ac = a_content[@as];
                byte bc = b_content[bs];
                while (@as < ae - 1 && ac == ' ')
                {
                    @as++;
                    ac = a_content[@as];
                }
                while (bs < be - 1 && bc == ' ')
                {
                    bs++;
                    bc = b_content[bs];
                }
                if (ac != bc)
                {
                    return false;
                }
                @as++;
                bs++;
            }
            return @as == ae && bs == be;
        }

        protected override int HashRegion(byte[] raw, int ptr, int end)
        {
            int hash = 5381;
            for (; ptr < end; ptr++)
            {
                byte c = raw[ptr];
                if (c != ' ')
                {
                    hash = ((hash << 5) + hash) + (c & unchecked((int)(0xff)));
                }
            }
            return hash;
        }

        private int TrimTrailingSpace(byte[] raw, int start, int end)
        {
            int ptr = end - 1;
            while (start <= ptr && raw[ptr] == ' ')
            {
                ptr--;
            }
            return ptr + 1;
        }
    }
}
