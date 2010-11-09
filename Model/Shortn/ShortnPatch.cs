using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Soylent.Model.Shortn
{
    public class ShortnPatch: Patch
    {
        public bool isLocked = false;
        public string lockedReplacement = null;

        public ShortnPatch(): base() {}

        public ShortnPatch(Microsoft.Office.Interop.Word.Range range, List<string> replacements): base(range, replacements) {}

        public void lockSelection(string lockedReplacement){
            this.lockedReplacement = lockedReplacement;
            this.isLocked = true;
        }

        public void unlockSelection()
        {
            this.lockedReplacement = null;
            this.isLocked = false;
        }
    }
}
