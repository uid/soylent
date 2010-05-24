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
        private Microsoft.Office.Tools.CustomTaskPane hitStatus;
        public SoylentPanel soylent;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            soylent = new SoylentPanel();
            hitStatus = this.CustomTaskPanes.Add(soylent, "Soylent");
            hitStatus.Visible = true;
        }


        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            //foreach (Microsoft.Office.Tools.SmartTagBase tag in VstoSmartTags)
            //{
            //    if (tag.GetType().Namespace == HumanMacroResult.NAMESPACE)
            //    {
            //        VstoSmartTags.Remove(tag);
            //    }
            //}
            //this.CustomTaskPanes.Remove(hitStatus);
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
