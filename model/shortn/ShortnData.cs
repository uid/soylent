using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Diagnostics;
using System.Xml.Serialization;

using Soylent.View.Shortn;
using Soylent.View;

namespace Soylent.Model.Shortn
{
    /// <summary>
    /// A subclass of HITData that is the Model for a specific Shortn job
    /// </summary>
    public class ShortnData: HITData
    {
        //[System.Xml.Serialization.XmlArray("patches")]
        //[System.Xml.Serialization.XmlArrayItem("patch", typeof(List<Patch>))]
        
        [XmlIgnore] public List<ShortnPatch> patches;
        [XmlIgnore] public int paragraphsCompleted = 0;
        [XmlIgnore] private Dictionary<ResultType, List<List<bool>>> gottenOneYet;
        [XmlIgnore] public StageData findStageData;
        [XmlIgnore] public StageData fixStageData;
        [XmlIgnore] public StageData verifyStageData;
        [XmlIgnore] Dictionary<int, List<PatchSelection>> cachedSelections = new Dictionary<int, List<PatchSelection>>();

        public int shortestLength
        {
            get
            {
                if (_shortestLength == -1)
                {
                    // LINQ
                    _shortestLength = (from patch in getPatchSelections(0) select patch.selection.Length).Sum();
                }
                return _shortestLength;
            }
        }
        private int _shortestLength = -1;    // for caching

        public int longestLength
        {
            get
            {
                if (_longestLength == -1)
                {
                    _longestLength = (from patch in getPatchSelections(int.MaxValue) select patch.selection.Length).Sum();
                }
                return _longestLength;
            }
        }

        private int _longestLength = -1;    // for caching

        /// <summary>
        /// A subclass of HITData that is the Model for a specific Shortn job
        /// </summary>
        /// <param name="toShorten">The Range object selected for this task</param>
        /// <param name="job">The unique job number for this task</param>
        public ShortnData(Word.Range toShorten, int job) : base(toShorten, job)
        {
            
            findStageData = new StageData(ResultType.Find, numParagraphs, job);
            stages.Add(findStageData);
            fixStageData = new StageData(ResultType.Fix, numParagraphs, job);
            stages.Add(fixStageData);
            verifyStageData = new StageData(ResultType.Verify, numParagraphs, job);
            stages.Add(verifyStageData);
            
            /*
            typeMap = new Dictionary<string,ResultType>();
            typeMap["find"] = ResultType.Find;
            typeMap["fix"] = ResultType.Fix;
            typeMap["verify"] = ResultType.Verify;
            */


            patches = new List<ShortnPatch>();
            gottenOneYet = new Dictionary<ResultType, List<List<bool>>>();
        
        }
        
        internal ShortnData()
            : base()
        {
            findStageData = new StageData(ResultType.Find, numParagraphs, job);
            stages.Add(findStageData);
            fixStageData = new StageData(ResultType.Fix, numParagraphs, job);
            stages.Add(fixStageData);
            verifyStageData = new StageData(ResultType.Verify, numParagraphs, job);
            stages.Add(verifyStageData);

            patches = new List<ShortnPatch>();
            gottenOneYet = new Dictionary<ResultType, List<List<bool>>>();
        }
       

        /// <summary>
        /// Updates the model for a given status update
        /// </summary>
        /// <param name="status">The status update delivered by TurKit</param>
        public override void updateStatus(TurKitSocKit.TurKitStatus status)
        {
            string stringtype = status.stage;
            System.Diagnostics.Debug.WriteLine(stringtype);
            //System.Diagnostics.Debug.WriteLine("^^^^^ stringtype ^^^^^^");
            /*
            ResultType type = ResultType.Find;// = typeMap[stringtype];
            if (stringtype == "find"){type = ResultType.Find;}
            else if (stringtype == "fix") { type = ResultType.Fix; }
            else if (stringtype == "verify") { type = ResultType.Verify; }
            */
            StageData stage = null;//stages[type];
            if (stringtype == "find") { stage = findStageData; }
            else if (stringtype == "fix") { stage = fixStageData; }
            else if (stringtype == "verify") { stage = verifyStageData; }
            //stage.updateStage(status.numCompleted, status.paragraph);
              
            stage.updateStage(status);
            (view as ShortnView).updateView();
            cost = (view as ShortnView).cost;
            //System.Diagnostics.Debug.WriteLine("GOT A ************");
            
        }

        /// <summary>
        /// Process a StageComplete message from TurKit
        /// </summary>
        /// <param name="donezo"></param>
        public void stageCompleted(TurKitSocKit.TurKitStageComplete donezo){
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
            (view as ShortnView).updateView();
        }

