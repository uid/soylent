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

namespace Soylent
{
    class TurKit
    {
        public string directory;
        public string rootDirectory;
        private string amazonSECRET;
        private string amazonKEY;
        private HITData hdata;
        private Timer turkitLoopTimer;

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
        public void startTask(){
            if (hdata is ShortenData)
            {
                ShortenData data = hdata as ShortenData;
     
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
                paragraphs = "var paragraphs = " + paragraphs + ";";

                int request = hdata.job;
                directory = rootDirectory + @"\turkit\templates\shortn\";

                string requestLine = "var soylentJob = " + request + ";";
                string[] script = File.ReadAllLines(directory + @"\shortn.template.js");

                string[] newScript = new string[2 + script.Length];
                newScript[0] = requestLine;
                newScript[1] = paragraphs;
                Array.Copy(script, 0, newScript, 2, script.Length);


                string requestFile = rootDirectory + @"\turkit\active-hits\shortn." + request + ".js";
                File.WriteAllLines(requestFile, newScript);

                InitializeAmazonKeys();
                
                ProcessInformation info = new ProcessInformation("java", 
                    " -jar TurKit-0.2.3.jar -f " + requestFile + " -a "+amazonKEY+" -s "+amazonSECRET+" -m sandbox -o 100 -h 1000", 
                    rootDirectory + @"\turkit", 
                    false);
                TimerCallback callback = ExecuteProcess;
                turkitLoopTimer = new Timer(callback, info, 0, 60 * 1000);  // starts the timer every 60 seconds
            }
        }
        
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

            using( Process process = Process.Start( new ProcessStartInfo( info.cmd, info.cmdParams ) ) )
            {
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
                //process.WaitForExit();
            }
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
