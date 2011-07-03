// SharpGit\RawTextAccessor.cs
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