        public void postProcessSocKitMessage(TurKitSocKit.TurKitFindFixVerify message)
        {
            Word.Range curParagraphRange = range.Paragraphs[message.paragraph + 1].Range;
            int nextStart = 0; //Is always the location where the next patch (dummy or otherwise) should start.
            int nextEnd; //Is where the last patch ended.  Kinda poorly named. Tells us if we need to add a dummy patch after the last real patch

            Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[this.job];

            foreach (TurKitSocKit.TurKitFindFixVerifyPatch tkspatch in message.patches)
            {


                //For text in between patches, we create dummy patches.
                if (tkspatch.editStart > nextStart)
                {
                    nextEnd = tkspatch.editStart;
                    int dummyStart = curParagraphRange.Start + nextStart;
                    int dummyEnd = curParagraphRange.Start + nextEnd;
                    Word.Range dummyRange = doc.Range(dummyStart, dummyEnd);

                    DummyPatch dummyPatch = new DummyPatch(dummyRange);
                    Debug.WriteLine("dummy patch: " + dummyRange.Text);

                    patches.Add(dummyPatch);
                }

                int start = curParagraphRange.Start + tkspatch.editStart;
                int end = curParagraphRange.Start + tkspatch.editEnd;
                Word.Range patchRange = doc.Range(start, end);
                Debug.WriteLine("new patch: " + patchRange.Text);

                List<string> alternatives = new List<string>();
                foreach (TurKitSocKit.TurKitFindFixVerifyOption option in (from option in tkspatch.options where option.editsText select option))
                {
                    alternatives.AddRange(from alternative in option.alternatives select alternative.editedText);
                }

                ShortnPatch thisPatch = new ShortnPatch(patchRange, alternatives);
                // add the original as an option
                thisPatch.replacements.Add(tkspatch.originalText);

                patches.Add(thisPatch);
                nextStart = tkspatch.editEnd;
            }


            //If the last patch we found isn't the end of the paragraph, create a DummyPatch
            if (nextStart < (curParagraphRange.Text.Length - 1))
            {
                nextEnd = curParagraphRange.Text.Length;
                Word.Range dummyRange = doc.Range(curParagraphRange.Start + nextStart, curParagraphRange.End);
                Debug.WriteLine("dummy patch: " + dummyRange.Text);

                DummyPatch dummyPatch = new DummyPatch(dummyRange);

                patches.Add(dummyPatch);
            }
            paragraphsCompleted++;

            if (paragraphsCompleted == numParagraphs) //If we have done all paragraphs, make them available to the user!
            {
                //TODO: use a delegate.
                if (this.tk.turkitLoopTimer != null)
                {
                    this.tk.turkitLoopTimer.Dispose();
                }

                this.jobDone = true;

                /*
                foreach (Patch patch in patches)
                {
                    Debug.WriteLine(patch.range.Start + " - " + patch.range.End + " : " + patch.range.Text + " || " + (patch is DummyPatch));
                }
                */

                returnShortnResults();
            }
        }

        /// <summary>
        /// Processes a shortn message, one that contains the final results of the algorithm. One per paragraph
        /// </summary>
        /// <param name="message"></param>
        public void processSocKitMessage(TurKitSocKit.TurKitFindFixVerify message)
        {
            findFixVerifies.Add(message);
            postProcessSocKitMessage(message);
        }

        private void returnShortnResults()
        {
            //popupShortnWindow();
            popupShortnWindowDelegate del = new popupShortnWindowDelegate(this.popupShortnWindow);
            Globals.Soylent.soylentMap[Globals.Soylent.jobToDoc[this.job]].Invoke(del, null);
        }

        public delegate void popupShortnWindowDelegate();
        /// <summary>
        /// Pops up the Shortn dialog window
        /// </summary>
        public void popupShortnWindow()
        {
            (view as ShortnView).shortenDataReceived();
        }

        
        
