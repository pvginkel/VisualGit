using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using VisualGit.VS;
using System.Diagnostics;

namespace VisualGit.UI.SccManagement
{
    public partial class AddProjectToGit : AddToGit
    {
        public AddProjectToGit()
        {
            InitializeComponent();
        }

        protected override void ValidateAdd(object sender, CancelEventArgs e)
        {
            base.ValidateAdd(sender, e);

            if (e.Cancel)
                return;

            Debug.Assert(RepositoryAddUrl != null);

            IVisualGitSolutionSettings ss = Context.GetService<IVisualGitSolutionSettings>();

            // Error if the RepositoryAddUrl is below the url of the projectroot
            if (ss.ProjectRootUri.IsBaseOf(RepositoryAddUrl))
            {
                e.Cancel = true;

                errorProvider1.SetError(repositoryTree, "Please select a location that is not below the solution binding path, or move the project to a directory below the solution binding path on disk");
                return;
            }
        }

        [DefaultValue(true)]
        public bool MarkAsManaged
        {
            get { return markAsManaged.Checked; }
            set { markAsManaged.Checked = value; }
        }

        [DefaultValue(true)]
        public bool WriteCheckOutInformation
        {
            get { return writeUrlInSolution.Checked; }
            set { writeUrlInSolution.Checked = value; }
        }
    }
}
