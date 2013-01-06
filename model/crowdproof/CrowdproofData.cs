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
using System.Xml.Serialization;

using Soylent.Model.HumanMacro;
using Soylent.View;
using Soylent.View.Crowdproof;

namespace Soylent.Model.Crowdproof
{
    public class CrowdproofData : HITData
    {
        [XmlIgnore] int paragraphsCompleted = 0;
        [XmlIgnore] public List<CrowdproofPatch> patches { get; set;}
        [XmlIgnore] public StageData findStageData;
        [XmlIgnore] public StageData fixStageData;
        [XmlIgnore] public StageData verifyStageData;

        public CrowdproofData(Word.Range range, int job)
            : base(range, job)
        {
            findStageData = new StageData(ResultType.Find, numParagraphs, job);
            stages.Add(findStageData);
            fixStageData = new StageData(ResultType.Fix, numParagraphs, job);
            stages.Add(fixStageData);
            verifyStageData = new StageData(ResultType.Verify, numParagraphs, job);
            stages.Add(verifyStageData);

            /*
            stages[ResultType.Find] = new StageData(ResultType.Find, numParagraphs);
            stages[ResultType.Fix] = new StageData(ResultType.Fix, numParagraphs);
            stages[ResultType.Verify] = new StageData(ResultType.Verify, numParagraphs);
            */
            patches = new List<CrowdproofPatch>();
            /*
            typeMap = new Dictionary<string, ResultType>();
            typeMap["find"] = ResultType.Find;
            typeMap["fix"] = ResultType.Fix;
            typeMap["verify"] = ResultType.Verify;
             */
        }

        public CrowdproofData()
            : base()
        {
            findStageData = new StageData(ResultType.Find, numParagraphs, job);
            stages.Add(findStageData);
            fixStageData = new StageData(ResultType.Fix, numParagraphs, job);
            stages.Add(fixStageData);
            verifyStageData = new StageData(ResultType.Verify, numParagraphs, job);
            stages.Add(verifyStageData);

            /*
            stages[ResultType.Find] = new StageData(ResultType.Find, numParagraphs);
            stages[ResultType.Fix] = new StageData(ResultType.Fix, numParagraphs);
            stages[ResultType.Verify] = new StageData(ResultType.Verify, numParagraphs);
            */
            patches = new List<CrowdproofPatch>();
            /*
            typeMap = new Dictionary<string, ResultType>();
            typeMap["find"] = ResultType.Find;
            typeMap["fix"] = ResultType.Fix;
            typeMap["verify"] = ResultType.Verify;
             */
        }

        public override void updateStatus(TurKitSocKit.TurKitStatus status)
        {
            string stringtype = status.stage;
            System.Diagnostics.Debug.WriteLine(stringtype);
            //System.Diagnostics.Debug.WriteLine("^^^^^ stringtype ^^^^^^");
            /*
            ResultType type = typeMap[stringtype];
            StageData stage = stages[type];
            */
            StageData stage = null;//stages[type];
            if (stringtype == "find") { stage = findStageData; }
            else if (stringtype == "fix") { stage = fixStageData; }
            else if (stringtype == "verify") { stage = verifyStageData; }
            //stage.updateStage(status.numCompleted, status.paragraph);

            stage.updateStage(status);
            (view as CrowdproofView).updateView();
            cost = (view as CrowdproofView).cost;
            //System.Diagnostics.Debug.WriteLine("GOT A ************");
        }

        public void stageCompleted(TurKitSocKit.TurKitStageComplete donezo)
        {
            stageCompletes.Add(donezo);

            StageData stage = null;
            if (donezo.stage == "find") { stage = findStageData; }
            else if (donezo.stage == "fix") { stage = fixStageData; }
            else if (donezo.stage == "verify") { stage = verifyStageData; }

            stage.terminatePatch(donezo.paragraph, donezo.patchNumber);
        }
        public void terminateStage(TurKitSocKit.TurKitStageComplete donezo)
        {
            StageData stage = null;
            if (donezo.stage == "find") { stage = findStageData; }
            else if (donezo.stage == "fix") { stage = fixStageData; }
            else if (donezo.stage == "verify") { stage = verifyStageData; }

            stage.terminateStage(donezo);
            (view as CrowdproofView).updateView();
        }

