using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using NGit.Util;
using NGit.Diff;

namespace SharpGit
{
    internal static class RawTextAccessor
    {
        private static FieldInfo _linesField;
        private static FieldInfo _contentField;

        public static IntList GetLines(RawText rawText)
        {
            if (_linesField == null)
                _linesField = typeof(RawText).GetField("lines", BindingFlags.NonPublic | BindingFlags.Instance);

            return (IntList)_linesField.GetValue(rawText);
        }

        public static byte[] GetContent(RawText rawText)
        {
            if (_contentField == null)
                _contentField = typeof(RawText).GetField("content", BindingFlags.NonPublic | BindingFlags.Instance);

            return (byte[])_contentField.GetValue(rawText);
        }
    }
}
