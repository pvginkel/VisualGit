// SharpGit\GitCommandType.cs
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

namespace SharpGit
{
    public enum GitCommandType
    {
        //Unknown = 0,
        Add = 1,
        Blame = 3,
        //CheckOut = 4,
        //CleanUp = 5,
        Commit = 6,
        //Copy = 7,
        //CreateDirectory = 8,
        Delete = 9,
        Diff = 10,
        //DiffMerge = 11,
        //DiffSummary = 12,
        Export = 13,
        //GetAppliedMergeInfo = 14,
        //GetProperty = 15,
        //GetRevisionProperty = 16,
        //GetSuggestedMergeSources = 17,
        //Import = 18,
        Info = 19,
        //List = 20,
        //Lock = 22,
        Log = 23,
        Merge = 24,
        //MergesEligible = 25,
        //MergesMerged = 26,
        Move = 27,
        //PropertyList = 28,
        //ReintegrationMerge = 29,
        //Relocate = 30,
        Resolved = 32,
        Revert = 33,
        //RevisionPropertyList = 34,
        //SetProperty = 35,
        //SetRevisionProperty = 36,
        Status = 37,
        Switch = 38,
        //Unlock = 39,
        //Update = 40,
        Write = 41,
        //CropWorkingCopy = 42,
        Push = 43,
        Pull = 44,
        RemoteRefs = 45,
        Clone = 46,
        CreateRepository = 47,
        Branch = 48,
        Tag = 49,
        RevertItem = 50,
        Reset = 51,
        //GetWorkingCopyInfo = 4097,
        //GetWorkingCopyVersion = 4098,
        //GetWorkingCopyEntries = 4099,
        //FileVersions = 8193,
        //ReplayRevision = 8194,
        //WriteRelated = 8195,
    }
}
