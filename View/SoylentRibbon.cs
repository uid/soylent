using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Office.Tools.Ribbon;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms.Integration;
using System.Windows.Forms;
using System.Diagnostics;

using Soylent.Model;
using Soylent.Model.Crowdproof;
using Soylent.Model.Shortn;
using Soylent.Model.HumanMacro;
using Soylent.View;
using Soylent.View.Shortn;
using Soylent.View.HumanMacro;
using Soylent.View.Crowdproof;

using System.IO;
using System.Xml;
using System.Xml.Serialization;



namespace Soylent
{
    public partial class SoylentRibbon : OfficeRibbon
    {
        public SoylentRibbon()
        {
            InitializeComponent();
        }

        private void Ribbon_Load(object sender, RibbonUIEventArgs e)
        {
            //isTaskPaneVisible = Globals.Soylent.HITView.Visible;
        }

        private void shortenBtn_Click(object sender, RibbonControlEventArgs e)
        {
            Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            //ShortnData newHIT = new ShortnData(toShorten, jobNumber);
            //ShortnJob s = new ShortnJob(newHIT, jobNumber);
            ShortnJob s = new ShortnJob(jobNumber, toShorten);
        }

        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
            Word.Range toCrowdproof = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            //CrowdproofData newHIT = new CrowdproofData(toCrowdproof, jobNumber);
            //CrowdproofJob c = new CrowdproofJob(newHIT, jobNumber);
            CrowdproofJob c = new CrowdproofJob(jobNumber, toCrowdproof);
        }

        private void humanMacroBtn_Click(object sender, RibbonControlEventArgs e)
        {
            System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();
            newForm.Width = 1200;
            newForm.Height = 600;
            //newForm.BackColor = System.Drawing.Color.White;

            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Width = newForm.Width;
            host.Height = newForm.Height;

            //Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            //HumanMacroData newHIT = new HumanMacroData(Globals.Soylent.Application.Selection.Range, jobNumber, HumanMacroData.Separator.Sentence);
            //allHITs[newHIT.job] = newHIT;

            // Create the WPF UserControl.
            HumanMacroDialog hm = new HumanMacroDialog(Globals.Soylent.Application.Selection.Range, jobNumber);
            //hm.setText();

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = hm;

            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();
        }

        private void directManipulate_Click(object sender, RibbonControlEventArgs e)
        {
            ShortnView.openShortnDialog(ShortnData.getCannedData());
        }

        private void humanMacroInline_Click(object sender, RibbonControlEventArgs e)
        {
            // insert text
            Globals.Soylent.Application.Selection.Range.InsertAfter("Dugan, C., Muller, M., Millen, D.R., et al. \"The Dogear Game: A Social Bookmark Recommender System,\" GROUP '07, pp. 387-390, 2007.");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<string> results = new List<string>() {
                                    "Dugan, C., Muller, M., Millen, D.R., et al. 2007. The Dogear Game: A Social Bookmark Recommender System. In Proc. GROUP '07, 387-390.",
                                    "DUGAN, C., MULLER, M., MILLEN, D.R., ET AL. 2007. The dogear game: a social book-mark recommender system. In Proc. of GROUP '07, ACM Press, 387–390."
                                   };
            //HumanMacroData result = new HumanMacroData(canned_range, results);
            //result.AnnotateResult(HumanMacroData.ResultType.SmartTag);
        }

        private void humanMacroComment_Click(object sender, RibbonControlEventArgs e)
        {
            // insert text
            Globals.Soylent.Application.Selection.Range.InsertAfter("Rob Miller is my advisor.");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<string> results = new List<string>() {
                                    "Oh dude, I love that guy!",
                                    "I hear he's a big fan of Terminator 1."
                                   };

            //HumanMacroData result = new HumanMacroData(canned_range, results);
            //result.AnnotateResult(HumanMacroData.ResultType.Comment);
        }

