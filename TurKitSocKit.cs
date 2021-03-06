﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using Soylent.Model;
using Soylent.Model.Shortn;
using Soylent.Model.Crowdproof;
using Soylent.Model.HumanMacro;
using System.Windows.Forms;
using Soylent.View;


namespace Soylent
{
    /**
     * Connects to a TurKit instance
     */
    public class TurKitSocKit
    {
        private int port = 11000;
        private List<ConnectionInfo> _connections = new List<ConnectionInfo>();
        private Socket serverSocket;

        private class ConnectionInfo
        {
            public Socket Socket;
            public byte[] Buffer;
        }

        public TurKitSocKit()
        {
        }

        ~TurKitSocKit()
        {
            // destructor to make sure that socket is closed
            serverSocket.Close();
        }
        /// <summary>
        /// Connects to a socket on the local machine and begins listening on the socket.
        /// </summary>
        public void Listen() {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEP = new IPEndPoint(address, port);
            Debug.WriteLine("Local address and port : " + localEP.ToString());
            serverSocket = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.IP);
            serverSocket.ReceiveBufferSize = 100000;

            try
            {
                serverSocket.Bind(localEP);
                serverSocket.Listen(10);

                Debug.WriteLine("Waiting for a connection...");
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw e;
            }

            Debug.WriteLine("Closing the listener...");
        }

