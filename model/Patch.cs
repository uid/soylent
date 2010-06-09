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
    public class Patch
    {
        public Word.Range range;
        public List<string> replacements;
        public string original;

        public Patch(Word.Range range, List<string> replacements)
        {
            this.range = range;
            this.replacements = replacements;
            this.original = range.Text;
        }
    }

    public class PatchSelection
    {
        public Patch patch;
        public string selection;
        public bool isCurrent {
            get {
                return (patch.range.Text == selection);
            }
        }
        public bool isOriginal
        {
            get
            {
                return (patch.original == selection);
            }
        }

        public PatchSelection(Patch patch, string selection)
        {
            this.patch = patch;
            this.selection = selection;
        }
    }
}
