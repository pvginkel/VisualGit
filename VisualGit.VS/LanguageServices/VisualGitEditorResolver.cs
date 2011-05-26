using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Package;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace VisualGit.VS.LanguageServices
{
    [GlobalService(typeof(IVisualGitEditorResolver))]
    class VisualGitEditorResolver : VisualGitService, IVisualGitEditorResolver
    {
        readonly EditorFactory _factory;
        public VisualGitEditorResolver(IVisualGitServiceProvider context)
            : base(context)
        {
            _factory = new EditorFactory();
            _factory.SetSite(this);
        }

        public bool TryGetLanguageService(string extension, out Guid languageService)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                string value = _factory.GetLanguageService(extension);

                if (!string.IsNullOrEmpty(value))
                {
                    languageService = new Guid(value);
                    return true;
                }
            }
            languageService = Guid.Empty;
            return false;
        }
    }
}