        /// <summary>
        /// Make the desired changes in the document.
        /// </summary>
        /// <param name="desiredLength">The desired character length for the region.  Selections will be retrieved and reflected in the document</param>
        public void makeChangesInDocument(int desiredLength)
        {
            /* This is the way we have to make changes in the document when the slider moves.
            * What we should be able to do: Range.Text = PatchSelection.selection
            * What we have to do instead:
            * Collapse the patch's range to the beginning of the range
            * Add the new text after this 0-length range.  The range now extends to contain this new text.
            * Collapse the range to it's end.  This puts a 0-length range between the new text and the old text to be deleted.
            * Delete the next n characters, where n is the length of the old text.  i.e. delete the old text. 
            * Move the starting point of the range to the beginning of the new text. Now the range reflects the location of the new text.
            * 
            * Why do we have to do this? Ranges behave weirdly when you add/remove text at the point where ranges touch.
            * By doing this convoluted process, we ensure that we never add/remove text in a way that causes ranges to overlap.
            */
            List<PatchSelection> pSelections = getPatchSelections(desiredLength);          
            for (int i = 0; i < pSelections.Count; i++)
            {
                PatchSelection selection1 = pSelections[i];
                if (selection1.isCurrent) { continue; }
                int originalLength = selection1.patch.range.End - selection1.patch.range.Start;
                string newString = selection1.selection;
                int newLength = newString.Length;

                /*
                if (newLength == 0)
                {
                    newString = " ";
                    newLength = 1;
                }
                */

                selection1.patch.range.Collapse(); //Collapse to the beginning
                selection1.patch.range.InsertAfter(newString); //Insert the new text
                int newStart = selection1.patch.range.Start;
                int newEnd = selection1.patch.range.End;
                selection1.patch.range.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd); //Collapse to the end
                selection1.patch.range.Delete(Microsoft.Office.Interop.Word.WdUnits.wdCharacter, originalLength); //Delete old text
                selection1.patch.range.SetRange(newStart, newEnd); // Fix the range

                if (originalLength == 0)
                {
                    fixFutureRangeStarts(pSelections, i, newStart);
                }
            }
        }

        public static void fixFutureRangeStarts(List<PatchSelection> pSelections, int i, int start)
        {
            PatchSelection first = pSelections[i];
            if (i + 1 < pSelections.Count)
            {
                PatchSelection next = pSelections[i + 1];
                if (next.patch.range.Start == start)
                {
                    next.patch.range.Start = first.patch.range.End;
                    fixFutureRangeStarts(pSelections, i + 1, start);
                }
                else
                {
                    return;
                }
            }
        }

        //Dictionary<int, List<PatchSelection>> cachedSelections = new Dictionary<int, List<PatchSelection>>();
        /// <summary>
        /// Gets the patch selections for this job
        /// </summary>
        /// <param name="desiredLength">The desired character length for the returned list of selections</param>
        /// <returns></returns>
        public List<PatchSelection> getPatchSelections(int desiredLength)
        {
            cachedSelections = new Dictionary<int, List<PatchSelection>>();

            if (cachedSelections.Keys.Count == 0)
            {
                initializeSelections();
            }

            IEnumerable<int> lengthList = cachedSelections.Keys.OrderByDescending(len => len);
            if (desiredLength > lengthList.ElementAt(0))
            {
                return cachedSelections[lengthList.ElementAt(0)];
            }

            for (int i = 0; i < lengthList.Count(); i++)
            {
                if (lengthList.ElementAt(i) < desiredLength)
                {
                    return cachedSelections[lengthList.ElementAt(i - 1)];
                }
            }
            //return (from patch in patches select new PatchSelection(patch, patch.replacements[0])).ToList();
            // return the smallest one we got
            return cachedSelections[lengthList.ElementAt(lengthList.Count() - 1)];
        }

        /// <summary>
        /// Returns a list of possible lengths, given the different permutations of selection choices.
        /// </summary>
        /// <returns></returns>
        public List<int> possibleLengths()
        {
            return possibleLengths(true);
            //return (from entry in cachedSelections orderby entry.Key ascending select entry.Key).ToList();
        }

        public List<int> possibleLengths(bool useCache)
        {
            if (useCache == true)
            {
                return (from entry in cachedSelections orderby entry.Key ascending select entry.Key).ToList();
            }
            else
            {
                cachedSelections = new Dictionary<int, List<PatchSelection>>();
                initializeSelections();
                return (from entry in cachedSelections orderby entry.Key ascending select entry.Key).ToList();
            }
        }

        private void initializeSelections()
        {
            // Exponential for now
            List<List<PatchSelection>> allOptions = recursiveInitialization(this.patches);
            foreach (List<PatchSelection> choices in allOptions)
            {
                // count length
                int len = choices.Sum(choice => choice.selection.Length);
                cachedSelections[len] = choices;
            }
        }

