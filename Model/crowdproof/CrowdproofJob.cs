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
        public CrowdproofJob(CrowdproofData data, int jobNumber)
        {
            this.data = data;
            this.jobNumber = jobNumber;

            HITView hit = Globals.Soylent.soylent.addHIT(HIT_TYPE, data, jobNumber);
            hit.addStage(1, HITData.ResultType.Find, "Identify Errors", 10, 0.10);
            hit.addStage(2, HITData.ResultType.Fix, "Fix Errors", 5, 0.05);
            hit.addStage(3, HITData.ResultType.Verify, "Quality Control", 5, 0.05);

            data.startTask();
        }
    }
}
