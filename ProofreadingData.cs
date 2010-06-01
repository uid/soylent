using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using VSTO = Microsoft.Office.Tools.Word;

namespace Soylent
{
    class ProofreadingData : HITData
    {
        public List<ProofreadingPatch> patches { get; set;}

        public ProofreadingData(Word.Range range, int job)
            : base(range, job)
        {
        }

        public void AnnotateResult()
        {
            Word.Range text = range;
            foreach (ProofreadingPatch pp in patches)
            {
                string tag = HumanMacroResult.NAMESPACE + "#soylent" + DateTime.Now.Ticks;
                SmartTag resultTag = new SmartTag(tag, pp.reasons[0]);
                Regex pattern = new Regex(pp.original.Text.Trim().Replace(" ", "\\s"), RegexOptions.IgnorePatternWhitespace);
                resultTag.Expressions.Add(pattern);

                List<VSTO.Action> actions = new List<Microsoft.Office.Tools.Word.Action>();
                foreach (string result in pp.replacements)
                {
                    VSTO.Action action = new VSTO.Action(result);
                    action.Click += new ActionClickEventHandler(HumanMacroResult.replaceText);
                    actions.Add(action);
                }

                foreach (string reason in pp.reasons)
                {
                    VSTO.Action action = new VSTO.Action("Error Descriptions///" + reason);
                    action.Click += new ActionClickEventHandler(HumanMacroResult.replaceText);
                    actions.Add(action);
                }

                resultTag.Actions = actions.ToArray();
                Globals.Soylent.VstoSmartTags.Add(resultTag);
            }
        }

        public static ProofreadingData getCannedData()
        {
            // insert text
            Globals.Soylent.Application.Selection.Range.InsertAfter("However, while GUI made using computers be more intuitive and easier to learn, it didn't let people be able to control computers efficiently.  Masses only can use the software developed by software companies, unless they know how to write programs.  In other words, if one who knows nothing about programming needs to click through 100 buttons to complete her job everyday, the only thing she can do is simply to click through those buttons by hand every time.  But if she happens to be a computer programmer, there is a little chance that she can write a program to automate everything.  Why is there only a little chance?  In fact, each GUI application is a big black box, which usually have no outward interfaces for connecting to other programs.  In other words, this truth builds a great wall between each GUI application so that people have difficulty in using computers efficiently.  People still do much tedious and repetitive work in front of a computer.");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<ProofreadingPatch> patches = new List<ProofreadingPatch>();

            string[] onesToFind = {"GUI made using computers be more intuitive and easier to learn", "let people be able to control", "Masses only can use"};
            foreach (Word.Range r in canned_range.Sentences)
            {
                foreach(string oneToFind in onesToFind) {
                    if (r.Text.Contains(oneToFind)) {
                        object start = r.Text.IndexOf(oneToFind) + r.Start;
                        object end = (int) start + oneToFind.Length;
                        Word.Range newRange = Globals.Soylent.Application.ActiveDocument.Range(ref start, ref end);

                        List<string> replacements = new List<string>();
                        List<string> explanations = new List<string>();

                        if (oneToFind == onesToFind[0])
                        {
                            replacements.Add("GUI made using computers more intuitive and easier to learn");

                            explanations.Add("The word \"be\" is incorrectly inserted.");
                            explanations.Add("style issues");
                        }
                        else if (oneToFind == onesToFind[1])
                        {
                            replacements.Add("allow people to control");

                            explanations.Add("'Be able to' is unnecessary.");
                            explanations.Add("awkward construction");
                        }
                        else if (oneToFind == onesToFind[2])
                        {
                            replacements.Add("The masses can only");
                            replacements.Add("The majority of people only can");

                            explanations.Add("'Only can' should be switched around to 'can only'");
                            explanations.Add("Masses is not a proper name and should have \"The\" in front of it. Only in front of can is too choppy.");
                            explanations.Add("Bad word usage");
                        }

                        ProofreadingPatch patch = new ProofreadingPatch(newRange, replacements, explanations);
                        patches.Add(patch);
                    }
                }
            }

            ProofreadingData pd = new ProofreadingData(canned_range, Ribbon.generateJobNumber());
            pd.patches = patches;
            return pd;
        }
    }
}
