using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Soylent
{
    class TurKit
    {
        public static string directory = @"C:\Users\msbernst\Documents\Soylent\turkit\cut";

        public TurKit(long request)
        {            
            string requestLine = "var request = " + request + ";";
            string[] script = File.ReadAllLines(directory + @"\cut.js");
            
            string[] newScript = new string[1 + script.Length];
            newScript[0] = requestLine;
            Array.Copy(script, 0, newScript, 1, script.Length);

            string requestFile = directory + @"\cut." + request + ".js";
            File.WriteAllLines(requestFile, newScript);

            string output = null;
            string error = null;
            ExecuteProcess( @"java"
                            , " -jar TurKit-0.2.3.jar -f " + requestFile + " -a amazonID -s amazonKEY -m sandbox -o 100 -h 1000"
                            , directory
                            , out output
                            , out error);
            Debug.Print(output);
            
            // TODO: if we wait, we could delete the file...google the original file back w/ ExecuteProcess
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
