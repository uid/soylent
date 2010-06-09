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
    class Shortn
    {
        public static string HIT_TYPE = "Shortn";
        private ShortnData data;
        private int jobNumber;

        public Shortn(ShortnData data, int jobNumber)
        {
            this.data = data;
            this.jobNumber = jobNumber;

            ShortnView hit = Globals.Soylent.soylent.addHIT(HIT_TYPE, data, jobNumber) as ShortnView;
            hit.addStage(1, HITData.ResultType.Find, "Find Verbose Text", 10, 0.10);
            hit.addStage(2, HITData.ResultType.Fix, "Shorten Verbose Text", 5, 0.05);
            hit.addStage(3, HITData.ResultType.Verify, "Quality Control", 5, 0.05);

            data.startTask();
        }
    }
}
