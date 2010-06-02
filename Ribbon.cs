using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms.Integration;

namespace Soylent
{
    public partial class Ribbon : OfficeRibbon
    {
        public Dictionary<int, HITData> allHITs = new Dictionary<int,HITData>();

        public Ribbon()
        {
            InitializeComponent();
        }

        private void Ribbon_Load(object sender, RibbonUIEventArgs e)
        {

        }

        private void shortenBtn_Click(object sender, RibbonControlEventArgs e)
        {
            Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            int jobNumber = generateJobNumber();
            ShortenData newHIT = new ShortenData(toShorten, jobNumber);
            allHITs[newHIT.job] = newHIT;
            Shorten s = new Shorten(newHIT, jobNumber);
        }

        private void directManipulate_Click(object sender, RibbonControlEventArgs e)
        {
            System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();
            newForm.Width = 1195;
            newForm.Height = 1000;
            newForm.BackColor = System.Drawing.Color.White;

            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Width = newForm.Width;
            host.Height = newForm.Height;

            // Create the WPF UserControl.
            Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            ShortenDialog sd = new ShortenDialog(ShortenData.getCannedData());

            // Assign the WPF UserControl to the ElementHost control's
            // Child property.
            host.Child = sd;


            newForm.Visible = false;
            // Add the ElementHost control to the form's
            // collection of child controls.
            newForm.Controls.Add(host);
            newForm.Show();

            double beforeHeight = sd.before.DesiredSize.Height;
            newForm.Height = (int)(beforeHeight + 60);
            sd.lengthSlider.Height = (int)(beforeHeight - 20);
            
            newForm.Visible = true;
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
            TurKitSocKit tksc = new TurKitSocKit();
            tksc.Listen();
        }

        private void button2_Click(object sender, RibbonControlEventArgs e)
        {
           
            TurKitSocKit.TurKitShortnPatch p1 = new TurKitSocKit.TurKitShortnPatch();
            p1.start = 4; p1.end = 24;
            p1.options = new List<TurKitSocKit.TurKitShortnPatchOption>();
            TurKitSocKit.TurKitShortnPatchOption p1a = new TurKitSocKit.TurKitShortnPatchOption();
            TurKitSocKit.TurKitShortnPatchOption p1b = new TurKitSocKit.TurKitShortnPatchOption();
            TurKitSocKit.TurKitShortnPatchOption p1c = new TurKitSocKit.TurKitShortnPatchOption();
            p1a.text="figuring"; p1b.text="trying to figure"; p1c.text="working out";
            
            p1.options.Add(p1a); p1.options.Add(p1b); p1.options.Add(p1c);

            TurKitSocKit.TurKitShortnPatch p2 = new TurKitSocKit.TurKitShortnPatch();
            p2.start = 64; p2.end = 70;
            p2.options = new List<TurKitSocKit.TurKitShortnPatchOption>();
            TurKitSocKit.TurKitShortnPatchOption p2a = new TurKitSocKit.TurKitShortnPatchOption();
            TurKitSocKit.TurKitShortnPatchOption p2b = new TurKitSocKit.TurKitShortnPatchOption();
            TurKitSocKit.TurKitShortnPatchOption p2c = new TurKitSocKit.TurKitShortnPatchOption();
            p2a.text = "proper"; p2b.text = "right"; p2c.text = "yes";
            p2.options.Add(p2a); p2.options.Add(p2b); p2.options.Add(p2c);

            TurKitSocKit.TurKitShortn tks = new TurKitSocKit.TurKitShortn();
            tks.job = 1; tks.paragraph = 1; tks.patches = new List<TurKitSocKit.TurKitShortnPatch>();
            tks.patches.Add(p1); tks.patches.Add(p2);

            ShortenData shortenData = Globals.Soylent.soylent.jobMap[tks.job] as ShortenData;
            shortenData.processSocKitMessage(tks);
            

            System.Windows.Forms.Form newForm = new System.Windows.Forms.Form();
            newForm.Width = 1195;
            newForm.Height = 380;
            newForm.BackColor = System.Drawing.Color.White;

            // Create the ElementHost control for hosting the
            // WPF UserControl.
            ElementHost host = new ElementHost();
            host.Width = newForm.Width;
            host.Height = newForm.Height;

            // Create the WPF UserControl.
            //Word.Range toShorten = Globals.Soylent.Application.Selection.Range;
            ShortenDialog sd = new ShortenDialog(Globals.Soylent.soylent.jobMap[1] as ShortenData);

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
            ProofreadingData data = ProofreadingData.getCannedData();
            data.AnnotateResult();
        }
    }
}
