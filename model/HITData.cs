using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Forms;
using Soylent.View;

namespace Soylent.Model
{
    /// <summary>
    /// A superclass that represents the Model for an individual HIT.
    /// </summary>
    public class HITData
    {
        public int job { get; set; }
        public Word.Range range { get; set; }
        public enum ResultType { Find, Fix, Verify, Macro };
        public Dictionary<ResultType, StageData> stages;
        public Dictionary<string, ResultType> typeMap;// = new Dictionary<string,ResultType>();
        public int numParagraphs;
        public TurKit tk;
        public HITView view;

        public string originalText
        {
            get
            {
                object bookmark = (object)range.BookmarkID;
                return ((Microsoft.Office.Interop.Word.Bookmark)Globals.Soylent.Application.ActiveDocument.Bookmarks.get_Item(ref bookmark)).Range.Text;
            }
        }

        /// <summary>
        /// A superclass that represents the Model for an individual HIT.
        /// </summary>
        /// <param name="range">The Range object selected for this task</param>
        /// <param name="job">The unique job number for this task</param>
        public HITData(Word.Range range, int job)
        {
            this.range = range;
            this.job = job;
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string bookmarkName = "Soylent" + unixTime;

            typeMap = new Dictionary<string,ResultType>();

            numParagraphs = range.Paragraphs.Count;

            stages = new Dictionary<ResultType, StageData>();
            //TODO: Use Word XML binding to text instead of bookmarks.
            /*
             * Improved Data Mapping Provides Separation Between a Document's Data and Its Formatting
             *  XML mapping allows you to attach XML data to Word documents and link XML elements to placeholders in the document. 
             *  Combined with content controls, XML mapping becomes a powerful tool for developers. 
             *  These features provide you with the capability to position content controls in the document and then link them to XML elements. 
             *  This type of data and view separation allows you to access Word document data to repurpose and integrate with other systems and applications.
             */

            object bkmkRange = (object)range;
            Globals.Soylent.Application.ActiveDocument.Bookmarks.Add(bookmarkName, ref bkmkRange);

            tk = new TurKit(this);
        }

        /// <summary>
        /// Start the task on the TurKit isntance tied to this job
        /// </summary>
        public void startTask()
        {
            tk.startTask();
        }

        /// <summary>
        /// Register a View that listens to this Model
        /// </summary>
        /// <param name="hview"></param>
        public virtual void register(HITView hview)
        {
            view = hview;
        }

        public void updateStatus(TurKitSocKit.TurKitStatus status)
        {
            System.Diagnostics.Debug.WriteLine("GOT A STATUSSSSSSS");
            /* moved to ShortenData.cs
            string stringtype = status.method;
            ResultType type = typeMap[stringtype];
            StageData stage = stages[type];
            stage.updateStage(status.numCompleted);
            */
        }
    }
}