        public void AnnotateResult()
        {

            Word.Range text = range;
            foreach (CrowdproofPatch pp in patches)
            {
                string tag = HumanMacroData.NAMESPACE + "#soylent" + DateTime.Now.Ticks;
                SmartTag resultTag = Globals.Factory.CreateSmartTag(tag, pp.reasons[0]);
                Regex pattern = new Regex(pp.range.Text.Trim().Replace(" ", "\\s"), RegexOptions.IgnorePatternWhitespace);
                resultTag.Expressions.Add(pattern);

                List<VSTO.Action> actions = new List<Microsoft.Office.Tools.Word.Action>();
                foreach (string result in pp.replacements)
                {
                    VSTO.Action action = Globals.Factory.CreateAction(result);
                    action.Click += new ActionClickEventHandler(HumanMacroData.replaceText);
                    actions.Add(action);
                }

                /* this doesn't work in Word 2010 */
                if (WordVersion.currentVersion < WordVersion.OFFICE_2010)
                {
                    foreach (string reason in pp.reasons)
                    {
                        VSTO.Action action = Globals.Factory.CreateAction("Error Descriptions///" + reason);
                        action.Click += new ActionClickEventHandler(HumanMacroData.replaceText);
                        actions.Add(action);
                    }
                }

                resultTag.Actions = actions.ToArray();
                Globals.Soylent.VstoSmartTags.Add(resultTag);

                if (WordVersion.currentVersion >= WordVersion.OFFICE_2010)
                {
                    pp.range.Underline = Word.WdUnderline.wdUnderlineWavy;
                    pp.range.Font.UnderlineColor = Word.WdColor.wdColorLavender;
                }
            }
        }

        public void postProcessSocKitMessage(TurKitSocKit.TurKitFindFixVerify message)
        {

            Word.Range curParagraphRange = range.Paragraphs[message.paragraph + 1].Range;
            Word.Document doc = Globals.Soylent.jobToDoc[job];

            foreach (TurKitSocKit.TurKitFindFixVerifyPatch tkspatch in message.patches)
            {

                int start = curParagraphRange.Start + tkspatch.editStart;
                int end = curParagraphRange.Start + tkspatch.editEnd;
                //Word.Range patchRange = Globals.Soylent.Application.ActiveDocument.Range(start, end); //New range for this patch, yay!
                Word.Range patchRange = doc.Range(start, end);

                List<string> alternatives = new List<string>();
                foreach (TurKitSocKit.TurKitFindFixVerifyOption option in (from option in tkspatch.options where option.field == "revision" select option))
                {
                    alternatives.AddRange(from alternative in option.alternatives select alternative.editedText);
                }

                List<string> reasons = new List<string>();
                foreach (TurKitSocKit.TurKitFindFixVerifyOption option in (from option in tkspatch.options where option.field == "reason" select option))
                {
                    reasons.AddRange(from alternative in option.alternatives select alternative.text);
                }



                CrowdproofPatch thisPatch = new CrowdproofPatch(patchRange, alternatives, reasons);
                // add the original as an option
                //thisPatch.replacements.Add(tkspatch.originalText);

                patches.Add(thisPatch);
            }

            paragraphsCompleted++;

            if (paragraphsCompleted == numParagraphs) //If we have done all paragraphs, make them available to the user!
            {
                this.jobDone = true;

                if (this.tk.turkitLoopTimer != null)
                {
                    this.tk.turkitLoopTimer.Dispose();
                }

                //TODO: use a delegate.
                //Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].Invoke(new crowdproofDelegate(this.crowdproofDataReceived), new object[] { });
                Globals.Soylent.soylentMap[doc].Invoke(new crowdproofDelegate(this.crowdproofDataReceived), new object[] { });

                //CrowdproofView view = this.view as CrowdproofView;
                //view.crowdproofDataReceived();
                //this.AnnotateResult();
            }

        }