        private void button1_Click(object sender, RibbonControlEventArgs e)
        {
            /*
            Globals.Soylent.Application.ActiveDocument.ShowRevisions = true;//!Globals.Soylent.Application.ActiveDocument.ShowRevisions;
            Globals.Soylent.Application.ActiveDocument.TrackRevisions = !Globals.Soylent.Application.ActiveDocument.TrackRevisions;
            Word.Range range = Globals.Soylent.Application.ActiveDocument.Range(0, 4);
            range.Text = Globals.Soylent.Application.ActiveDocument.TrackRevisions.ToString();
             * */

            CrowdproofData data = CrowdproofData.getCannedData();
            //CrowdproofView.insertTrackChanges(data);

            //Globals.Soylent.Application.ActiveDocument.TrackRevisions = true;
            /*
            TurKitSocKit tksc = new TurKitSocKit();
            tksc.Listen();
            */
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {

            //I is trying to learn how to write gooder.
            TurKitSocKit.TurKitFindFixVerifyPatch p1 = new TurKitSocKit.TurKitFindFixVerifyPatch();
            p1.start = 4; p1.end = 24;
            p1.options = new List<TurKitSocKit.TurKitFindFixVerifyOption>();
            TurKitSocKit.TurKitFindFixVerifyOption option = new TurKitSocKit.TurKitFindFixVerifyOption();
            option.editsText = true;
            option.field = "revision";
            option.alternatives = new List<TurKitSocKit.TurKitFindFixVerifyAlternative>();
            p1.options.Add(option);

            TurKitSocKit.TurKitFindFixVerifyAlternative p1a = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            TurKitSocKit.TurKitFindFixVerifyAlternative p1b = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            TurKitSocKit.TurKitFindFixVerifyAlternative p1c = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            p1a.text="figuring"; p1b.text="trying to figure"; p1c.text="working out";

            option.alternatives.Add(p1a); option.alternatives.Add(p1b); option.alternatives.Add(p1c);

            TurKitSocKit.TurKitFindFixVerifyPatch p2 = new TurKitSocKit.TurKitFindFixVerifyPatch();
            p2.start = 64; p2.end = 70;

            TurKitSocKit.TurKitFindFixVerifyOption option2 = new TurKitSocKit.TurKitFindFixVerifyOption();
            option2.editsText = true;
            option2.field = "revision";
            option2.alternatives = new List<TurKitSocKit.TurKitFindFixVerifyAlternative>();

            TurKitSocKit.TurKitFindFixVerifyAlternative p2a = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            TurKitSocKit.TurKitFindFixVerifyAlternative p2b = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            TurKitSocKit.TurKitFindFixVerifyAlternative p2c = new TurKitSocKit.TurKitFindFixVerifyAlternative();
            p2a.text = "proper"; p2b.text = "right"; p2c.text = "yes";
            option2.alternatives.Add(p2a); option2.alternatives.Add(p2b); option2.alternatives.Add(p2c);

            TurKitSocKit.TurKitFindFixVerify tks = new TurKitSocKit.TurKitFindFixVerify();
            tks.job = 1; tks.paragraph = 1; tks.patches = new List<TurKitSocKit.TurKitFindFixVerifyPatch>();
            tks.patches.Add(p1); tks.patches.Add(p2);

            ShortnData shortenData = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[tks.job] as ShortnData;
            shortenData.processSocKitMessage(tks);
            

            System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();
            //newForm.Width = 1195;
            //newForm.Height = 380;
            newForm.BackColor = System.Drawing.Color.White;

            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Width = newForm.Width;
            host.Height = newForm.Height;

            // Create the WPF UserControl.
            //Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            ShortnDialog sd = new ShortnDialog(Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[1] as ShortnData);

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.

            host.Child = sd;

            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();
        }
        internal static void setLastJob(int i)
        {
            lastJob = Math.Max(lastJob, i);
        }

        private static int lastJob = 0;
        public static int generateJobNumber()
        {
            //System.Diagnostics.Trace.WriteLine(lastJob + 1);
            return ++lastJob;
            
        }

        private void jobStatus_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.Soylent.HITView.Visible = ((RibbonToggleButton)sender).Checked;
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            //int i = generateJobNumber();
            Word.Document doc = Globals.Soylent.Application.ActiveDocument;
            
            /*
            ElementHost host = new ElementHost();
            host.Name = "HITViewHost";
            host.Dock = DockStyle.Fill;

            Sidebar hs = new Sidebar();
            host.Child = hs;
            Globals.Soylent.soylent.Controls.Add(host);

            Word.Range toCrowdproof = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            CrowdproofData data = new CrowdproofData(toCrowdproof, jobNumber);

            CrowdproofView hit = Globals.Soylent.soylent.addHITtoList("Crowdproof", data, jobNumber) as CrowdproofView;
            hit.addStage(1, HITData.ResultType.Find, data.findStageData, "Identify Errors", 10, 0.10);
            hit.addStage(2, HITData.ResultType.Fix, data.fixStageData, "Fix Errors", 5, 0.05);
            hit.addStage(3, HITData.ResultType.Verify, data.verifyStageData, "Quality Control", 5, 0.05);

            hs.addHitView(jobNumber,hit);



            Word.Range toCrowdproof2 = Globals.Soylent.Application.Selection.Range;
            int jobNumber2 = generateJobNumber();
            CrowdproofData data2 = new CrowdproofData(toCrowdproof2, jobNumber2);

            CrowdproofView hit2 = Globals.Soylent.soylent.addHITtoList("Crowdproof", data2, jobNumber2) as CrowdproofView;
            hit2.addStage(1, HITData.ResultType.Find, data2.findStageData, "Identify Errors", 10, 0.10);
            hit2.addStage(2, HITData.ResultType.Fix, data2.fixStageData, "Fix Errors", 5, 0.05);
            hit2.addStage(3, HITData.ResultType.Verify, data2.verifyStageData, "Quality Control", 5, 0.05);

            hs.addHitView(jobNumber2, hit2);



            Word.Range toCrowdproof3 = Globals.Soylent.Application.Selection.Range;
            int jobNumber3 = generateJobNumber();
            CrowdproofData data3 = new CrowdproofData(toCrowdproof3, jobNumber3);

            CrowdproofView hit3 = Globals.Soylent.soylent.addHITtoList("Crowdproof", data3, jobNumber3) as CrowdproofView;
            hit3.addStage(1, HITData.ResultType.Find, data3.findStageData, "Identify Errors", 10, 0.10);
            hit3.addStage(2, HITData.ResultType.Fix, data3.fixStageData, "Fix Errors", 5, 0.05);
            hit3.addStage(3, HITData.ResultType.Verify, data3.verifyStageData, "Quality Control", 5, 0.05);

            hs.addHitView(jobNumber3, hit3);
            */
        }

