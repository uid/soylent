using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Soylent.View;

namespace Soylent.Model.HumanMacro
{
    class HumanMacroJob
    {
        public static string HIT_TYPE = "Human-Macro";
        private HumanMacroResult data;
        private int jobNumber;
        /// <summary>
        /// The Model for a Crowdproof job.  This creates the View elements for this task
        /// </summary>
        /// <param name="data">The CrowdproofData instance for this job</param>
        /// <param name="jobNumber">The unique job number</param>
        //public HumanMacroJob(HumanMacroResult data, int jobNumber)
        //public HumanMacroJob(int jobNumber, Microsoft.Office.Interop.Word.Range range)
        public HumanMacroJob(Microsoft.Office.Interop.Word.Range toShorten, int job, HumanMacroResult.Separator separator, double reward, int redundancy, string title, string subtitle, string instructions, HumanMacroResult.ReturnType type, HumanMacroResult.TestOrReal test)
        {
            //this.data = data;
            this.jobNumber = job;

            Globals.Soylent.jobToDoc[jobNumber] = Globals.Soylent.Application.ActiveDocument;

            this.data = new HumanMacroResult(toShorten, job, separator, reward, redundancy, title, subtitle, instructions, type, test);

            HITView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber);
            hit.addStage(1, HITData.ResultType.Macro, data.macroStageData, "Running Macro", 10, 0.10, jobNumber);
            //hit.addStage(2, HITData.ResultType.Fix, "Fix Errors", 5, 0.05);
            //hit.addStage(3, HITData.ResultType.Verify, "Quality Control", 5, 0.05);

            data.startTask();
        }
        public HumanMacroJob(HumanMacroResult data, int jobNumber)
        {
            this.data = data;
            this.jobNumber = jobNumber;
            
            HITView hit = Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].addHIT(HIT_TYPE, data, jobNumber);
            hit.addStage(1, HITData.ResultType.Macro, data.macroStageData, "Running Macro", 10, 0.10, jobNumber);

            data.startTask();
        }
    }
}
