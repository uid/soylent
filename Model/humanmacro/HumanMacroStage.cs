using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soylent.Model.HumanMacro
{
    class HumanMacroStage: StageData
    {
        public int redundancy;
        public override int numRequested
        {
            get
            {
                return numParagraphs * redundancy;
            }
        }

        public HumanMacroStage(HITData.ResultType type, int redundancy, int job)
            : base(type, job)
        {
            this.redundancy = redundancy;
        }
    }
}
