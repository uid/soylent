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
using Soylent.View.Crowdproof;

using System.Text.RegularExpressions;

namespace Soylent
{
    public partial class Soylent
    {
        public Microsoft.Office.Tools.CustomTaskPane HITView;
        //public SoylentPanel soylent;
        //private TurKitSocKit tksc;
        private TurKitHTTP tkhttp;
        public JobManager jobManager;

        public Dictionary<Word.Document, SoylentPanel> soylentMap = new Dictionary<Word.Document,SoylentPanel>();
        public Dictionary<int, Word.Document> jobToDoc = new Dictionary<int, Word.Document>();

        public static bool DEBUG = false;

        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            //Globals.Ribbons.Ribbon.debug.Visible = true;

            jobManager = new JobManager();

            tkhttp = new TurKitHTTP();
            tkhttp.Listen();

            //For save/load of hits
            this.Application.DocumentOpen += new Word.ApplicationEvents4_DocumentOpenEventHandler(Application_DocumentOpen);
            this.Application.DocumentBeforeSave += new Word.ApplicationEvents4_DocumentBeforeSaveEventHandler(Application_DocumentBeforeSave);
            this.Application.DocumentChange += new Word.ApplicationEvents4_DocumentChangeEventHandler(Application_DocumentChange);
        }

        void Application_DocumentChange()
        {          
            Word.Document doc = this.Application.ActiveDocument;
            // If the document is already open, don't worry about it.  If not (e.g. created a new document, loaded old), set up its panel
            if (!soylentMap.Keys.Contains<Word.Document>(doc)){ 
                SoylentPanel soylent = new SoylentPanel();
                
                HITView = this.CustomTaskPanes.Add(soylent, "Soylent");
                HITView.VisibleChanged += new EventHandler(hitviewVisibleChanged);
                //HITView.Visible = true;

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
            SoylentPanel soylent = soylentMap[doc];
            List<string> rawHITs = new List<string>();
            
            //One problem: loads jobs in reverse order.  This is easy to fix.  But is either correct?
            foreach (Microsoft.Office.Core.CustomXMLPart xmlPart in Globals.Soylent.Application.ActiveDocument.CustomXMLParts)
            {
                string xml = xmlPart.XML;
                Regex typeRegex = new Regex("<job>(.*?)</job>"); //To filter out Soylent jobs from the xml parts Word automatically saves
                Match regexResult = typeRegex.Match(xml);
                string jobString = regexResult.ToString();
                if (jobString.Length < 6) { continue; }

                int job = Int32.Parse(jobString.Substring(5, jobString.Length - 11));

                jobToDoc[job] = doc;
                rawHITs.Add(xml);
            }
            rawHITs.Reverse();

            foreach(string xml in rawHITs)
            {
                StringReader sr = new StringReader(xml);
                XmlReader xr = XmlReader.Create(sr);

                if (new Regex("</ShortnData>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ShortnData));
                    object raw = serializer.Deserialize(xr);

                    ShortnData hit = raw as ShortnData;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    //SoylentRibbon.setLastJob(hit.job);


                    if (hit.jobDone){
                        ShortnJob s = new ShortnJob(hit, hit.job, false);
                        //Use saved TurKit messages to recreate the results.
                        foreach (TurKitSocKit.TurKitStageComplete message in hit.stageCompletes)
                        {
                            hit.terminateStage(message);
                        }
                        foreach (TurKitSocKit.TurKitFindFixVerify message in hit.findFixVerifies){
                            hit.postProcessSocKitMessage(message);
                        }
                        
                    }
                    else{
                        // This will work if you are restarting it on the same machine, where the
                        // TurKit javascript file still sits. Otherwise, it will restart the job.
                        ShortnJob s = new ShortnJob(hit, hit.job, true);
                    }


                }

                else if (new Regex("</CrowdproofData>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CrowdproofData));
                    object raw = serializer.Deserialize(xr);

                    CrowdproofData hit = raw as CrowdproofData;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    //SoylentRibbon.setLastJob(hit.job);

                    if (hit.jobDone)
                    {
                        CrowdproofJob s = new CrowdproofJob(hit, hit.job, false);
                        //Use saved TurKit messages to recreate the results.
                        foreach (TurKitSocKit.TurKitStageComplete message in hit.stageCompletes)
                        {
                            hit.terminateStage(message);
                        }
                        foreach (TurKitSocKit.TurKitFindFixVerify message in hit.findFixVerifies)
                        {
                            hit.postProcessSocKitMessage(message);
                        }
                    }
                    else
                    {
                        CrowdproofJob s = new CrowdproofJob(hit, hit.job, true);
                    }
                }

                else if (new Regex("</HumanMacroData>").Match(xml).Length > 0)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(HumanMacroData));
                    object raw = serializer.Deserialize(xr);

                    HumanMacroData hit = raw as HumanMacroData;

                    Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + hit.job];
                    hit.range = a.Range;

                    //SoylentRibbon.setLastJob(hit.job);

                    if (hit.jobDone)
                    {
                        HumanMacroJob s = new HumanMacroJob(hit, hit.job, false);
                        //Use saved TurKit messages to recreate the results.
                        foreach (TurKitSocKit.TurKitHumanMacroResult message in hit.messages)
                        {
                            hit.postProcessSocKitMessage(message);
                        }
                        hit.finishStageData();
                    }
                    else
                    {
                        HumanMacroJob s = new HumanMacroJob(hit, hit.job, true);
                    }
                }
            }

        }

        void Application_DocumentBeforeSave(Word.Document Doc, ref bool SaveAsUI, ref bool Cancel)
        {
            foreach (Microsoft.Office.Core.CustomXMLPart xmlPart in Globals.Soylent.Application.ActiveDocument.CustomXMLParts)
            {
                if (!(xmlPart.BuiltIn))
                {
                    xmlPart.Delete();
                }
            }
            foreach (object obj in soylentMap[Doc].sidebar.jobs.Children)
            {
                if (!(obj is StackPanel)) { continue; }
                StackPanel elem = obj as StackPanel;
                foreach (object elem2 in elem.Children)
                {
                    HITData raw;
                    if (elem2 is HITView)
                    {
                        raw = (elem2 as HITView).data;
                    }
                    else
                    {
                        raw = (elem2 as HITViewStub).data;
                    }

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
                    else if (raw is HumanMacroData)
                    {
                        HumanMacroData hit = raw as HumanMacroData;

                        XmlSerializer x = new XmlSerializer(hit.GetType());
                        StringWriter sw = new StringWriter();
                        x.Serialize(sw, hit);
                        string xml = sw.ToString();
                        Microsoft.Office.Core.CustomXMLPart xmlPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(xml);
                    }

                }
            }
            /*
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
                else if (raw is HumanMacroData)
                {
                    HumanMacroData hit = raw as HumanMacroData;

                    XmlSerializer x = new XmlSerializer(hit.GetType());
                    StringWriter sw = new StringWriter();
                    x.Serialize(sw, hit); 
                    string xml = sw.ToString();
                    Microsoft.Office.Core.CustomXMLPart xmlPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(xml);
                }
             
            }*/
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
