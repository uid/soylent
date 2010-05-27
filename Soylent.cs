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
        private Microsoft.Office.Tools.CustomTaskPane HITView;
        public SoylentPanel soylent;
        private TurKitSocKit tksc;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            soylent = new SoylentPanel();
            HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
            HITView.Visible = true;

            tksc = new TurKitSocKit();
            tksc.Listen();
        }


        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

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
