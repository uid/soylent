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
    class ProofreadingPatch : Patch
    {
        public List<string> reasons { get; set; }

        public ProofreadingPatch(Word.Range original, List<string> replacements, List<string> reasons)
            : base(original, replacements)
        {
            this.reasons = reasons;
        }

    }
}
