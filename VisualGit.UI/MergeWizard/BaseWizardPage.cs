using System;
using VisualGit.UI.WizardFramework;

namespace VisualGit.UI.MergeWizard
{
    public partial class BaseWizardPage : WizardPage
    {
        protected BaseWizardPage()
        {
            InitializeComponent();
        }

        public new MergeWizard Wizard
        {
            get { return (MergeWizard)base.Wizard; }
        }

        protected internal override void OnAfterAdd(WizardPageCollection collection)
        {
            base.OnAfterAdd(collection);

            Wizard.PageChanged += new EventHandler(WizardDialog_PageChangeEvent);
            Wizard.PageChanging += new EventHandler<WizardPageChangingEventArgs>(WizardDialog_PageChangingEvent);
        }


        void WizardDialog_PageChangingEvent(object sender, WizardPageChangingEventArgs e)
        {
            if (Wizard.CurrentPage == this)
                OnPageChanging(e);
        }
        void WizardDialog_PageChangeEvent(object sender, EventArgs e)
        {
            if (Wizard.CurrentPage.PreviousPage == this)
                OnPageChanged(e);
        }

        protected virtual void OnPageChanging(WizardPageChangingEventArgs e)
        {
        }

        protected virtual void OnPageChanged(EventArgs e)
        {
        }

    }
}
