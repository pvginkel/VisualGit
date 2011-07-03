// VisualGit.Diff\DiffUtils\Classes\DiagonalVector.cs
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

#region Copyright And Revision History

/*---------------------------------------------------------------------------

	DiagonalVector.cs
	Copyright (c) 2002 Bill Menees.  All rights reserved.
	Bill@Menees.com

	Who		When		What
	-------	----------	-----------------------------------------------------
	BMenees	10.20.2002	Created.

-----------------------------------------------------------------------------*/

#endregion

using System;
using System.Diagnostics;

namespace VisualGit.Diff.DiffUtils
{
    /// <summary>
    /// Implements a vector from -MAX to MAX
    /// </summary>
    internal sealed class DiagonalVector
    {
        int m_iMax;
        int[] m_arData;

        public DiagonalVector(int N, int M)
        {
            int iDelta = N - M;

            //We have to add Delta to support reverse vectors, which are
            //centered around the Delta diagonal instead of the 0 diagonal.
            m_iMax = N + M + Math.Abs(iDelta);

            //Create an array of size 2*MAX+1 to hold -MAX..+MAX.
            m_arData = new int[2 * m_iMax + 1];
        }

        public int this[int iUserIndex]
        {
            get
            {
                return m_arData[GetActualIndex(iUserIndex)];
            }
            set
            {
                m_arData[GetActualIndex(iUserIndex)] = value;
            }
        }

        private int GetActualIndex(int iUserIndex)
        {
            int iIndex = iUserIndex + m_iMax;
            Debug.Assert(iIndex >= 0 && iIndex < m_arData.Length);
            return iIndex;
        }
    }
}
