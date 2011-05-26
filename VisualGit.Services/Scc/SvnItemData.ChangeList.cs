using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using VisualGit.UI;

namespace VisualGit.Scc
{
    partial class GitItemData
    {
        [TypeConverter(typeof(ChangeListTypeConverter)), ImmutableObject(true)]
        public sealed class GitChangeList
        {
            readonly string _list;

            public GitChangeList(string list)
            {
                if (list == null)
                    throw new ArgumentNullException("list");

                _list = list;
            }

            public string List
            {
                get { return _list; }
            }

            public override string ToString()
            {
                return _list;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as GitChangeList);
            }

            public bool Equals(GitChangeList obj)
            {
                if (obj == null)
                    return false;

                return List == obj.List;
            }

            public override int GetHashCode()
            {
                return StringComparer.Ordinal.GetHashCode(List);
            }

            public static implicit operator string(GitChangeList list)
            {
                if (list == null)
                    return null;
                return list.List;
            }

            public static implicit operator GitChangeList(string list)
            {
                if (list == null)
                    return null;
                return new GitChangeList(list);
            }
        }

        sealed class ChangeListTypeConverter : StringConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (typeof(string).IsAssignableFrom(sourceType) || sourceType == typeof(GitChangeList))
                    return true;

                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
            {
                GitChangeList chl = value as GitChangeList;
                if (chl != null)
                    return chl;

                string cl = value as string;

                if (cl != null)
                    return string.IsNullOrEmpty(cl) ? null : new GitChangeList(cl);

                return base.ConvertFrom(context, culture, value);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                if (destinationType == null)
                    throw new ArgumentNullException("destinationType");
                if (destinationType.IsAssignableFrom(typeof(string)) || destinationType == typeof(GitChangeList))
                    return true;

                return base.CanConvertTo(context, destinationType);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == null)
                    throw new ArgumentNullException("destinationType");

                GitChangeList chl = value as GitChangeList;
                string listName = chl != null ? chl.List : null;

                if (destinationType.IsAssignableFrom(typeof(string)))
                    return listName;
                else if (destinationType == typeof(GitChangeList))
                    return chl;

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                List<GitChangeList> names = new List<GitChangeList>();

                if (context != null)
                {
                    IVisualGitPackage package = context.GetService(typeof(IVisualGitPackage)) as IVisualGitPackage;

                    if (package != null)
                    {
                        IPendingChangesManager pcm = package.GetService<IPendingChangesManager>();

                        foreach (string cl in pcm.GetSuggestedChangeLists())
                        {
                            names.Add(cl);
                        }
                    }
                }

                names.Add("ignore-on-commit");

                StandardValuesCollection svc = new StandardValuesCollection(names);
                return svc;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
