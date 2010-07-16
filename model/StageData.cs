using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using Soylent.View;

namespace Soylent.Model
{
    /// <summary>
    /// Represents the data from a specific stage in a job.  For example: the Find, Fix, or Verify stage.
    /// </summary>
    public class StageData
    {
        [XmlIgnore] private StageView listener;
        public HITData.ResultType type;
        public int numCompleted {
            get
            {
                return (from numCompletedThisParagraph in numCompletedperParagraph select numCompletedThisParagraph.Sum()).Sum();
            }
        }
        public int numRequested
        {
            get
            {
                return (from totalRequestedThisParagraph in totalRequested select totalRequestedThisParagraph.Sum()).Sum();
            }
        }

        public int numParagraphs;
        private List<List<int>> numCompletedperParagraph;   // list of number of workers who completed per patch per paragraph
        public double moneySpent;
        protected List<List<int>> totalRequested;    // list of number of workers requested per patch per paragraph
        private List<int> numPatches;
        private List<List<bool>> done;

        /// <summary>
        /// Represents the data from a specific stage in a job.  For example: the Find, Fix, or Verify stage.
        /// </summary>
        /// <param name="type">The type of the stage.  For example, Find, Fix, or Verify</param>
        /// <param name="numParagraphs">The number of paragraphs in this job</param>
        public StageData(HITData.ResultType type, int numParagraphs)
        {
            this.type = type;
            this.FixParagraphNumber(numParagraphs);
        }

        public StageData(HITData.ResultType type)
        {
            this.type = type;
        }

        internal StageData() { }

        public void FixParagraphNumber(int numParagraphs)
        {
            this.numParagraphs = numParagraphs;
            this.moneySpent = 0;
            this.numPatches = new List<int>();
            this.totalRequested = new List<List<int>>();
            this.numCompletedperParagraph = new List<List<int>>();   // per patch
            done = new List<List<bool>>();
            for (int i = 0; i < numParagraphs; i++)
            {
                numPatches.Add(1);
                numCompletedperParagraph.Add(new List<int> { 0 });
                totalRequested.Add(new List<int> { 0 });
                done.Add(new List<bool>());
                done[i].Add(false);
            }
        }

        /// <summary>
        /// Registers a View as a listener for this Model
        /// </summary>
        /// <param name="sview">The view for this stage</param>
        public void registerListener(StageView sview)
        {
            listener = sview;
        }

        /// <summary>
        /// Updates the Model with a new status message and then notifies the listener
        /// </summary>
        /// <param name="status"></param>
        public void updateStage(TurKitSocKit.TurKitStatus status)
        {
            if (numPatches[status.paragraph] != status.totalPatches)  // we need to update the total number of patches
            {
                numPatches[status.paragraph] = status.totalPatches;
                numCompletedperParagraph[status.paragraph] = new List<int>();
                totalRequested[status.paragraph] = new List<int>();
                // need to initialize the list for the number of patches we have
                for (int i = 0; i < status.totalPatches; i++)
                {

                    numCompletedperParagraph[status.paragraph].Add(0);
                    totalRequested[status.paragraph].Add(0);
                    done[status.paragraph].Add(false);
                }
            }

            if (done[status.paragraph][status.patchNumber]) { return; }
            numCompletedperParagraph[status.paragraph][status.patchNumber] = status.numCompleted;
            moneySpent = status.payment * numCompleted;

            totalRequested[status.paragraph][status.patchNumber] = status.totalRequested;

            if (listener != null)
            {
                listener.notify();
            }
        }

        /// <summary>
        /// When ShortnData receives a StageComplete message, this should be called to reveal early termination to the user.
        /// </summary>
        /// <param name="paragraph">Paragraph number</param>
        /// <param name="patch">Patch number</param>
        public void terminatePatch(int paragraph, int patch)
        {
            if (done[paragraph][patch]) { return; }
            totalRequested[paragraph][patch] = numCompletedperParagraph[paragraph][patch];
            done[paragraph][patch] = true;
            if (listener != null)
            {
                listener.notify();
            }
        }
    }
}
