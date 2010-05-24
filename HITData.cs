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
    public class HITData
    {
        HITData h;
        public int job { get; set; }
        public Word.Range range { get; set; }

        public HITData(Word.Range range, int job)
        {
            this.range = range;
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string bookmarkName = "Soylent" + unixTime;
            //TODO: Use Word XML binding to text instead of bookmarks.
            /*
             * Improved Data Mapping Provides Separation Between a Document's Data and Its Formatting

                XML mapping allows you to attach XML data to Word documents and link XML elements to placeholders in the document. Combined with content controls, XML mapping becomes a powerful tool for developers. These features provide you with the capability to position content controls in the document and then link them to XML elements. This type of data and view separation allows you to access Word document data to repurpose and integrate with other systems and applications.
             */

            object bkmkRange = (object)range;
            Globals.Soylent.Application.ActiveDocument.Bookmarks.Add(bookmarkName, ref bkmkRange);
        }
    }
}
