using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit
{
    /// <summary>
    /// Container of guids used by the package and command framework
    /// </summary>
    public static class VisualGitId
    {
        //************ The Package Load Key Registration ***********************************
        /// <summary>
        /// The guid the VisualGitSvn package is registered with inside Visual Studio
        /// </summary>
        public const string PackageId = "23558e20-d3d3-4de3-8101-54851f33f629";
        public const string PackageDescription = "VisualGit - Git Support for Visual Studio";

        /// <summary>The package version as used in the PLK</summary>
        public const string PlkVersion = "2.0";
        /// <summary>The product name as used in the PLK</summary>
        public const string PlkProduct = "VisualGit";
        /// <summary>The company name as used in the PLK</summary>
        public const string PlkCompany = "VisualGit";
        //**********************************************************************************

        public const string AssemblyCopyright = "Copyright � VisualGit 2011";
        public const string AssemblyProduct = "VisualGit - Git Support for Visual Studio";
        public const string AssemblyCompany = "VisualGit";
        //**********************************************************************************

        /// <summary>The Git SCC Provider name as used in the solution file</summary>
        public const string GitSccName = "GitScc";
        public const string SccStructureName = "SccStructure";

        // Items for the VS 2010 Extension registration
        public const string ExtensionTitle = VisualGitId.AssemblyProduct;
        public const string ExtensionAuthor = VisualGitId.AssemblyCompany;
        public const string ExtensionDescription = "Open Source Git SCC Provider for Visual Studio 2005, 2008 and 2010.";
        public const string ExtensionMoreInfoUrl = "http://www.ankhsvn.net/";
        public const string ExtensionGettingStartedUrl = "http://www.ankhsvn.net/";

        /// <summary>
        /// The guid used for registering the commands registered by the VisualGitSvn package
        /// </summary>
        /// <remarks>Must be changed when the PackageId changes</remarks>
        public const string CommandSet = "ef48818d-629d-41a9-a9a6-50bba9133444";

        /// <summary>The SCC Provider guid (used as SCC active marker by VS)</summary>
        public const string SccProviderId = "75fae898-e066-4f7b-9f9e-d8492d971c04";
        /// <summary>The GUID of the SCC Service</summary>
        public const string SccServiceId = "7fe71a89-61a4-4d4c-a4e0-526d57a4650e";
        /// <summary>Language neutral SCC Provider title</summary>
        public const string SccProviderTitle = "VisualGit - Git Support for Visual Studio";


        //************ Special contexts managed by our Trigger package *********************        
        /// <summary>
        /// Context set by Trigger package when another Scc package is active
        /// </summary>
        public const string CtxNoOtherSccActive = "ee46100d-3901-4ae5-8081-86f75c1a0422";
        public const string CtxNoOtherSccManaging = "af002ebc-9e2f-4bcc-baf5-2e1f812019b7";
        
        // When this context is set the full package is loaded and the trigger package is deactivated
        public const string CtxFullSccLoaded = "f86a50fc-b065-403c-9585-a48bce8e793c";
        //**********************************************************************************

        // Increase this value when you want to have VisualGitCommand.MigrateSettings called on first use
        public const int MigrateVersion = 5;

        public const string VisualGitServicesAvailable = "1A667225-B18F-4EAA-85E5-0C86E464CFC0";
        public const string VisualGitRuntimeStarted = "6ACA795D-C994-4DD0-AC9C-2C936D45A1BB";

        public const string LogMessageLanguageServiceId = "EC98C414-9B51-4F51-9CFA-F4C71ECDC2A8";
        public const string LogMessageServiceName = "VisualGit Log Messages";

        public const string UnifiedDiffLanguageServiceId = "C00D9098-DCA3-4819-89BD-2FF52C8A8037";
        public const string UnifiedDiffServiceName = "VisualGit Unified Diff";

        public const string TriggerExtenderGuid = "ED797D43-13D0-4883-8F56-BDC2F82443BB";
        public const string TriggerExtenderName = "VisualGitExtenderProvider";
        public const string SccExtenderGuid = "046603DB-74E7-478E-9933-F6E32433A8AE";

        /// <summary>
        /// The guid used for our on-and-only bitmap resource
        /// </summary>
        public const string BmpId = "A7C70823-6626-4E56-B855-339D5AE83F21";


        public const string VisualGitOutputPaneId = "1BA9280B-3B7E-41F2-91DC-0EBED18DD619";


        public const string RepositoryExplorerToolWindowId = "AFDAA869-4921-4859-8FE3-BCE8868227DA";
        public const string PendingChangesToolWindowId = "7D28E788-89D6-464B-A6A6-FDCB06B7D664";
        public const string WorkingCopyExplorerToolWindowId = "63C4CB4B-2A73-47F3-ADB6-99D52B9C00FD";
        public const string BlameToolWindowId = "843AB0C2-A28C-46E4-B2E5-EF55B570CC1F";
        public const string LogToolWindowId = "D81233FE-925A-4285-9038-2BA21F0B3610";
        public const string DiffToolWindowId = "F1095693-1237-4682-9605-79AF3C41955C";
        public const string GitInfoToolWindowId = "EC03B4B6-7444-4143-AC47-CC51E98245E2";

        public const string PendingChangeViewContext = "CACA63FD-EC08-4032-990B-AE5884C89CA9";
        public const string DiffMergeViewContext = "92734E38-7AD7-4189-A207-7CC6DABD6B80";
        public const string SccExplorerViewContext = "75CE7E40-9400-4D40-82B3-803AACF37212";
        public const string LogViewContext = "27A4F1E4-3EE9-44B3-ADA7-63A29F0C071A";

        public const string AnnotateContext = "ED1FE1D2-8378-41D5-A943-23B9CF859CB5";
        public const string DiffEditorId = "3D57C254-990C-4062-86F1-D2FE6E75869E";
        public const string DynamicEditorId = "87CFC2E1-DD6E-4683-8240-E0E7C6822A69";
        public const string DiffEditorViewId = "579E41BB-0FBD-4469-A7D9-8C28C9BCA6D2";

        public const string EnvironmentSettingsPageGuid = "3DFCF4B7-2037-446A-9423-AB6AD794E906";
        public const string UserToolsSettingsPageGuid = "F6EF370E-EB7C-41E2-A725-F7976D11EFC2";
        public const string IssueTrackerSettingsPageGuid = "E208E058-A2D5-423A-9746-9B49FCCBEDF5";

        /// <summary>
        /// The command set as a guid
        /// </summary>
        public static readonly Guid CommandSetGuid = new Guid(CommandSet);

        /// <summary>
        /// The package is as a guid
        /// </summary>
        public static readonly Guid PackageGuid = new Guid(PackageId);

        /// <summary>
        /// The guid for the generated Bmp
        /// </summary>
        public static readonly Guid BmpGuid = new Guid(BmpId);

        public static readonly Guid SccProviderGuid = new Guid(SccProviderId);
        public static readonly Guid SccServiceGuid = new Guid(SccServiceId);

        public static readonly Guid PendingChangeContextGuid = new Guid(PendingChangeViewContext);
        public static readonly Guid DiffMergeContextGuid = new Guid(DiffMergeViewContext);
        public static readonly Guid SccExplorerContextGuid = new Guid(SccExplorerViewContext);
        public static readonly Guid LogContextGuid = new Guid(LogViewContext);
        public static readonly Guid AnnotateContextGuid = new Guid(AnnotateContext);
    }
}
