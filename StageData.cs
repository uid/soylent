using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soylent
{
    public class StageData
    {
        private StageView listener;
        private HITData.ResultType type;
        public int numCompleted { get; set; }
        private int numParagraphs;
        private List<int> numCperP;

        public StageData(HITData.ResultType type, int numCompleted, int numParagraphs)
        {
            this.type = type;
            this.numCompleted = numCompleted;
            this.numParagraphs = numParagraphs;
            numCperP = new List<int>(numParagraphs);
        }
        public void registerListener(StageView sview)
        {
            listener = sview;
        }
        public void updateStage(int numCthisP, int paragraph)
        {
            //TODO: figure out how we want to do this.
            numCperP[paragraph] = numCthisP;
            numCompleted = numCperP.Sum();

            if (listener != null)
            {
                listener.notify();
            }
        }
    }
}
