using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using VSTO = Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Soylent.Model.HumanMacro
{
    class HumanMacroResult
    {
        public enum ResultType {Comment, SmartTag};
        
        private Word.Range text;
        private List<String> results;

        public static string NAMESPACE = "http://uid.csail.mit.edu/soylent";

        public HumanMacroResult(Word.Range text, List<String> results)
        {
            this.text = text;
            this.results = results;
        }

        public void AnnotateResult(ResultType type)
        {
            if (type == ResultType.SmartTag)
            {
                string tag = NAMESPACE + "#soylent" + DateTime.Now.Ticks;
                SmartTag resultTag = new SmartTag(tag, "Soylent Results: " + text.Text.Substring(0, 10) + "...");
                Regex pattern = new Regex(text.Text.Trim().Replace(" ", "\\s"), RegexOptions.IgnorePatternWhitespace);
                resultTag.Expressions.Add(pattern);

                List<VSTO.Action> actions = new List<Microsoft.Office.Tools.Word.Action>();
                foreach (string result in results)
                {
                    VSTO.Action action = new VSTO.Action(result);
                    action.Click += new ActionClickEventHandler(replaceText);
                    actions.Add(action);
                }
                resultTag.Actions = actions.ToArray();

                Globals.Soylent.VstoSmartTags.Add(resultTag);
            }
            else if (type == ResultType.Comment)
            {
                foreach (string result in results)
                {
                    object commentText = result;
                    Globals.Soylent.Application.ActiveDocument.Comments.Add(text, ref commentText);
                    foreach (Microsoft.Office.Interop.Word.Comment c in Globals.Soylent.Application.ActiveDocument.Comments)
                    {
                        c.Author = "Turker";
                        c.Initial = "Turker";
                    }
                    //Globals.Soylent.Application.ActiveDocument.Comments[0].Author = "Turker";
                }
            }
        }

        public static void replaceText(object sender, ActionEventArgs e)
        {
            Object unit = Type.Missing;
            Object count = Type.Missing;
            e.Range.Delete(ref unit, ref count);
            String newText = ((VSTO.Action)sender).Caption;
            e.Range.InsertAfter(newText);
        }
    }
}
