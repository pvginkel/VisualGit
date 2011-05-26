using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Resources;
using VisualGit;

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
[assembly: System.Runtime.InteropServices.ComVisible(false)]

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

[assembly: AssemblyVersionAttribute("2.2.*")]
