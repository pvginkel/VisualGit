# VisualGit

## Introduction

VisualGit is an extension for Visual Studio 2010 providing a fully functional source control
provider without any dependencies other than this extension.

## Project status

This project is currently in Alpha state and under heavy development. Please report any
bugs or issues you encounter so they can be solved promptly.

## Known issues

Review the issue list at <http://github.com/pvginkel/VisualGit/issues> for a complete list
of issues. The most important issues currently are:

* Project load and refresh is very slow on large projects [http://github.com/pvginkel/VisualGit/issues/25](#25);
* Maintenance on branches and tags is missing [http://github.com/pvginkel/VisualGit/issues/8](#8);
* Maintenance on remotes is missing [http://github.com/pvginkel/VisualGit/issues/30](#30).

## Dependencies

VisualGit is heavily based on the excellent Subversion source control provider AnkhSVN
which can be downloaded from <http://ankhsvn.open.collab.net/>.

Git repository management is provided by NGit, a Sharpen port of JGit. NGit can be found at
<http://github.com/slluis/ngit>, Sharpen can be found at
<http://developer.db4o.com/Projects/html/projectspaces/db4o_product_design/sharpen.html> and
JGit can be found at <http://eclipse.org/jgit>.

## Reporting bugs

Integrated with VisualGit is [http://github.com/pvginkel/CrashReporter.NET](CrashReporter.NET).
If you encounter a bug, send a crash report through the CrashReporter.NET interface.
These issues are collected at http://visualgit-bugs.appspot.com/.

Other issues and requests can be reported through GitHub at 
<http://github.com/pvginkel/VisualGit/issues>.
