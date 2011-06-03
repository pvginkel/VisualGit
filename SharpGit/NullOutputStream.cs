using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharpen;

namespace SharpGit
{
    internal class NullOutputStream : OutputStream
    {
        public static readonly NullOutputStream Instance = new NullOutputStream();

        private NullOutputStream()
        {
        }

        public override void Write(int b)
        {
            throw new InvalidOperationException();
        }
    }
}
