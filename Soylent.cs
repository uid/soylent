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
using System.Windows.Forms.Integration;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;


using Soylent.View;

namespace Soylent
{
    public partial class Soylent
    {
        public Microsoft.Office.Tools.CustomTaskPane HITView;
        public SoylentPanel soylent;
        private TurKitSocKit tksc;

        public static bool DEBUG = true;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            soylent = new SoylentPanel();
            HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
            HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);
            HITView.Visible = true;

            tksc = new TurKitSocKit();
            tksc.Listen();

            //Microsoft.Office.Interop.Word.Application.DocumentOpen
            
            this.Application.DocumentOpen += new Word.ApplicationEvents4_DocumentOpenEventHandler(Application_DocumentOpen);
            this.Application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(Application_DocumentBeforeSave);
        }

        void Application_DocumentOpen(Word.Document Doc)
        {
            throw new NotImplementedException();
        }

        void Application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            throw new NotImplementedException();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {

        }

        void hitviewVisibleChanged(object sender, EventArgs e)
        {
            Globals.Ribbons.Ribbon.jobStatus.Checked = HITView.Visible;
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
