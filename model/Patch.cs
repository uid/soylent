using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;

using System.Xml.Serialization;

namespace Soylent.Model
{
    /// <summary>
    /// A Patch represents a region of the original text.  It contains the different options Turkers have supplied for this particular range.
    /// </summary>
    public class Patch
    {
        [XmlIgnore] public Word.Range range;
        public List<string> replacements;
        public string original;

        /// <summary>
        /// A Patch represents a region of the original text.  It contains the different options Turkers have supplied for this particular range.
        /// </summary>
        /// <param name="range">The Range object this Patch represents</param>
        /// <param name="replacements">A list of replacement options for this range</param>
        public Patch(Word.Range range, List<string> replacements)
        {
            this.range = range;
            this.replacements = replacements;
            this.original = range.Text;
        }

        public Patch()
        {

        }
    }

    /// <summary>
    /// Represents a patch selection.  Gives the option that is currently selected for this patch in the View.
    /// </summary>
    public class PatchSelection
    {
        /// <summary>
        /// The patch that this object identifies a selection from
        /// </summary>
        public Patch patch;
        /// <summary>
        /// The selected text
        /// </summary>
        public string selection;
        /// <summary>
        /// Is this selection currently shown in the document
        /// </summary>
        public bool isCurrent {
            get {
                return (patch.range.Text == selection);
            }
        }
        /// <summary>
        /// Is this selection the text that was originally in the user-selected region
        /// </summary>
        public bool isOriginal
        {
            get
            {
                return (patch.original == selection);
            }
        }
        /// <summary>
        /// Represents a patch selection.  Gives the option that is currently selected for this patch in the View.
        /// </summary>
        /// <param name="patch">The patch that this object identifies a selection from</param>
        /// <param name="selection">The selected text</param>
        public PatchSelection(Patch patch, string selection)
        {
            this.patch = patch;
            this.selection = selection;
        }
    }
}
