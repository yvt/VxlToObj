using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VxlToObj.Shell.Gui
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            var corever = typeof(VxlToObj.Core.MeshSlice).Assembly.GetName().Version.ToString();
            var guiver = GetType().Assembly.GetName().Version.ToString();
            versionLabel.Text = $"Core: {corever}, GUI: {guiver}";
        }
    }
}
