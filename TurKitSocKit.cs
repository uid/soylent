using System;
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

            try
            {
                serverSocket.Bind(localEP);
                serverSocket.Listen(10);

                Debug.WriteLine("Waiting for a connection...");
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), serverSocket);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }

            Console.WriteLine("Closing the listener...");
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
                connection.Socket = s.EndAccept(result);
                connection.Buffer = new byte[10000];
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
            Console.WriteLine("Receiving data");
            ConnectionInfo connection = (ConnectionInfo)result.AsyncState;
            /*try
            {*/
                int bytesRead = connection.Socket.EndReceive(result);
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
                        
                        HITData concernedHIT = Globals.Soylent.soylent.jobMap[receivedObject.job];
                        
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
                    }
                    else if (messageType == "stageComplete")
                    {
                        Debug.WriteLine("Stage complete message");
                        TurKitStageComplete receivedObject = serializer.Deserialize<TurKitStageComplete>(incomingString);

                        if (jobType == "shortn")
                        {
                            ShortnData shortenData = Globals.Soylent.soylent.jobMap[receivedObject.job] as ShortnData;
                            shortenData.stageCompleted(receivedObject);
                        }
                        else if (jobType == "crowdproof")
                        {
                            CrowdproofData crowdproofData = Globals.Soylent.soylent.jobMap[receivedObject.job] as CrowdproofData;
                            crowdproofData.stageCompleted(receivedObject);
                        }
                    }
                    else if (messageType == "complete")
                    {
                        TurKitFindFixVerify receivedObject = serializer.Deserialize<TurKitFindFixVerify>(incomingString);
                        if (jobType == "shortn")
                        {
                            ShortnData shortenData = Globals.Soylent.soylent.jobMap[receivedObject.job] as ShortnData;
                            shortenData.processSocKitMessage(receivedObject);
                        }
                        else if (jobType == "crowdproof")
                        {
                            CrowdproofData crowdproofData = Globals.Soylent.soylent.jobMap[receivedObject.job] as CrowdproofData;
                            crowdproofData.processSocKitMessage(receivedObject);
                        }
                    }
                    Debug.WriteLine("got it!");
                     
                    connection.Socket.BeginReceive(connection.Buffer, 0, 
                        connection.Buffer.Length, SocketFlags.None, 
                        new AsyncCallback(ReceiveCallback), connection);
                }
                else CloseConnection(connection);
            /*}
catch (SocketException exc)
{
    CloseConnection(connection);
    Console.WriteLine("Socket exception: " + exc.SocketErrorCode);
}
 */
        }

        private void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (_connections) _connections.Remove(ci);
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
            public Dictionary<string, int> votes;
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

    }
}
