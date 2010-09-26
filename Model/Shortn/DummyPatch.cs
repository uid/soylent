using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;

namespace Soylent.Model.Shortn
{
    class DummyPatch : ShortnPatch
    {
        /// <summary>
        /// Creates a dummy patch with the only option being its original text.
        /// </summary>
        /// <param name="range">A Range object the patch represents</param>
        public DummyPatch(Word.Range range) : base(range, new List<string> { range.Text })
        {
            // Creates a patch with the only option being itself
        }
    }
}
