using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Soylent
{
    public partial class SoylentPanel : UserControl
    {
        public static string HOSTNAME = "HITStatusHost";

        public SoylentPanel()
        {
            InitializeComponent();
        }

        private void WPFContainer_Load(object sender, EventArgs e)
        {
        }

        public HITStatus addHIT(string name, string text)
        {
            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Name = HOSTNAME;
            host.Dock = DockStyle.Fill;

            // Create the WPF UserControl.
            HITStatus hs = new HITStatus(name, text);

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = hs;

            // Add the ElementHost control to the form's
            // collection of child controls.
            this.Controls.Add(host);
            return hs;
        }

        public IEnumerable<HITStatus> getHITs()
        {
            List<HITStatus> hits = new List<HITStatus>();
            var temp = Controls.Find(HOSTNAME, true);
            //ElementHost[] hostControls = (ElementHost[]) Controls.Find(HOSTNAME, true);
            //return from control in hostControls select control.Child as HITStatus;
            return null;
        }
    }
}
