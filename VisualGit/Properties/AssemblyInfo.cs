// VisualGit\Properties\AssemblyInfo.cs
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Resources;
using VisualGit;
using System.Runtime.InteropServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: CLSCompliant(true)] 
[assembly: AssemblyTitle("VisualGit - Git support for Visual Studio")]
[assembly: AssemblyDescription("VisualGit - Git support for Visual Studio")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(VisualGitId.AssemblyCompany)]
[assembly: AssemblyProduct(VisualGitId.AssemblyProduct)]
[assembly: AssemblyCopyright(VisualGitId.AssemblyCopyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en-US")]

// We only want the Connect class to be visible
[assembly: ComVisible(false)]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Revision
//      Build Number
//
// You can specify all the value or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("0.1.*")]
