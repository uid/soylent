using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using VSTO = Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Soylent.View.HumanMacro;
using System.Xml.Serialization;

namespace Soylent.Model.HumanMacro
{
    public class HumanMacroData: HITData
    {
        public enum ReturnType {Comment, SmartTag};

        public List<TurKitSocKit.TurKitHumanMacroResult> messages = new List<TurKitSocKit.TurKitHumanMacroResult>();

        
        public enum Separator { Sentence, Paragraph };
        public Separator separator;
        [XmlIgnore] private Word.Range text;
        [XmlIgnore] private List<String> results;
        public List<HumanMacroPatch> patches = new List<HumanMacroPatch>();

        public int numberReturned;

        public enum TestOrReal { Test, Real };
        public TestOrReal test;
        public double reward;
        public int redundancy;
        public string title;
        public string subtitle;
        public string instructions;
        public ReturnType type;
        public string spacesBetweenSentences;

        public static string NAMESPACE = "http://uid.csail.mit.edu/soylent";

        [XmlIgnore] public StageData macroStageData;

        //public HumanMacroData(Word.Range text, List<String> results)
        public HumanMacroData(Word.Range toShorten, int job, Separator separator, double reward, int redundancy, string title, string subtitle, string instructions, ReturnType type, TestOrReal test) : base(toShorten, job)
        {
            this.text = toShorten;
            this.separator = separator;
            //this.results = results;
            //patches = new List<HumanMacroPatch>();

            this.reward = reward;
            this.redundancy = redundancy;
            this.title = title;
            this.subtitle = subtitle;
            this.instructions = instructions;
            this.type = type;
            this.numberReturned = 0;
            this.test = test;

            //stages[HITData.ResultType.Macro] = new StageData(HITData.ResultType.Macro);
            macroStageData = new StageData(HITData.ResultType.Macro, job);
            //stages[HITData.ResultType.Macro] = new HumanMacroStage(HITData.ResultType.Macro, redundancy);
        }

        public HumanMacroData() : base()
        {
            //patches = new List<HumanMacroPatch>();
            macroStageData = new StageData(HITData.ResultType.Macro, job);
        }

        new public void updateStatus(TurKitSocKit.TurKitStatus status)
        {
            StageData stage = macroStageData;//stages[HITData.ResultType.Macro];
            //stage.updateStage(status.numCompleted, status.paragraph);
            stage.numParagraphs = patches.Count;
            stage.updateStage(status);
            (view as HumanMacroView).updateView();
            cost = (view as HumanMacroView).cost;
            //System.Diagnostics.Debug.WriteLine("GOT A ************");
        }

        /// <summary>
        /// When this is loaded from a saved file and is finished, set the stage data accordingly.
        /// </summary>
        public void finishStageData()
        {
            int totalCompleted = numParagraphs * redundancy;
            double totalCost = totalCompleted * reward;

            this.macroStageData.setFinishedData(totalCompleted, totalCost);
            (view as HumanMacroView).updateView();
        }

        public void prepareRanges()
        {
            foreach (HumanMacroPatch patch in patches)
            {
                if (patch.range == null) { patch.range = Globals.Soylent.jobToDoc[this.job].Range(); }
                patch.range.SetRange(patch.rangeStart + this.range.Start, patch.rangeEnd + this.range.Start);
            }
        }

        public void patchesFound(string spaces)
        {
            spacesBetweenSentences = spaces;
            numParagraphs = patches.Count();
            macroStageData.FixParagraphNumber(numParagraphs);
        }

        public void postProcessSocKitMessage(TurKitSocKit.TurKitHumanMacroResult message)
        {
            prepareRanges();

            Patch patch = patches[message.input];
            
            if (patch.replacements.Count == 0)
            {
                foreach (string replacement in message.alternatives)
                {
                    if (this.separator == Separator.Sentence)
                    {
                        patch.replacements.Add(replacement + spacesBetweenSentences);
                    }
                    else
                    {
                        patch.replacements.Add(replacement);
                    }
                }
                numberReturned++;
            }

            if (numberReturned >= patches.Count)
            {
                if (this.tk.turkitLoopTimer != null)
                {
                    this.tk.turkitLoopTimer.Dispose();
                }
                this.jobDone = true;

                Globals.Soylent.soylentMap[Globals.Soylent.jobToDoc[this.job]].Invoke(new resultsBackDelegate(this.resultsBack), new object[] { });
            }
        }

        public void processSocKitMessage(TurKitSocKit.TurKitHumanMacroResult message)
        {
            messages.Add(message);
            postProcessSocKitMessage(message);
            /*
            Patch patch = patches[message.input];
            if (patch.replacements.Count == 0)
            {
                foreach (string replacement in message.alternatives)
                {
                    if (this.separator == Separator.Sentence)
                    {
                        patch.replacements.Add(replacement + spacesBetweenSentences);
                    }
                    else
                    {
                        patch.replacements.Add(replacement);
                    }
                }
                numberReturned++;
            }

            if (numberReturned == patches.Count)
            {
                this.tk.turkitLoopTimer.Dispose();
                Globals.Soylent.soylentMap[Globals.Soylent.jobToDoc[this.job]].Invoke(new resultsBackDelegate(this.resultsBack), new object[] { });
            }
            */
        }

        public void resultsBack()
        {
            HumanMacroView hview = view as HumanMacroView;
            hview.humanMacroDataReceived();
        }
        public delegate void resultsBackDelegate();

        public void AnnotateResult(ReturnType type)
        {
            if (type == ReturnType.SmartTag)
            {
                string tag = NAMESPACE + "#soylent" + DateTime.Now.Ticks;
                SmartTag resultTag = new SmartTag(tag, "Soylent Results: " + text.Text.Substring(0, 10) + "...");
                Regex pattern = new Regex(text.Text.Trim().Replace(" ", "\\s"), RegexOptions.IgnorePatternWhitespace);
                resultTag.Expressions.Add(pattern);

                List<VSTO.Action> actions = new List<Microsoft.Office.Tools.Word.Action>();
                foreach (string result in results)
                {
                    VSTO.Action action = new VSTO.Action(result);
                    action.Click += new ActionClickEventHandler(replaceText);
                    actions.Add(action);
                }
                resultTag.Actions = actions.ToArray();

                Globals.Soylent.VstoSmartTags.Add(resultTag);
            }
            else if (type == ReturnType.Comment)
            {
                foreach (string result in results)
                {
                    object commentText = result;
                    Globals.Soylent.jobToDoc[this.job].Comments.Add(text, ref commentText);
                    foreach (Microsoft.Office.Interop.Word.Comment c in Globals.Soylent.jobToDoc[this.job].Comments)
                    {
                        c.Author = "Turker";
                        c.Initial = "Turker";
                    }
                }
            }
        }

        public static void replaceText(object sender, ActionEventArgs e)
        {
            Object unit = Type.Missing;
            Object count = Type.Missing;
            e.Range.Delete(ref unit, ref count);
            String newText = ((VSTO.Action)sender).Caption;
            e.Range.InsertAfter(newText);
        }
    }
}
