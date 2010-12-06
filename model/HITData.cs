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
using System.Xml.Serialization;

namespace Soylent.Model
{
    /// <summary>
    /// A superclass that represents the Model for an individual HIT.
    /// </summary>
    public abstract class HITData
    {
        public int job; 
        public enum ResultType { Find, Fix, Verify, Macro };
        //[XmlIgnore] public Dictionary<ResultType, StageData> stages;
        public int numParagraphs;
        public bool jobDone = false;
        // A list of TurKit messages used to recreate the results when the document is reloaded.
        public List<TurKitSocKit.TurKitFindFixVerify> findFixVerifies = new List<TurKitSocKit.TurKitFindFixVerify>();
        public List<TurKitSocKit.TurKitStageComplete> stageCompletes = new List<TurKitSocKit.TurKitStageComplete>();
        [XmlIgnore] public TurKit tk;
        [XmlIgnore] public HITView view;
        [XmlIgnore] public Word.Range range;
        public double cost;
        public string originalText
        {
            get
            {
                object bookmark = (object)range.BookmarkID;
                //return ((Microsoft.Office.Interop.Word.Bookmark)Globals.Soylent.Application.ActiveDocument.Bookmarks.get_Item(ref bookmark)).Range.Text;
                return ((Microsoft.Office.Interop.Word.Bookmark)Globals.Soylent.jobToDoc[this.job].Bookmarks.get_Item(ref bookmark)).Range.Text;
            }
        }
        public List<string> errors = new List<string>();

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
            string bookmarkName = "Soylent" + job;

            numParagraphs = range.Paragraphs.Count;

            //stages = new Dictionary<ResultType, StageData>();
            //TODO: Use Word XML binding to text instead of bookmarks.
            /*
             * Improved Data Mapping Provides Separation Between a Document's Data and Its Formatting
             *  XML mapping allows you to attach XML data to Word documents and link XML elements to placeholders in the document. 
             *  Combined with content controls, XML mapping becomes a powerful tool for developers. 
             *  These features provide you with the capability to position content controls in the document and then link them to XML elements. 
             *  This type of data and view separation allows you to access Word document data to repurpose and integrate with other systems and applications.
             */

            object bkmkRange = (object)range;
            Globals.Soylent.jobToDoc[this.job].Bookmarks.Add(bookmarkName, ref bkmkRange);
            
            tk = new TurKit(this);
        }

        public HITData()
        {
            this.range = null;
            this.job = 0;
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            string bookmarkName = "Soylent" + unixTime;

            numParagraphs = 1;//range.Paragraphs.Count;

            //stages = new Dictionary<ResultType, StageData>();
            //TODO: Use Word XML binding to text instead of bookmarks.
            /*
             * Improved Data Mapping Provides Separation Between a Document's Data and Its Formatting
             *  XML mapping allows you to attach XML data to Word documents and link XML elements to placeholders in the document. 
             *  Combined with content controls, XML mapping becomes a powerful tool for developers. 
             *  These features provide you with the capability to position content controls in the document and then link them to XML elements. 
             *  This type of data and view separation allows you to access Word document data to repurpose and integrate with other systems and applications.
             */

            //object bkmkRange = (object)range;
            //Globals.Soylent.Application.ActiveDocument.Bookmarks.Add(bookmarkName, ref bkmkRange);

            tk = new TurKit(this);
        }

        /// <summary>
        /// Start the task on the TurKit isntance tied to this job
        /// </summary>
        public void startTask()
        {
            TurKit.noKeysDelegate cancel = () =>
            {
                this.view.RemoveHITVIew();
            };

            tk.startTask(cancel);
        }

        /// <summary>
        /// Register a View that listens to this Model
        /// </summary>
        /// <param name="hview"></param>
        public virtual void register(HITView hview)
        {
            view = hview;
        }

        public abstract void updateStatus(TurKitSocKit.TurKitStatus status);

        public delegate void showErrorDelegate(string exceptionCode);
        public void showError(string exceptionCode)
        {
            view.showError(exceptionCode);
        }

    }
}
