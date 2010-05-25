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

        public Shorten(ShortenData data)
        {
            this.data = data;

            HITStatus hit = Globals.Soylent.soylent.addHIT(HIT_TYPE, data.originalText);
            hit.addStage(1, "Find Shortenable Regions", 10, 0.10);
            hit.addStage(2, "Shorten Text", 5, 0.05);
            hit.addStage(3, "Verify Work", 5, 0.05);

            hit.stageList[0].updateProgress(10, 0.10);
            hit.stageList[1].updateProgress(3, 0.03);

            data.startTask();
        }
    }
}
