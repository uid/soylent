using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;  

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;

namespace Soylent
{
    class Shorten
    {
        public static string HIT_TYPE = "Shortn";
        private ShortenData data;
        private int jobNumber;

        public Shorten(ShortenData data, int jobNumber)
        {
            this.data = data;
            this.jobNumber = jobNumber;

            //HITView hit = Globals.Soylent.soylent.addHIT(HIT_TYPE, data.originalText);
            HITView hit = Globals.Soylent.soylent.addHIT(HIT_TYPE, data, jobNumber);
            hit.addStage(1, HITData.ResultType.Find, "Find Shortenable Regions", 10, 0.10);
            hit.addStage(2, HITData.ResultType.Fix, "Shorten Text", 5, 0.05);
            hit.addStage(3, HITData.ResultType.Verify, "Verify Work", 5, 0.05);

            //hit.stageList[0].updateProgress(10, 0.10);
            //hit.stageList[1].updateProgress(3, 0.03);

            data.startTask();
        }
    }
}
