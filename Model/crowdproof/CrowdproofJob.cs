using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;
using Soylent.View;
using Soylent.View.Crowdproof;

namespace Soylent.Model.Crowdproof
{
    /// <summary>
    /// The Model for a Crowdproof job.  This creates the View elements for this task
    /// </summary>
    class CrowdproofJob
    {
        public static string HIT_TYPE = "Crowdproof";
        private CrowdproofData data;
        private int jobNumber;

        /// <summary>
        /// The Model for a Crowdproof job.  This creates the View elements for this task
        /// </summary>
        /// <param name="data">The CrowdproofData instance for this job</param>
        /// <param name="jobNumber">The unique job number</param>
        public CrowdproofJob(int jobNumber, Word.Range range)
        {
            //this.data = data;
            this.jobNumber = jobNumber;

            Globals.Soylent.jobToDoc[jobNumber] = Globals.Soylent.Application.ActiveDocument;

            this.data = new CrowdproofData(range, jobNumber);

            CrowdproofView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber) as CrowdproofView;
            hit.addStage(1, HITData.ResultType.Find, data.findStageData, "Identify Errors", 10, 0.10, jobNumber);
            hit.addStage(2, HITData.ResultType.Fix, data.fixStageData, "Fix Errors", 5, 0.05, jobNumber);
            hit.addStage(3, HITData.ResultType.Verify, data.verifyStageData, "Quality Control", 5, 0.05, jobNumber);

            data.startTask();
        }
        public CrowdproofJob(CrowdproofData data, int jobNumber)
        {
            this.data = data;
            this.jobNumber = jobNumber;

            CrowdproofView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber) as CrowdproofView;
            hit.addStage(1, HITData.ResultType.Find, data.findStageData, "Identify Errors", 10, 0.10, jobNumber);
            hit.addStage(2, HITData.ResultType.Fix, data.fixStageData, "Fix Errors", 5, 0.05, jobNumber);
            hit.addStage(3, HITData.ResultType.Verify, data.verifyStageData, "Quality Control", 5, 0.05, jobNumber);

            data.startTask();
        }

    }
}
