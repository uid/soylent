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

using Soylent.Model;
using Soylent.Model.Crowdproof;
using Soylent.Model.Shortn;
using Soylent.Model.HumanMacro;
using Soylent.View;
using Soylent.View.Shortn;
using Soylent.View.HumanMacro;
using Soylent.View.Crowdproof;

namespace Soylent
{
    public partial class SoylentRibbon : OfficeRibbon
    {
        public Dictionary<int, HITData> allHITs = new Dictionary<int,HITData>();

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
            ShortnData newHIT = new ShortnData(toShorten, jobNumber);
            allHITs[newHIT.job] = newHIT;
            ShortnJob s = new ShortnJob(newHIT, jobNumber);
        }

        private void directManipulate_Click(object sender, RibbonControlEventArgs e)
        {
            //ShortnView.openShortnDialog(ShortnData.getCannedData());
            /*
            TurKitSocKit.TurKitStatus receivedObject = new TurKitSocKit.TurKitStatus();
            receivedObject.hitURL = "http://www.google.com";
            receivedObject.job = 1;
            receivedObject.numCompleted = 1;
            receivedObject.paragraph = 0;
            receivedObject.patchNumber = 0;
            receivedObject.payment = .05;
            receivedObject.stage = "find";
            receivedObject.totalPatches = 1;
            receivedObject.totalRequested = 7;
            */
            TurKitSocKit.TurKitCrowdproof crowdproof = new TurKitSocKit.TurKitCrowdproof();
            crowdproof.job = 1;
            crowdproof.paragraph = 0;
            crowdproof.patches = new List<TurKitSocKit.TurKitCrowdproofPatch>();
            TurKitSocKit.TurKitCrowdproofPatch patch1 = new TurKitSocKit.TurKitCrowdproofPatch();
            patch1.start = 2;
            patch1.end = 2;
            List<string> reasons = new List<string>();
            reasons.Add("Subject/Verb agreement");
            List<TurKitSocKit.TurKitCrowdproofPatchOption> options = new List<TurKitSocKit.TurKitCrowdproofPatchOption>();
            TurKitSocKit.TurKitCrowdproofPatchOption op1 = new TurKitSocKit.TurKitCrowdproofPatchOption();
            op1.editStart = 2;
            op1.editEnd = 4;
            op1.replacement = "am";
            op1.text = "is";
            options.Add(op1);
            patch1.reasons = reasons;
            patch1.originalText = "is";
            patch1.options = options;
            patch1.numEditors = 2;
            patch1.editStart = 2;
            patch1.editEnd = 4;
            crowdproof.patches.Add(patch1);

            TurKitSocKit.TurKitCrowdproofPatch patch2 = new TurKitSocKit.TurKitCrowdproofPatch();
            patch2.start = 34;
            patch2.end = 40;
            List<string> reasons2 = new List<string>();
            reasons2.Add("'Gooder' is not a word");
            reasons2.Add("This is awful.  Use 'better'");
            List<TurKitSocKit.TurKitCrowdproofPatchOption> options2 = new List<TurKitSocKit.TurKitCrowdproofPatchOption>();
            TurKitSocKit.TurKitCrowdproofPatchOption op2 = new TurKitSocKit.TurKitCrowdproofPatchOption();
            op2.editStart = 34;
            op2.editEnd = 40;
            op2.replacement = "better";
            op2.text = "gooder";
            options2.Add(op2);
            TurKitSocKit.TurKitCrowdproofPatchOption op3 = new TurKitSocKit.TurKitCrowdproofPatchOption();
            op3.editStart = 34;
            op3.editEnd = 38;
            op3.replacement = "well";
            op3.text = "gooder";
            options2.Add(op3);
            patch2.reasons = reasons2;
            patch2.originalText = "is";
            patch2.options = options2;
            patch2.numEditors = 2;
            patch2.editStart = 34;
            patch2.editEnd = 40;
            crowdproof.patches.Add(patch2);
            
            CrowdproofData cpd = Globals.Soylent.soylent.jobMap[crowdproof.job] as CrowdproofData;
            //cpd.processSocKitMessage(crowdproof);
            //cpd.updateStatus(receivedObject);
            
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

            // Create the WPF UserControl.
            HumanMacroDialog hm = new HumanMacroDialog(Globals.Soylent.Application.Selection.Range);
            //hm.setText();

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = hm;

            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();
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
            HumanMacroResult result = new HumanMacroResult(canned_range, results);
            result.AnnotateResult(HumanMacroResult.ResultType.SmartTag);
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

            HumanMacroResult result = new HumanMacroResult(canned_range, results);
            result.AnnotateResult(HumanMacroResult.ResultType.Comment);
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

            ShortnData shortenData = Globals.Soylent.soylent.jobMap[tks.job] as ShortnData;
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
            ShortnDialog sd = new ShortnDialog(Globals.Soylent.soylent.jobMap[1] as ShortnData);

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.

            host.Child = sd;

            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();
        }

        private static int lastJob = 0;
        public static int generateJobNumber()
        {
            System.Diagnostics.Trace.WriteLine(lastJob + 1);
            return ++lastJob;
        }

        private void button3_Click(object sender, RibbonControlEventArgs e)
        {
            //CrowdproofData data = CrowdproofData.getCannedData();

            Word.Range toCrowdproof = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            CrowdproofData newHIT = new CrowdproofData(toCrowdproof, jobNumber);
            allHITs[newHIT.job] = newHIT;
            CrowdproofJob c = new CrowdproofJob(newHIT, jobNumber);
            //CrowdproofData data = CrowdproofData.getCannedData();
            //data.AnnotateResult();
        }

        private void jobStatus_Click(object sender, RibbonControlEventArgs e)
        {
            Globals.Soylent.HITView.Visible = ((RibbonToggleButton)sender).Checked;
        }

        private void button4_Click(object sender, RibbonControlEventArgs e)
        {
            //Globals.Soylent.Application.ActiveDocument.Final = true;
            Globals.Soylent.Application.ActiveDocument.DeleteAllComments();
            Globals.Soylent.Application.ActiveDocument.AcceptAllRevisions();
        }
    }
}