        public class Sample
        {
            public List<int> list;
            public Sample()
            {
                list = new List<int>();
                list.Add(1);
            }
        }

        private void button5_Click(object sender, RibbonControlEventArgs e)
        {
            //fake p = new fake();

            //HITData p = new HITData();

            //ShortnData p = ShortnData.getCannedData();
            
            /*
            TurKitSocKit.TurKitFindFixVerify tks = new TurKitSocKit.TurKitFindFixVerify();
            //TurKitSocKit.TurKitFindFixVerifyOption tks = new TurKitSocKit.TurKitFindFixVerifyOption();
            tks.job = 3;
            tks.paragraph = 2;
            tks.patches = new List<TurKitSocKit.TurKitFindFixVerifyPatch>();
            */
            //Sample tks = new Sample();

            //tks.list.Add(2);
            //tks.list.Add(3);

            HumanMacroData tks = new HumanMacroData();
            

            XmlSerializer x = new XmlSerializer(tks.GetType());
            StringWriter sw = new StringWriter();
            x.Serialize(sw, tks);
            string a = sw.ToString();
            Debug.Write(a);

            /*
            CrowdproofData p = CrowdproofData.getCannedData();

            XmlSerializer x = new XmlSerializer(p.GetType());
            StringWriter sw = new StringWriter();
            x.Serialize(sw, p);
            string a = sw.ToString();
            Debug.Write(a);

            Microsoft.Office.Core.CustomXMLPart employeeXMLPart = Globals.Soylent.Application.ActiveDocument.CustomXMLParts.Add(a);
            */
             
            /*
            string s = "";

            foreach (Microsoft.Office.Core.CustomXMLPart cus in Globals.Soylent.Application.ActiveDocument.CustomXMLParts)
            {
                s = cus.XML;

            }

            
            XmlSerializer y = new XmlSerializer(p.GetType());

            //StringReader sr = new StringReader(a);
            StringReader sr = new StringReader(s);

            XmlReader z = XmlReader.Create(sr);
            object obj = y.Deserialize(z);

            CrowdproofData q = obj as CrowdproofData;

            Word.Bookmark b = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + q.job];
            q.range = b.Range;         
            */
        }

        private void button6_Click(object sender, RibbonControlEventArgs e)
        {
            string s = "";

            foreach (Microsoft.Office.Core.CustomXMLPart cus in Globals.Soylent.Application.ActiveDocument.CustomXMLParts)
            {
                s = cus.XML;

            }

            XmlSerializer y = new XmlSerializer(typeof(CrowdproofData));

            //StringReader sr = new StringReader(a);
            StringReader sr = new StringReader(s);

            XmlReader z = XmlReader.Create(sr);
            object obj = y.Deserialize(z);

            CrowdproofData q = obj as CrowdproofData;

            Word.Bookmark a = Globals.Soylent.Application.ActiveDocument.Bookmarks["Soylent" + q.job];
            q.range = a.Range;
        }
    }
}
