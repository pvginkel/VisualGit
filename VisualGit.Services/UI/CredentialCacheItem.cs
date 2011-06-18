using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualGit.UI
{
    public class CredentialCacheItem
    {
        public CredentialCacheItem(string uri, string type, string promptText, string response)
        {
            Uri = uri;
            Type = type;
            PromptText = promptText;
            Response = response;
        }

        public string Uri { get; set; }
        public string Type { get; set; }
        public string PromptText { get; set; }
        public string Response { get; set; }
    }
}
