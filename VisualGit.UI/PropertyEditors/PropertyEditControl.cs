using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using SharpSvn;

namespace VisualGit.UI.PropertyEditors
{
    class PropertyEditControl : UserControl
    {
        public PropertyEditControl()
        {
            Size = new Size(348, 196);
        }

        [Localizable(true), DefaultValue(typeof(Size), "348;196")]
        public new Size Size
        {
            get { return base.Size; }
            set { base.Size = value; }
        }

        public virtual SvnPropertyValue PropertyItem
        {
            get { return null; }
            set { throw new InvalidOperationException(); }
        }

        string _propertyName;

        public virtual string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        public virtual bool Valid
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the kind of the allowed node.
        /// </summary>
        /// <returns></returns>
        /// <remarks>This code assumes .File and .Directory are bitflags</remarks>
        public virtual bool AllowNodeKind(SvnNodeKind kind)
        {
            return true;
        }

        public event EventHandler Changed;

        /// <summary>
        /// Raises the <see cref="E:Changed"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        /// <summary>
        /// Gets the VisualGit context
        /// </summary>
        /// TODO should we make this a settable property?
        protected virtual IVisualGitServiceProvider Context
        {
            get
            {
                IVisualGitServiceProvider context = null;
                Control parent = this;
                while (parent != null)
                {
                    if (parent is VSDialogForm)
                    {
                        context = ((VSDialogForm)parent).Context;
                    }
                    parent = context == null ? parent.Parent : null;
                }
                return context;
            }
        }
    }
}
