using System;
using System.Collections.Generic;
using System.Text;

namespace VisualGit.Scc.SettingMap
{
    class SccCategorySettings
    {
        readonly SortedList<string, string> _props = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
        readonly VisualGitSccSettingStorage _store;
        readonly string _id;
        string _name;

        public SccCategorySettings(VisualGitSccSettingStorage store, string id)
        {
            if (store == null)
                throw new ArgumentNullException("store");
            else if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");

            _store = store;
            _id = id;

            store.AddCategory(this);
        }

        public string CategoryId
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            internal set { _name = value; }
        }

        public IDictionary<string, string> Properties
        {
            get { return _props; }
        }

        public bool ShouldPersist
        {
            get { return true; }
        }
    }
}
