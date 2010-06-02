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

namespace Soylent
{
    public class ShortenData: HITData
    {
        public List<Patch> patches;
        public int paragraphsCompleted = 0;
        private ShortnView shortnview;

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

        public ShortenData(Word.Range toShorten, int job) : base(toShorten, job)
        {
            stages[ResultType.Find] = new StageData(ResultType.Find, numParagraphs);
            stages[ResultType.Fix] = new StageData(ResultType.Fix, numParagraphs);
            stages[ResultType.Verify] = new StageData(ResultType.Verify, numParagraphs);

            typeMap = new Dictionary<string,ResultType>();
            typeMap["find"] = ResultType.Find;
            typeMap["fix"] = ResultType.Fix;
            typeMap["verify"] = ResultType.Verify;

            patches = new List<Patch>();
        }

        public override void register(HITView hview)
        {
            shortnview = hview as ShortnView;
        }

        new public void updateStatus(TurKitSocKit.TurKitStatus status)
        {
            string stringtype = status.stage;
            System.Diagnostics.Debug.WriteLine(stringtype);
            //System.Diagnostics.Debug.WriteLine("^^^^^ stringtype ^^^^^^");
            ResultType type = typeMap[stringtype];
            StageData stage = stages[type];
            //stage.updateStage(status.numCompleted, status.paragraph);
            stage.updateStage(status);
            //System.Diagnostics.Debug.WriteLine("GOT A ************");
        }

        public void processSocKitMessage(TurKitSocKit.TurKitShortn message)
        {
            Word.Range curParagraphRange = range.Paragraphs[message.paragraph + 1].Range;
            int nextStart = 0;
            int nextEnd;
            foreach (TurKitSocKit.TurKitShortnPatch tkspatch in message.patches)
            {
                if (tkspatch.editStart > nextStart)
                {
                    nextEnd = tkspatch.editStart;
                    Word.Range dummyRange = Globals.Soylent.Application.ActiveDocument.Range(curParagraphRange.Start + nextStart, curParagraphRange.Start + nextEnd);
                    DummyPatch dummyPatch = new DummyPatch(dummyRange);

                    patches.Add(dummyPatch);
                }

                int start = curParagraphRange.Start + tkspatch.editStart;
                int end = curParagraphRange.Start + tkspatch.editEnd;
                //TODO: How do I make a range?
                //Word.Range patchRange = null;//Globals.Soylent.
                Word.Range patchRange = Globals.Soylent.Application.ActiveDocument.Range(start,end);

                Patch thisPatch = new Patch(patchRange, (from option in tkspatch.options select option.editedText).ToList());
                // add the original as an option
                thisPatch.replacements.Add(tkspatch.originalText);
                // if it's cuttable, add the empty string as an option
                if (tkspatch.canCut)
                {
                    thisPatch.replacements.Add("");  // means "cuttable" for now
                }

                patches.Add(thisPatch);
                nextStart = tkspatch.end;
            }
            if (nextStart < (curParagraphRange.Text.Length - 1)){
                nextEnd = curParagraphRange.Text.Length;
                Word.Range dummyRange = Globals.Soylent.Application.ActiveDocument.Range(curParagraphRange.Start + nextStart, curParagraphRange.End);
                DummyPatch dummyPatch = new DummyPatch(dummyRange);

                patches.Add(dummyPatch);
            }
            paragraphsCompleted++;

            if (paragraphsCompleted == numParagraphs)
            {
                //TODO: use a delegate.
                this.tk.turkitLoopTimer.Dispose();
                returnShortnResults();
            }
        }

      

        public void returnShortnResults()
        {
            Globals.Soylent.soylent.Invoke(new popupShortnWindowDelegate(this.popupShortnWindow), new object[] { });
        }

        public delegate void popupShortnWindowDelegate();
        public void popupShortnWindow()
        {
            shortnview.shortenDataReceived();
        }

