using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;

namespace Soylent.Model.HumanMacro
{
    public class HumanMacroPatch: Patch
    {
        public int rangeStart;
        public int rangeEnd;

        public HumanMacroPatch(Word.Range range, int start, int end) : base(range, new List<string>()) {
            rangeStart = start;
            rangeEnd = end;
        }

        public HumanMacroPatch() : base() { }
    }
}