        private List<List<PatchSelection>> recursiveInitialization(List<ShortnPatch> patches)
        {
            /*
            List<string> string_options = patches[0].replacements;
            List<PatchSelection> options = (from option in string_options select new PatchSelection(patches[0], option)).ToList();

            List<List<PatchSelection>> results = new List<List<PatchSelection>>();
            if (patches.Count == 1)
            {
                results.AddRange((from option in options select new List<PatchSelection> { option }).ToList() );
                return results;
            }
            */

            List<string> string_options;
            if (patches[0].isLocked)
            {
                string_options = new List<string>();
                string_options.Add(patches[0].lockedReplacement);
            }
            else
            {
                string_options = patches[0].replacements;
            }
            List<PatchSelection> options = (from option in string_options select new PatchSelection(patches[0], option)).ToList();

            List<List<PatchSelection>> results = new List<List<PatchSelection>>();
            if (patches.Count == 1)
            {
                results.AddRange((from option in options select new List<PatchSelection> { option }).ToList());
                return results;
            }

            // iterate over the first set in the list
            // get all possible combinations of all other lists
            List<List<PatchSelection>> allCombos = recursiveInitialization(patches.Skip(1).ToList());
            
            foreach (PatchSelection option in options)
            {
                foreach (List<PatchSelection> selections in allCombos)
                {
                    List<PatchSelection> clone = new List<PatchSelection>(selections);
                    // add your one to each of them
                    clone.Insert(0, option);
                    results.Add(clone);
                }
            }
            return results;
        }

        /// <summary>
        /// Gets canned data for testing purposes
        /// </summary>
        /// <returns></returns>
        public static ShortnData getCannedData()
        {
            // insert text
            //Globals.Soylent.Application.Selection.Range.InsertAfter("Automatic clustering generally helps separate different kinds of records that need to be edited differently, but it isn't perfect. Sometimes it creates more clusters than needed, because the differences in structure aren't important to the user's particular editing task.  For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant, and it isn't necessary to split based on those differences.  Conversely, sometimes the clustering isn't fine enough, leaving heterogeneous clusters that must be edited one line at a time.  One solution to this problem would be to let the user rearrange the clustering manually, perhaps using drag-and-drop to merge and split clusters.  Clustering and selection generalization would also be improved by recognizing common text structure like URLs, filenames, email addresses, dates, times, etc.  ");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;


            List<ShortnPatch> canned_patches = new List<ShortnPatch>();
            foreach (Word.Range r in canned_range.Sentences)
            {
                List<string> options = new List<string>();
                //options.Add(r.Text);

                string textToMatch = null;
                bool found = false;
                
                if (r.Text.Contains(textToMatch = "because the differences in structure aren't important to the user's particular editing task"))
                {
                    found = true;
                    options.Add("because the differences in structure aren't relevant to a specific task");
                    options.Add("as structure differences aren't important to the editing task");
                    options.Add("because the structural differences aren't important to the user's editing task");
                }
                else if (r.Text.Contains(textToMatch = "For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant, and it isn't necessary to split based on those differences.  "))
                {
                    found = true;
                    options.Add("For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant.  ");
                    options.Add("");
                }
                else if (r.Text.Contains(textToMatch = "One solution to this problem would be to let the user rearrange the clustering manually, perhaps using drag-and-drop to merge and split clusters.  "))
                {
                    found = true;
                    options.Add("One solution to this problem would be to let the user rearrange the clustering manually.  ");
                    options.Add("One solution to this problem would be to let the user rearrange the clustering manually using drag-and-drop edits.  ");
                    options.Add("The user could solve this problem by merging and splitting clusters manually.  ");
                    options.Add("");
                }

                if (found)
                {
                    int start = r.Start + r.Text.IndexOf(textToMatch);
                    int end = start + textToMatch.Length;

                    if (start - r.Start > 0)
                    {
                        Word.Range dummy1 = Globals.Soylent.Application.ActiveDocument.Range(r.Start, start);
                        canned_patches.Add(new DummyPatch(dummy1));
                    }

                    Word.Range newRange = Globals.Soylent.Application.ActiveDocument.Range(start, end);
                    options.Add(newRange.Text);
                    canned_patches.Add(new ShortnPatch(newRange, options));

                    if (r.End - end > 0)
                    {
                        Word.Range dummy2 = Globals.Soylent.Application.ActiveDocument.Range(end, r.End);
                        canned_patches.Add(new DummyPatch(dummy2));
                    }
                }
                else
                {
                    canned_patches.Add(new DummyPatch(r));
                }
            }

            ShortnData sd = new ShortnData(canned_range, Globals.Soylent.jobManager.generateJobNumber());
            sd.patches = canned_patches;
            return sd;
        }

        /*
        public static ShortenData socketData(TurKitSocKit.TurKitShorten shorten)
        {
            Globals.Soylent.Application.Selection.Range.InsertAfter(String.Join("  ", shorten.paragraph));
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<Patch> patches = new List<Patch>();
            //foreach(
        }
        */
    }
}
