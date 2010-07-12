using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using Word = Microsoft.Office.Interop.Word;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Word;
using Microsoft.Office.Tools.Word.Extensions;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using Soylent.Model;
using Soylent.Model.Shortn;
using Soylent.Model.Crowdproof;
using Soylent.Model.HumanMacro;

namespace Soylent
{
    public class TurKit
    {
        public string directory;
        public string rootDirectory;
        private string amazonSECRET;
        private string amazonKEY;
        private HITData hdata;
        public Timer turkitLoopTimer;

        public static string TURKIT_VERSION = "TurKit-0.2.4.jar";
        /// <summary>
        /// Creates a TurKit job for the selected task.
        /// </summary>
        /// <param name="hdata">The HITData representing the desired job</param>
        public TurKit(HITData hdata)
        {
            rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (rootDirectory.Length > 10)
            {
                if (rootDirectory.Substring(rootDirectory.Length - 11, 10) == @"\bin\Debug")
                {
                    rootDirectory = rootDirectory.Substring(0, rootDirectory.Length - 10);
                }
            }
            this.hdata = hdata;
        }
        /// <summary>
        /// Starts a task.  For a Shortn task, this breaks the selected range into appropriate groupings, overwrites the template file, and runs TurKit on a Timer.
        /// </summary>
        public void startTask(){
            if (hdata is ShortnData)
            {
                ShortnData data = hdata as ShortnData;
     
                string[][] pgraphs = new string[data.range.Paragraphs.Count][];
                // Range.Paragraphs and Range.Sentences are 1 INDEXED
                for(int i = 0; i < data.range.Paragraphs.Count; i++){
                    Word.Paragraph paragraph = data.range.Paragraphs[i+1];
                    pgraphs[i] = new string[paragraph.Range.Sentences.Count];
                    for (int j = 0; j < paragraph.Range.Sentences.Count; j++ )
                    {
                        Word.Range sentence = paragraph.Range.Sentences[j+1];
                        string temp = sentence.Text;

                        // Whitespace characters in the middle of sentences will not be removed
                        temp = temp.Trim();
                        pgraphs[i][j] = temp;
                    }
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string paragraphs = js.Serialize(pgraphs);
                //string paragraphs = JsonConvert.SerializeObject(pgraphs);

                paragraphs = "var paragraphs = " + paragraphs + ";";

                // figure out whether there are one or two spaces between sentences
                string firstSentence = data.range.Paragraphs[1].Range.Sentences[1].Text;
                string spacesBetweenSentences = " ";
                if (firstSentence.EndsWith("  "))
                {
                    spacesBetweenSentences = "  ";
                }
                string numSpaces = "var sentence_separator = '" + spacesBetweenSentences + "';";

                int request = hdata.job;
                directory = rootDirectory + @"\turkit\templates\shortn\";

                string requestLine = "var soylentJob = " + request + ";";
                string debug = "var debug = " + (Soylent.DEBUG ? "true" : "false") + ";";

                string[] script = File.ReadAllLines(directory + @"\shortn.data.js");

                int new_lines = 4;
                string[] newScript = new string[new_lines + script.Length];
                newScript[0] = requestLine;
                newScript[1] = paragraphs;
                newScript[2] = debug;
                newScript[3] = numSpaces;
                Array.Copy(script, 0, newScript, new_lines, script.Length);

                // Delete old files
                /*
                if (!Soylent.DEBUG)
                {
                    DirectoryInfo di = new DirectoryInfo(rootDirectory + @"\turkit\active-hits\");
                    FileInfo[] rgFiles = di.GetFiles("shortn." + request + "*");
                    foreach (FileInfo file in rgFiles)
                    {
                        file.Delete();
                    }
                }
                 */

                string requestFile = rootDirectory + @"\turkit\active-hits\shortn." + request + ".data.js";
                File.WriteAllLines(requestFile, newScript, Encoding.UTF8);

                InitializeAmazonKeys();

                string arguments = " -jar " + TURKIT_VERSION + " -f \"" + requestFile + "\" -a " + amazonKEY + " -s " + amazonSECRET + " -o 100 -h 1000";
                if (Soylent.DEBUG)
                {
                    arguments += " -m sandbox";
                }
                else
                {
                    arguments += " -m real";
                }

                Debug.Print(arguments);

                //ProcessInformation info = new ProcessInformation("java", arguments, rootDirectory + @"\turkit", Soylent.DEBUG);
                ProcessInformation info = new ProcessInformation("java", arguments, rootDirectory + @"\turkit", false);

                TimerCallback callback = ExecuteProcess;
                int timer = 60 * 1000;
                if (Soylent.DEBUG)
                {
                    timer = 30 * 1000;
                }
                turkitLoopTimer = new Timer(callback, info, 0, timer);  // starts the timer every 60 seconds
            }
            else if (hdata is CrowdproofData)
            {
                CrowdproofData data = hdata as CrowdproofData;

                string[][] pgraphs = new string[data.range.Paragraphs.Count][];
                // Range.Paragraphs and Range.Sentences are 1 INDEXED
                for (int i = 0; i < data.range.Paragraphs.Count; i++)
                {
                    Word.Paragraph paragraph = data.range.Paragraphs[i + 1];
                    pgraphs[i] = new string[paragraph.Range.Sentences.Count];
                    for (int j = 0; j < paragraph.Range.Sentences.Count; j++)
                    {
                        Word.Range sentence = paragraph.Range.Sentences[j + 1];
                        string temp = sentence.Text;

                        // Whitespace characters in the middle of sentences will not be removed
                        temp = temp.Trim();
                        pgraphs[i][j] = temp;
                    }
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string paragraphs = js.Serialize(pgraphs);
                //string paragraphs = JsonConvert.SerializeObject(pgraphs);

                paragraphs = "var paragraphs = " + paragraphs + ";";

                // figure out whether there are one or two spaces between sentences
                string firstSentence = data.range.Paragraphs[1].Range.Sentences[1].Text;
                string spacesBetweenSentences = " ";
                if (firstSentence.EndsWith("  "))
                {
                    spacesBetweenSentences = "  ";
                }
                string numSpaces = "var sentence_separator = '" + spacesBetweenSentences + "';";

                int request = hdata.job;
                directory = rootDirectory + @"\turkit\templates\crowdproof\";

                string requestLine = "var soylentJob = " + request + ";";
                string debug = "var debug = " + (Soylent.DEBUG ? "true" : "false") + ";";

                string[] script = File.ReadAllLines(directory + @"\crowdproof.data.js");

                int new_lines = 4;
                string[] newScript = new string[new_lines + script.Length];
                newScript[0] = requestLine;
                newScript[1] = paragraphs;
                newScript[2] = debug;
                newScript[3] = numSpaces;
                Array.Copy(script, 0, newScript, new_lines, script.Length);

                // Delete old files
                /*
                if (!Soylent.DEBUG)
                {
                    DirectoryInfo di = new DirectoryInfo(rootDirectory + @"\turkit\active-hits\");
                    FileInfo[] rgFiles = di.GetFiles("shortn." + request + "*");
                    foreach (FileInfo file in rgFiles)
                    {
                        file.Delete();
                    }
                }
                 */

                string requestFile = rootDirectory + @"\turkit\active-hits\crowdproof." + request + ".data.js";
                File.WriteAllLines(requestFile, newScript, Encoding.UTF8);

                InitializeAmazonKeys();

                string arguments = " -jar " + TURKIT_VERSION + " -f \"" + requestFile + "\" -a " + amazonKEY + " -s " + amazonSECRET + " -o 100 -h 1000";
                if (Soylent.DEBUG)
                {
                    arguments += " -m sandbox";
                }
                else
                {
                    arguments += " -m real";
                }

                
                ProcessInformation info = new ProcessInformation("java", arguments, rootDirectory + @"\turkit", false);

                TimerCallback callback = ExecuteProcess;
                int timer = 60 * 1000;
                if (Soylent.DEBUG)
                {
                    timer = 30 * 1000;
                }
                turkitLoopTimer = new Timer(callback, info, 0, timer);  // starts the timer every 60 seconds
                
            }
            else if (hdata is HumanMacroResult)
            {
                HumanMacroResult data = hdata as HumanMacroResult;


                JavaScriptSerializer js = new JavaScriptSerializer();
                string inputs;

                if (data.separator == HumanMacroResult.Separator.Paragraph)
                {
                    string[] pgraphs;

                    if (data.test == HumanMacroResult.TestOrReal.Test)
                    {
                        pgraphs = new string[1];
                    }
                    else
                    {
                        pgraphs = new string[data.range.Paragraphs.Count];
                    }
                    for (int i = 0; i < data.range.Paragraphs.Count; i++)
                    {
                        Word.Paragraph paragraph = data.range.Paragraphs[i + 1];
                        string temp = paragraph.Range.Text;
                        temp = temp.Trim();
                        pgraphs[i] = temp;

                        /*
                        Patch patch = new Patch(paragraph.Range, new List<string>());
                        patch.original = paragraph.Range.Text;
                         */
                        HumanMacroPatch patch = new HumanMacroPatch(paragraph.Range, paragraph.Range.Start - data.range.Start, paragraph.Range.End - data.range.Start);
                        patch.original = paragraph.Range.Text;
                        data.patches.Add(patch);
                        if (data.test == HumanMacroResult.TestOrReal.Test)
                        {
                            break;
                        }
                    }
                    inputs = js.Serialize(pgraphs);
                }
                else
                {
                    string[] pgraphs;

                    if (data.test == HumanMacroResult.TestOrReal.Test)
                    {
                        pgraphs = new string[1];
                    }
                    else{
                        pgraphs = new string[data.range.Sentences.Count];
                    }
                    for (int i = 0; i < data.range.Sentences.Count; i++)
                    {
                        Word.Range range = data.range.Sentences[i + 1];

                        string temp = range.Text;
                        // Whitespace characters in the middle of sentences will not be removed
                        temp = temp.Trim();
                        pgraphs[i] = temp;

                        //Patch patch = new Patch(range, new List<string>());
                        HumanMacroPatch patch = new HumanMacroPatch(range, range.Start - data.range.Start, range.End - data.range.Start);
                        patch.original = range.Text;
                        data.patches.Add(patch);

                        if (data.test == HumanMacroResult.TestOrReal.Test)
                        {
                            break;
                        }
                    }
                    inputs = js.Serialize(pgraphs);
                }
                /*
                // Range.Paragraphs and Range.Sentences are 1 INDEXED
                for (int i = 0; i < data.range.Paragraphs.Count; i++)
                {
                    Word.Paragraph paragraph = data.range.Paragraphs[i + 1];
                    pgraphs[i] = new string[paragraph.Range.Sentences.Count];
                    for (int j = 0; j < paragraph.Range.Sentences.Count; j++)
                    {
                        Word.Range sentence = paragraph.Range.Sentences[j + 1];
                        string temp = sentence.Text;

                        // Whitespace characters in the middle of sentences will not be removed
                        temp = temp.Trim();
                        pgraphs[i][j] = temp;
                    }
                }*/
                //JavaScriptSerializer js = new JavaScriptSerializer();
                //string inputs = js.Serialize(pgraphs);

                inputs = "var inputs = " + inputs + ";";

                // figure out whether there are one or two spaces between sentences
                string firstSentence = data.range.Paragraphs[1].Range.Sentences[1].Text;
                string spacesBetweenSentences = " ";
                if (firstSentence.EndsWith("  "))
                {
                    spacesBetweenSentences = "  ";
                }
                data.patchesFound(spacesBetweenSentences);

                string numSpaces = "var sentence_separator = '" + spacesBetweenSentences + "';";

                int request = hdata.job;
                directory = rootDirectory + @"\turkit\templates\human-macro\";

                string requestLine = "var soylentJob = " + request + ";";
                string debug = "var debug = " + (Soylent.DEBUG ? "true" : "false") + ";";

                string reward = "var reward = " + data.reward + ";";
                string redundancy = "var redundancy = " + data.redundancy + ";";
                string title = "var title = '" + data.title + "';";
                string subtitle = "var subtitle = '" + data.subtitle + "';";
                string instructions = "var instructions = '" + data.instructions + "';";

                string[] script = File.ReadAllLines(directory + @"\macro.data.js");

                int new_lines = 9;
                string[] newScript = new string[new_lines + script.Length];
                newScript[0] = requestLine;
                newScript[1] = inputs;
                newScript[2] = debug;
                newScript[3] = numSpaces;
                newScript[4] = reward;
                newScript[5] = redundancy;
                newScript[6] = title;
                newScript[7] = subtitle;
                newScript[8] = instructions;
                Array.Copy(script, 0, newScript, new_lines, script.Length);

                // Delete old files
                /*
                if (!Soylent.DEBUG)
                {
                    DirectoryInfo di = new DirectoryInfo(rootDirectory + @"\turkit\active-hits\");
                    FileInfo[] rgFiles = di.GetFiles("shortn." + request + "*");
                    foreach (FileInfo file in rgFiles)
                    {
                        file.Delete();
                    }
                }
                 */

                string requestFile = rootDirectory + @"\turkit\active-hits\macro." + request + ".data.js";
                File.WriteAllLines(requestFile, newScript, Encoding.UTF8);

                InitializeAmazonKeys();

                
                string arguments = " -jar " + TURKIT_VERSION + " -f \"" + requestFile + "\" -a " + amazonKEY + " -s " + amazonSECRET + " -o 100 -h 1000";
                if (Soylent.DEBUG)
                {
                    arguments += " -m sandbox";
                }
                else
                {
                    arguments += " -m real";
                }

                //ProcessInformation info = new ProcessInformation("java", arguments, rootDirectory + @"\turkit", Soylent.DEBUG);
                ProcessInformation info = new ProcessInformation("java", arguments, rootDirectory + @"\turkit", false);

                TimerCallback callback = ExecuteProcess;
                int timer = 60 * 1000;
                if (Soylent.DEBUG)
                {
                    timer = 30 * 1000;
                }
                turkitLoopTimer = new Timer(callback, info, 0, timer);  // starts the timer every 60 seconds
                
            }
        }
        /// <summary>
        /// Reads in the AMT secret and key from the amazon.xml file so that HITs can be submitted.
        /// </summary>
        public void InitializeAmazonKeys()
        {
            //System.Xml.XmlTextReader amazonReader = new System.Xml.XmlTextReader("./amazon.template.xml");
            XDocument doc = XDocument.Load(rootDirectory+@"\amazon.xml");
            XElement secret = doc.Root.Element("amazonSECRET");
            XElement key = doc.Root.Element("amazonKEY");
            amazonSECRET = secret.Value;
            amazonKEY = key.Value;
        }
         
        ///<summary>
        /// Executes a process and waits for it to end. 
        ///</summary>
        ///<param name="cmd">Full Path of process to execute.</param>
        ///<param name="cmdParams">Command Line params of process</param>
        ///<param name="workingDirectory">Process' working directory</param>
        ///<param name="timeout">Time to wait for process to end</param>
        ///<param name="stdOutput">Redirected standard output of process</param>
        ///<returns>Process exit code</returns>
        private void ExecuteProcess(object infoObject)
        {
            string output, error;

            ProcessInformation info = (ProcessInformation) infoObject;
            if (info.showWindow)
            {
                info.cmdParams = " /k " + info.cmd + info.cmdParams;
                info.cmd = "cmd";
            }

            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(info.cmd, info.cmdParams);
            process.StartInfo.WorkingDirectory = info.workingDirectory;
            process.StartInfo.UseShellExecute = false;
            if (!info.showWindow)
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
            }
            process.Start( );
            if (!info.showWindow)
            {
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
            }
            else
            {
                output = null;
                error = null;
            }
            process.WaitForExit();
        }

        private class ProcessInformation
        {
            public string cmd { get; set; }
            public string cmdParams { get; set; }
            public string workingDirectory { get ; set; }
            public bool showWindow { get; set; }

            public ProcessInformation(string cmd, string cmdParams, string workingDirectory, bool showWindow) {
                this.cmd = cmd;
                this.cmdParams = cmdParams;
                this.workingDirectory = workingDirectory;
                this.showWindow = showWindow;
            }
        }
    }
}
