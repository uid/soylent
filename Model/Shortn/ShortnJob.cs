using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;

using Soylent.View.Shortn;

namespace Soylent.Model.Shortn
{
    /// <summary>
    /// The Model for a Shortn job.  This creates the View elements for this task
    /// </summary>
    class ShortnJob
    {
        public static string HIT_TYPE = "Shortn";
        private ShortnData data;
        private int jobNumber;

        /// <summary>
        /// The Model for a Shortn job.  This creates the View elements for this task
        /// </summary>
        /// <param name="data">The ShortnData instance for this job</param>
        /// <param name="jobNumber">The unique job number</param>
        //public ShortnJob(ShortnData data, int jobNumber)
        public ShortnJob(int jobNumber, Word.Range range)
        {
            //this.data = data;
            this.jobNumber = jobNumber;

            Globals.Soylent.jobToDoc[jobNumber] = Globals.Soylent.Application.ActiveDocument;

            this.data = new ShortnData(range, jobNumber);

            ShortnView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber) as ShortnView;
            hit.addStage(1, HITData.ResultType.Find, data.findStageData, "Find Verbose Text", 10, 0.10, jobNumber);
            hit.addStage(2, HITData.ResultType.Fix, data.fixStageData, "Shorten Verbose Text", 5, 0.05, jobNumber);
            hit.addStage(3, HITData.ResultType.Verify, data.verifyStageData, "Quality Control", 5, 0.05, jobNumber);

            data.startTask();
        }
        /// <summary>
        /// This constructor used only when recreating a job from XML saved with a document
        /// </summary>
        /// <param name="data"></param>
        /// <param name="jobNumber"></param>
        public ShortnJob(ShortnData data, int jobNumber, bool startTurk)
        {
            this.data = data;
            this.jobNumber = jobNumber;

            ShortnView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber) as ShortnView;
            hit.addStage(1, HITData.ResultType.Find, data.findStageData, "Find Verbose Text", 10, 0.10, jobNumber);
            hit.addStage(2, HITData.ResultType.Fix, data.fixStageData, "Shorten Verbose Text", 5, 0.05, jobNumber);
            hit.addStage(3, HITData.ResultType.Verify, data.verifyStageData, "Quality Control", 5, 0.05, jobNumber);

            if (startTurk)
            {
                data.startTask();
            }
        }
    }
}
