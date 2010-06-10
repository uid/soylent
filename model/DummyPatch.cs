using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;

namespace Soylent.Model
{
    class DummyPatch : Patch
    {
        public DummyPatch(Word.Range range) : base(range, new List<string> { range.Text })
        {
            // Creates a patch with the only option being itself
        }
    }
}
