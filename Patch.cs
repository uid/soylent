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
        public Word.Range original;
        public List<string> replacements;

        public Patch(Word.Range original, List<string> replacements)
        {
            this.original = original;
            this.replacements = replacements;
        }
    }

    public class PatchSelection
    {
        public Patch patch;
        public string selection;
        public bool isOriginal {
            get {
                return (patch.original.Text == selection);
            }
        }

        public PatchSelection(Patch patch, string selection)
        {
            this.patch = patch;
            this.selection = selection;
        }
    }
}
