using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
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
                int i = 0;
                int j;
                foreach(Word.Paragraph paragraph in data.range.Paragraphs){
                    pgraphs[i] = new string[paragraph.Range.Sentences.Count];
                    j = 0;
                    foreach (Word.Range sentence in paragraph.Range.Sentences)
                    {
                        string temp = sentence.Text;

                        // Whitespace characters in the middle of sentences will not be removed
                        temp = temp.Trim();
                        pgraphs[i][j] = temp;

                        j++;
                    }
                    i++;
                    //System.Diagnostics.Trace.WriteLine("*************** end paragraph");
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string paragraphs = js.Serialize(pgraphs);
                //System.Diagnostics.Trace.WriteLine(sentences);
                paragraphs = "var paragraphs = " + paragraphs + ";";

                //string text = data.range.Text;
                //System.Diagnostics.Trace.WriteLine(text);

                int request = hdata.job;
                directory = rootDirectory + @"\turkit\templates\shortn\";
                //string lastten = directory.Substring(directory.Length - 11, 10);

                string requestLine = "var soylentJob = " + request + ";";
                string[] script = File.ReadAllLines(directory + @"\shortn.template.js");

                string[] newScript = new string[2 + script.Length];
                newScript[0] = requestLine;
                newScript[1] = paragraphs;
                Array.Copy(script, 0, newScript, 2, script.Length);


                string requestFile = rootDirectory + @"\turkit\active-hits\shortn." + request + ".js";
                File.WriteAllLines(requestFile, newScript);

                InitializeAmazonKeys();

                string output = null;
                string error = null;
                
                
                ExecuteProcess( @"java"
                                , " -jar TurKit-0.2.3.jar -f " + requestFile + " -a "+amazonKEY+" -s "+amazonSECRET+" -m sandbox -o 100 -h 1000"
                                , rootDirectory + @"\turkit"
                                , out output
                                , out error
                               , false);
                
                /*
                ExecuteProcess(@"cmd"
                                , " /k java -jar TurKit-0.2.3.jar -f " + requestFile + " -a " + amazonKEY + " -s " + amazonSECRET + " -m sandbox -o 100 -h 1000"
                                , rootDirectory + @"\turkit"
                                , out output
                                , out error
                                , true);
                 */

                // TODO: if we wait, we could delete the file...google the original file back w/ ExecuteProcess
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
        private void ExecuteProcess(string cmd, string cmdParams, string workingDirectory, out string output, out string error, bool showWindow)
        {
            using( Process process = Process.Start( new ProcessStartInfo( cmd, cmdParams ) ) )
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.UseShellExecute = false;
                if (!showWindow)
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                }
                process.Start( );
                if (!showWindow)
                {
                    output = process.StandardOutput.ReadToEnd();
                    error = process.StandardError.ReadToEnd();
                }
                else
                {
                    output = error = null;
                }
                process.WaitForExit();
            }
        }
    }
}
