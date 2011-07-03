// VisualGit.UI\GitLog\RevisionGrid\DvcsGraph_Node.cs
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

namespace VisualGit.UI.GitLog.RevisionGrid
{
    partial class DvcsGraph
    {
        #region Nested type: Node

        private class Node
        {
            public List<Junction> Ancestors { get; private set; }
            public List<Junction> Descendants { get; private set; }
            public string Id { get; private set; }
            public object Data { get; set; }
            public DataType DataType { get; set; }
            public int InLane { get; set; }
            public int Index { get; set; }

            public Node(string aId)
            {
                Ancestors = new List<Junction>();
                Descendants = new List<Junction>();
                InLane = int.MaxValue;
                Index = int.MaxValue;

                Id = aId;
            }

            public bool IsActive
            {
                get { return (DataType & DataType.Active) == DataType.Active; }
            }

            public bool IsFiltered
            {
                get { return (DataType & DataType.Filtered) == DataType.Filtered; }
                set
                {
                    if (value)
                    {
                        DataType |= DataType.Filtered;
                    }
                    else
                    {
                        DataType &= ~DataType.Filtered;
                    }
                }
            }

            public bool IsSpecial
            {
                get { return (DataType & DataType.Special) == DataType.Special; }
            }

            public override string ToString()
            {
                if (Data == null)
                {
                    string name = Id.ToString();
                    if (name.Length > 8)
                    {
                        name = name.Substring(0, 4) + ".." + name.Substring(name.Length - 4, 4);
                    }
                    return string.Format("{0} ({1})", name, Index);
                }
                else
                {
                    return Data.ToString();
                }
            }
        }

        #endregion
    }
}
