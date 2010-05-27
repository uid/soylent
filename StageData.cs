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
        public double moneySpent;
        public int totalRequested;

        public StageData(HITData.ResultType type, int numCompleted, int numParagraphs)
        {
            this.type = type;
            this.numCompleted = numCompleted;
            this.numParagraphs = numParagraphs;
            this.moneySpent = 0;
            this.totalRequested = 10;
            numCperP = new List<int>();
            for (int i = 0; i < numParagraphs; i++)
            {
                numCperP.Add(0);
                //numCperP[i] = 0;
            }
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
        public void updateStage(TurKitSocKit.TurKitStatus status)
        {
            numCperP[status.paragraph] = status.numCompleted;
            numCompleted = numCperP.Sum();

            moneySpent = status.payment * numCompleted;
            totalRequested = status.totalRequested;

            if (listener != null)
            {
                listener.notify();
            }
        }
    }
}