        /*
         * Function called as an asynchronous callback when data is received on the socket.
         */
        private void AcceptCallback(IAsyncResult result)
        {
            Console.WriteLine("Got a connection!");
            ConnectionInfo connection = new ConnectionInfo();
            try
            {
                // Finish Accept
                Socket s = (Socket)result.AsyncState;
                s.ReceiveBufferSize = 100000;
                connection.Socket = s.EndAccept(result);
                connection.Socket.ReceiveBufferSize = 100000;
                connection.Buffer = new byte[100000];
                lock (_connections) _connections.Add(connection);

                // Start Receive and a new Accept
                connection.Socket.BeginReceive(connection.Buffer, 0, connection.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), connection);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), result.AsyncState);
            }
            catch (SocketException exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
            }
            catch (Exception exc)
            {
                CloseConnection(connection);
                Console.WriteLine("Exception: " + exc);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            Debug.Write("Receiving data: ");
            ConnectionInfo connection = (ConnectionInfo)result.AsyncState;

            int bytesRead = connection.Socket.EndReceive(result);
            Debug.WriteLine(bytesRead + " bytes");
            if (0 != bytesRead)
            {
                /**
                    * TurKit sends us information that looks like JSON
                    * {
                    *      "__type__": "status",
                    *      "percent": 43.5,
                    *      ...
                    * }
                    */
                string incomingString = System.Text.ASCIIEncoding.ASCII.GetString(connection.Buffer, 0, bytesRead); 
                Debug.WriteLine(incomingString);

                HandleSocketMessage(incomingString);
                     
                connection.Socket.BeginReceive(connection.Buffer, 0, 
                    connection.Buffer.Length, SocketFlags.None, 
                    new AsyncCallback(ReceiveCallback), connection);
            }
            else CloseConnection(connection);

        }

        private void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (_connections) _connections.Remove(ci);
        }

        public static void HandleSocketMessage(string incomingString) {

                Regex typeRegex = new Regex("\"__type__\"\\s*:\\s*\"(?<messageType>.*)\"");
                Match regexResult = typeRegex.Match(incomingString);
                string messageType = regexResult.Groups["messageType"].Value;

                Regex jobtypeRegex = new Regex("\"__jobType__\"\\s*:\\s*\"(?<jobType>.*)\"");
                Match jobregexResult = jobtypeRegex.Match(incomingString);
                string jobType = jobregexResult.Groups["jobType"].Value;

                
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                if (messageType == "status")
                {
                    TurKitStatus receivedObject = serializer.Deserialize<TurKitStatus>(incomingString);

                    Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                    HITData concernedHIT = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job];//Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job];
                        
                    Debug.WriteLine(receivedObject.hitURL);
                        
                    //if (concernedHIT is ShortnData)
                    if (jobType == "shortn")
                    {
                        Debug.WriteLine("Status update for Shortn");
                        ShortnData shortenData = concernedHIT as ShortnData;
                        shortenData.updateStatus(receivedObject);
                    }
                    //else if (concernedHIT is CrowdproofData)
                    else if (jobType == "crowdproof")
                    {
                        CrowdproofData crowdproofData = concernedHIT as CrowdproofData;
                        crowdproofData.updateStatus(receivedObject);
                    }
                    if (jobType == "human-macro")
                    {
                        Debug.WriteLine("Status update for human-macro");
                        HumanMacroData humanMacro = concernedHIT as HumanMacroData;
                        humanMacro.updateStatus(receivedObject);
                    }
                }
                else if (messageType == "stageComplete")
                {
                    Debug.WriteLine("Stage complete message");
                    TurKitStageComplete receivedObject = serializer.Deserialize<TurKitStageComplete>(incomingString);

                    if (jobType == "shortn")
                    {
                        Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                        ShortnData shortenData = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job] as ShortnData;
                        //Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job] as ShortnData;
                        shortenData.stageCompleted(receivedObject);
                    }
                    else if (jobType == "crowdproof")
                    {
                        Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                        CrowdproofData crowdproofData = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job] as CrowdproofData;
                        //CrowdproofData crowdproofData = fixthis;//Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job] as CrowdproofData;
                        crowdproofData.stageCompleted(receivedObject);
                    }
                }
                else if (messageType == "complete")
                {
                    if (jobType == "human-macro")
                    {
                        TurKitHumanMacroResult receivedObject = serializer.Deserialize<TurKitHumanMacroResult>(incomingString);
                        Debug.WriteLine("\nHUMAN MACRO COMPLEEETE******");
                        Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                        HumanMacroData humanMacro = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job] as HumanMacroData;
                        //HumanMacroData humanMacro = fixthis;//Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job] as HumanMacroData;
                        humanMacro.processSocKitMessage(receivedObject);
                    }
                    else if (jobType == "shortn")
                    {
                        TurKitFindFixVerify receivedObject = serializer.Deserialize<TurKitFindFixVerify>(incomingString);
                        Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                        ShortnData shortenData = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job] as ShortnData;//Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job] as ShortnData;
                        shortenData.processSocKitMessage(receivedObject);
                    }
                    else if (jobType == "crowdproof")
                    {
                        TurKitFindFixVerify receivedObject = serializer.Deserialize<TurKitFindFixVerify>(incomingString);
                        Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                        CrowdproofData crowdproofData = Globals.Soylent.soylentMap[doc].jobMap[receivedObject.job] as CrowdproofData;
                        //CrowdproofData crowdproofData = fixthis;//Globals.Soylent.soylentMap[Globals.Soylent.Application.ActiveDocument].jobMap[receivedObject.job] as CrowdproofData;
                        crowdproofData.processSocKitMessage(receivedObject);
                    }
                }
                else if (messageType == "exception")
                {
                    Debug.WriteLine("TurKit exception thrown:");
                    TurKitException receivedObject = serializer.Deserialize<TurKitException>(incomingString);
                    Debug.WriteLine(receivedObject.exceptionString);

                    Microsoft.Office.Interop.Word.Document doc = Globals.Soylent.jobToDoc[receivedObject.job];
                    SoylentPanel panel = Globals.Soylent.soylentMap[doc];
                    HITData concernedHIT = panel.jobMap[receivedObject.job];

                    panel.Invoke(new HITData.showErrorDelegate(concernedHIT.showError), new object[] { receivedObject.exceptionString });
                    //concernedHIT.showError(receivedObject.exceptionCode);

                }
                //Debug.WriteLine("got it!");
        }

        /// <summary>
        /// High-level status report from TurKit
        /// </summary>
        public class TurKitStatus
        {
            public int job;
            public string stage;
            public int numCompleted;
            public int totalRequested;
            public int paragraph;
            public double payment;
            public string hitURL;
            public int patchNumber;
            public int totalPatches;
        }

        /// <summary>
        /// Signals that a stage for a specific job, paragraph, and patch has completed
        /// </summary>
        public class TurKitStageComplete
        {
            public int job;
            public string stage;
            public int totalRequested;
            public double payment;
            public int paragraph;
            public int patchNumber;
            public int totalPatches;
        }

        public class TurKitHumanMacroResult
        {
            public int job;
            public int input;
            public List<string> alternatives;
        }

        /// <summary>
        /// Data returned from a Find-Fix-Verify task
        /// </summary>
        public class TurKitFindFixVerify
        {
            public int job;
            public int paragraph;
            public List<TurKitFindFixVerifyPatch> patches;
        }

        /// <summary>
        /// Data returning a patch from a Shortn task.
        /// </summary>
        public class TurKitFindFixVerifyPatch
        {
            public int start;
            public int end;
            public int editStart;
            public int editEnd;
            public List<TurKitFindFixVerifyOption> options;
            public int numEditors;
            public bool merged;
            public string originalText;
        }


        /// <summary>
        /// Data returning an option for a specific patch
        /// </summary>
        public class TurKitFindFixVerifyOption
        {
            public string field;
            public List<TurKitFindFixVerifyAlternative> alternatives;
            public bool editsText;
        }

        public class TurKitFindFixVerifyAlternative
        {
            public string text;
            [XmlIgnore] public Dictionary<string, int> votes;
            public string editedText;
            public int editStart;
            public int editEnd;
            public int numVoters;
        }

        /// <summary>
        /// Data returned from a Shortn task
        /// </summary>
        public class TurKitShortn
        {
            public int job;
            public int paragraph;
            public List<TurKitShortnPatch> patches;
        }

        /// <summary>
        /// Data returning a patch from a Shortn task.
        /// </summary>
        public class TurKitShortnPatch
        {
            public int start;
            public int end;
            public int editStart;
            public int editEnd;
            public int numEditors;
            public bool merged;
            public bool canCut;
            public int cutVotes;
            public List<TurKitShortnPatchOption> options;
            public string originalText;
        }

        /// <summary>
        /// Data returning an option for a specific patch
        /// </summary>
        public class TurKitShortnPatchOption
        {
            public string text;
            public string editedText;
            public int editStart;
            public int editEnd;
            public int meaningVotes;
            public int grammarVotes;
            public int numVoters;
        }

        public class TurKitCrowdproof
        {
            public int job;
            public int paragraph;
            public List<TurKitCrowdproofPatch> patches;
        }

        public class TurKitCrowdproofPatch
        {
            public int start;
            public int end;
            public int editStart;
            public int editEnd;
            public int numEditors;
            public List<TurKitCrowdproofPatchOption> options;
            public List<string> reasons;
            public string originalText;
        }

        public class TurKitCrowdproofPatchOption
        {
            public string text;
            public int editStart;
            public int editEnd;
            public string replacement;
            //TODO: does this make sense?  multiple reasons?
        }

        public class TurKitException
        {
            public int job;
            public string exceptionString;
        }

    }
}