        public void processSocKitMessage(TurKitSocKit.TurKitFindFixVerify message)
        {
            findFixVerifies.Add(message);
            postProcessSocKitMessage(message);
            /*
            Word.Range curParagraphRange = range.Paragraphs[message.paragraph + 1].Range;
            Word.Document doc = Globals.Soylent.jobToDoc[job];

            foreach (TurKitSocKit.TurKitFindFixVerifyPatch tkspatch in message.patches)
            {

                int start = curParagraphRange.Start + tkspatch.editStart;
                int end = curParagraphRange.Start + tkspatch.editEnd;
                //Word.Range patchRange = Globals.Soylent.Application.ActiveDocument.Range(start, end); //New range for this patch, yay!
                Word.Range patchRange = doc.Range(start, end);

                List<string> alternatives = new List<string>();
                foreach (TurKitSocKit.TurKitFindFixVerifyOption option in (from option in tkspatch.options where option.field == "revision" select option))
                {
                    alternatives.AddRange(from alternative in option.alternatives select alternative.editedText);
                }

                List<string> reasons = new List<string>();
                foreach (TurKitSocKit.TurKitFindFixVerifyOption option in (from option in tkspatch.options where option.field == "reason" select option))
                {
                    reasons.AddRange(from alternative in option.alternatives select alternative.text);
                }



                CrowdproofPatch thisPatch = new CrowdproofPatch(patchRange, alternatives, reasons);
                // add the original as an option
                //thisPatch.replacements.Add(tkspatch.originalText);

                patches.Add(thisPatch);
            }

            paragraphsCompleted++;

            if (paragraphsCompleted == numParagraphs) //If we have done all paragraphs, make them available to the user!
            {
                //TODO: use a delegate.
                //Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].Invoke(new crowdproofDelegate(this.crowdproofDataReceived), new object[] { });
                Globals.Soylent.soylentMap[doc].Invoke(new crowdproofDelegate(this.crowdproofDataReceived), new object[] { });
                
                this.tk.turkitLoopTimer.Dispose();
                //CrowdproofView view = this.view as CrowdproofView;
                //view.crowdproofDataReceived();
                //this.AnnotateResult();
            }
            */
        }

        public void crowdproofDataReceived()
        {
            CrowdproofView view = this.view as CrowdproofView;
            view.crowdproofDataReceived();
        }
        public delegate void crowdproofDelegate();

        public static CrowdproofData getCannedData()
        {
            // insert text
            Globals.Soylent.Application.Selection.Range.InsertAfter("However, while GUIs made using computers be more intuitive and easier to learn, it didn't let people be able to control computers efficiently.  Masses only can use the software developed by software companies, unless they know how to write programs.  In other words, if one who knows nothing about programming needs to click through 100 buttons to complete her job everyday, the only thing she can do is simply to click through those buttons by hand every time.  But if she happens to be a computer programmer, there is a little chance that she can write a program to automate everything.  Why is there only a little chance?  In fact, each GUI application is a big black box, which usually have no outward interfaces for connecting to other programs.  In other words, this truth builds a great wall between each GUI application so that people have difficulty in using computers efficiently.  People still do much tedious and repetitive work in front of a computer.");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<CrowdproofPatch> patches = new List<CrowdproofPatch>();

            string[] onesToFind = {"GUIs made using computers be more intuitive and easier to learn", "let people be able to control", "Masses only can"};
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
                            replacements.Add("GUIs made using computers more intuitive and easier to learn");

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

                        CrowdproofPatch patch = new CrowdproofPatch(newRange, replacements, explanations);
                        patches.Add(patch);
                    }
                }
            }

            CrowdproofData pd = new CrowdproofData(canned_range, Globals.Soylent.jobManager.generateJobNumber());
            pd.patches = patches;
            return pd;
        }
    }
}