        Dictionary<int, List<PatchSelection>> cachedSelections = new Dictionary<int, List<PatchSelection>>();
        public List<PatchSelection> getPatchSelections(int desiredLength)
        {
            if (cachedSelections.Keys.Count == 0)
            {
                initializeSelections();
            }

            IEnumerable<int> lengthList = cachedSelections.Keys.OrderByDescending(len => len);
            if (desiredLength > lengthList.ElementAt(0))
            {
                return cachedSelections[lengthList.ElementAt(0)];
            }

            for (int i = 0; i < lengthList.Count(); i++ )
            {
                if (lengthList.ElementAt(i) < desiredLength)
                {
                    return cachedSelections[lengthList.ElementAt(i-1)];
                }
            }
            //return (from patch in patches select new PatchSelection(patch, patch.replacements[0])).ToList();
            // return the smallest one we got
            return cachedSelections[lengthList.ElementAt(lengthList.Count()-1)];
        }

        public List<int> possibleLengths()
        {
            return (from entry in cachedSelections orderby entry.Key ascending select entry.Key).ToList();
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

        private List<List<PatchSelection>> recursiveInitialization(List<Patch> patches)
        {
            List<string> string_options = patches[0].replacements;
            List<PatchSelection> options = (from option in string_options select new PatchSelection(patches[0], option)).ToList();

            List<List<PatchSelection>> results = new List<List<PatchSelection>>();
            if (patches.Count == 1)
            {
                results.AddRange((from option in options select new List<PatchSelection> { option }).ToList() );
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


        public static ShortenData getCannedData()
        {
            // insert text
            Globals.Soylent.Application.Selection.Range.InsertAfter("Automatic clustering generally helps separate different kinds of records that need to be edited differently, but it isn't perfect. Sometimes it creates more clusters than needed, because the differences in structure aren't important to the user's particular editing task.  For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant, and it isn't necessary to split based on those differences.  Conversely, sometimes the clustering isn't fine enough, leaving heterogeneous clusters that must be edited one line at a time.  One solution to this problem would be to let the user rearrange the clustering manually, perhaps using drag-and-drop to merge and split clusters.  Clustering and selection generalization would also be improved by recognizing common text structure like URLs, filenames, email addresses, dates, times, etc.");
            // select it
            Word.Range canned_range = Globals.Soylent.Application.ActiveDocument.Paragraphs[1].Range;

            List<Patch> canned_patches = new List<Patch>();
            foreach (Word.Range r in canned_range.Sentences)
            {
                List<string> options = new List<string>();
                options.Add(r.Text);
                
                if (r.Text == "Sometimes it creates more clusters than needed, because the differences in structure aren't important to the user's particular editing task.  ")
                {
                    options.Add("Sometimes it creates more clusters than needed, because the differences in structure aren't relevant to a specific task.  ");
                    options.Add("Sometimes it creates more clusters than needed, as structure differences aren't important to the editing task.  ");
                    options.Add("Sometimes it creates more clusters than needed, because the structural differences aren't important to the user's editing task.  ");
                }
                else if (r.Text == "For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant, and it isn't necessary to split based on those differences.  ")
                {
                    options.Add("For example, if the user only needs to edit near the end of each line, then differences at the start of the line are largely irrelevant.  ");
                    options.Add("|  ");
                }
                else if (r.Text == "One solution to this problem would be to let the user rearrange the clustering manually, perhaps using drag-and-drop to merge and split clusters.  ")
                {
                    options.Add("One solution to this problem would be to let the user rearrange the clustering manually.  ");
                    options.Add("One solution to this problem would be to let the user rearrange the clustering manually.  ");
                    options.Add("One solution to this problem would be to let the user rearrange the clustering manually using drag-and-drop edits.  ");
                    options.Add("The user could solve this problem by merging and splitting clusters manually.  ");
                    options.Add("|  ");
                }
                

                canned_patches.Add(new Patch(r, options));
            }

            ShortenData sd = new ShortenData(canned_range, Ribbon.generateJobNumber());
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
