using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace Soylent
{
    class TurKit
    {
        public string directory = null;
        public string rootDirectory = null;
        //public static string directory = @"C:\Users\msbernst\Documents\Soylent\turkit\cut";
        private string amazonID;
        private string amazonKEY;

        public TurKit(long request)
        {
            rootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //rootDirectory = @"C:\Users\UID\Documents\soylent\";
            if (rootDirectory.Length > 10)
            {
                if (rootDirectory.Substring(rootDirectory.Length - 11, 10) == @"\bin\Debug")
                {
                    rootDirectory = rootDirectory.Substring(0, rootDirectory.Length - 10);
                }
            }
            //{
            //    directory = directory.Substring(0, directory.Length - 10);
            //}
            directory = rootDirectory + @"\turkit\cut\template";
            //string lastten = directory.Substring(directory.Length - 11, 10);

            string requestLine = "var request = " + request + ";";
            string[] script = File.ReadAllLines(directory + @"\cut.js");

            string[] newScript = new string[1 + script.Length];
            newScript[0] = requestLine;
            Array.Copy(script, 0, newScript, 1, script.Length);


            string requestFile = directory + @"\cut." + request + ".js";
            File.WriteAllLines(requestFile, newScript);

            InitializeAmazonKeys();

            string output = null;
            string error = null;
            ExecuteProcess( @"java"
                            , " -jar TurKit-0.2.3.jar -f " + requestFile + " -a "+amazonID+" -s "+amazonKEY+" -m sandbox -o 100 -h 1000"
                            , directory
                            , out output
                            , out error);
            Debug.Print(output);
            
            // TODO: if we wait, we could delete the file...google the original file back w/ ExecuteProcess
        }
        
        public void InitializeAmazonKeys()
        {
            //System.Xml.XmlTextReader amazonReader = new System.Xml.XmlTextReader("./amazon.template.xml");
            XDocument doc = XDocument.Load(rootDirectory+@"\amazon.template.xml");
            XElement id = doc.Root.Element("amazonID");
            XElement key = doc.Root.Element("amazonKEY");
            amazonID = id.Value;
            amazonKEY = key.Value;

            //Debug.Print(amazonID);
            //Debug.Print(amazonKEY);
            System.Diagnostics.Trace.WriteLine(amazonID);
            System.Diagnostics.Trace.WriteLine(amazonKEY);
            //Console.WriteLine(amazonID);
            //Console.WriteLine(amazonKEY);
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
        private void ExecuteProcess(string cmd, string cmdParams, string workingDirectory, out string output, out string error)
        {
            using( Process process = Process.Start( new ProcessStartInfo( cmd, cmdParams ) ) )
            {
                process.StartInfo.WorkingDirectory = workingDirectory;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.Start( );
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
            }
        }
    }
}
