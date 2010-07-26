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

using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Soylent.Model;
using Soylent.Model.Shortn;
using Soylent.Model.Crowdproof;
using Soylent.Model.HumanMacro;
using Soylent.View;

using System.Text.RegularExpressions;

namespace Soylent
{
    public partial class Soylent
    {
        public Microsoft.Office.Tools.CustomTaskPane HITView;
        //public SoylentPanel soylent;
        private TurKitSocKit tksc;
        SoylentPanel first;
        public Dictionary<Word.Document, SoylentPanel> soylentMap = new Dictionary<Word.Document,SoylentPanel>();
        public Dictionary<int, Word.Document> jobToDoc = new Dictionary<int, Word.Document>();

        public static bool DEBUG = true;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            /*
            Word.Document doc = this.Application.ActiveDocument;
            SoylentPanel soylent = new SoylentPanel();
            soylentMap[doc] = soylent;


            HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
            HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);
            HITView.Visible = true;
            */
            

            tksc = new TurKitSocKit();
            tksc.Listen();

            //Microsoft.Office.Interop.Word.Application.DocumentOpen
            
            this.Application.DocumentOpen += new Word.ApplicationEvents4_DocumentOpenEventHandler(Application_DocumentOpen);
            this.Application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(Application_DocumentBeforeSave);
            this.Application.DocumentChange += new Word.ApplicationEvents4_DocumentChangeEventHandler(Application_DocumentChange);
        }

        void Application_DocumentChange()
        {
            
            Word.Document doc = this.Application.ActiveDocument;
            if (!soylentMap.Keys.Contains<Word.Document>(doc)){
                SoylentPanel soylent = new SoylentPanel();
                
                HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
                HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);
                HITView.Visible = true;

                addDocToMap(soylent);
            }
            
            
        }

        public void addDocToMap(SoylentPanel sp)
        {
            Word.Document doc = this.Application.ActiveDocument;
            soylentMap[doc] = sp;
        }


        void Application_DocumentOpen(Word.Document doc)
        {
            
            //SoylentPanel soylent = new SoylentPanel();
            //soylentMap[doc] = soylent; 
            //HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
            //HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);
            //HITView.Visible = true;
            SoylentPanel soylent = soylentMap[doc];
            

            foreach (Microsoft.Office.Core.CustomXMLPart xmlPart in Globals.Soylent.Application.ActiveDocument.CustomXMLParts)
            {
                string xml = xmlPart.XML;
                Regex typeRegex = new Regex("<job>(.*?)</job>");
                Match regexResult = typeRegex.Match(xml);
                string jobString = regexResult.ToString();
                if (jobString.Length < 6) { continue; }
                int job = Int32.Parse(jobString.Substring(5, jobString.Length - 11));

                jobToDoc[job] = doc;

                StringReader sr = new StringReader(xml);
                XmlReader xr = XmlReader.Create(sr);

                if (new Regex("</ShortnData>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ShortnData));
                    object raw = serializer.Deserialize(xr);

                    ShortnData hit = raw as ShortnData;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    SoylentRibbon.setLastJob(hit.job);
                    ShortnJob s = new ShortnJob(hit, hit.job);
                }

                else if (new Regex("</CrowdproofData>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CrowdproofData));
                    object raw = serializer.Deserialize(xr);

                    CrowdproofData hit = raw as CrowdproofData;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    SoylentRibbon.setLastJob(hit.job);
                    CrowdproofJob s = new CrowdproofJob(hit, hit.job);
                }

                else if (new Regex("</HumanMacroResult>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HumanMacroResult));
                    object raw = serializer.Deserialize(xr);

                    HumanMacroResult hit = raw as HumanMacroResult;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    SoylentRibbon.setLastJob(hit.job);
                    HumanMacroJob s = new HumanMacroJob(hit, hit.job);
                }
            }

        }

        void Application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            for (int i = 1; i <= soylentMap[Doc].jobMap.Keys.Count; i++)
            {
                HITData raw = soylentMap[Doc].jobMap[i];
                if (raw is ShortnData)
                {
                    ShortnData hit = raw as ShortnData;

                    XmlSerializer x = new XmlSerializer(hit.GetType());
                    StringWriter sw = new StringWriter();
                    x.Serialize(sw, hit);
                    string xml = sw.ToString();
                    Microsoft.Office.Core.CustomXMLPart xmlPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(xml);
                }
                else if (raw is CrowdproofData)
                {
                    CrowdproofData hit = raw as CrowdproofData;

                    XmlSerializer x = new XmlSerializer(hit.GetType());
                    StringWriter sw = new StringWriter();
                    x.Serialize(sw, hit);
                    string xml = sw.ToString();
                    Microsoft.Office.Core.CustomXMLPart xmlPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(xml);
                }
                else if (raw is HumanMacroResult)
                {
                    HumanMacroResult hit = raw as HumanMacroResult;

                    XmlSerializer x = new XmlSerializer(hit.GetType());
                    StringWriter sw = new StringWriter();
                    x.Serialize(sw, hit);
                    string xml = sw.ToString();
                    Microsoft.Office.Core.CustomXMLPart xmlPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(xml);
                }
            }
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
