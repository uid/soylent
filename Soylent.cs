using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Controls;


namespace Soylent
{
    public partial class Soylent
    {
        public Microsoft.Office.Tools.CustomTaskPane HITView;
        public SoylentPanel soylent;
        private TurKitSocKit tksc;
        private SoylentRibbon ribbon;

        public static bool DEBUG = false;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            soylent = new SoylentPanel();
            HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
            HITView.Visible = true;
            HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);

            tksc = new TurKitSocKit();
            tksc.Listen();

            // TODO: figure out ribbon and set it to the private variable
        }


        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        void hitviewVisibleChanged(object sender, EventArgs e)
        {
            // TODO: set ribbon here
            //Globals.Ribbons.SoylentRibbon.IsTaskPaneVisible = ctp.Visible;

        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
