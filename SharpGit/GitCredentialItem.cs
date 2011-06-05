using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGit.Transport;

namespace SharpGit
{
    public sealed class GitCredentialItem
    {
        private readonly CredentialItem _item;
        private string _value;

        internal GitCredentialItem(CredentialItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _item = item;

            SeedValue();
        }
    
        private void SeedValue()
        {
            var charArrayItem = _item as CredentialItem.CharArrayType;

            if (charArrayItem != null)
            {
                var value = charArrayItem.GetValue();

                _value = value != null ? new String(value) : null;
            }
            else
            {
                var stringItem = _item as CredentialItem.StringType;

                if (stringItem != null)
                {
                    _value = stringItem.GetValue();
                }
            }
        }

        public GitCredentialsType Type
        {
            get
            {
                if (_item is CredentialItem.InformationalMessage)
                    return GitCredentialsType.Informational;
                else if (_item is CredentialItem.Username)
                    return GitCredentialsType.Username;
                else if (_item is CredentialItem.Password)
                    return GitCredentialsType.Password;
                else if (_item is CredentialItem.YesNoType)
                    return GitCredentialsType.YesNo;
                else if (_item is CredentialItem.CharArrayType || _item is CredentialItem.StringType)
                    return GitCredentialsType.String;
                else
                    throw new NotSupportedException();
            }
        }

        public string PromptText
        {
            get
            {
                return _item.GetPromptText();
            }
        }

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                var charArrayItem = _item as CredentialItem.CharArrayType;

                if (charArrayItem != null)
                {
                    if (value == null)
                        charArrayItem.SetValue(null);
                    else
                        charArrayItem.SetValue(value.ToCharArray());
                }
                else
                {
                    var stringItem = _item as CredentialItem.StringType;

                    if (stringItem != null)
                    {
                        stringItem.SetValue(value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        public bool YesNoValue
        {
            get
            {
                var yesNoItem = _item as CredentialItem.YesNoType;

                if (yesNoItem != null)
                {
                    return yesNoItem.GetValue();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            set
            {
                var yesNoItem = _item as CredentialItem.YesNoType;

                if (yesNoItem != null)
                {
                    yesNoItem.SetValue(value);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
