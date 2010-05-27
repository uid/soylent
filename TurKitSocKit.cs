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

        public void Listen() {
            //IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");//Dns.GetHostName());
            //IPEndPoint localEP = new IPEndPoint(ipHostInfo.AddressList[1], port);
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEP = new IPEndPoint(address, port);
            Debug.WriteLine("Local address and port : " + localEP.ToString());
            serverSocket = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.IP);
            //serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

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
            try
            {
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

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    if (messageType == "status")
                    {
                        TurKitStatus receivedObject = serializer.Deserialize<TurKitStatus>(incomingString);
                        //receivedObject.stage = "find";
                        
                        HITData concernedHIT = Globals.Soylent.soylent.jobMap[receivedObject.job];
                        ShortenData shortenData = concernedHIT as ShortenData;
                        //Debug.WriteLine("method: " + receivedObject.stage);
                        //Debug.WriteLine("numCompleted: " + receivedObject.job);
                        if (shortenData != null)
                        {
                            shortenData.updateStatus(receivedObject);
                        }

                        //Debug.WriteLine(receivedObject.numCompleted);
                    }
                    else if (messageType == "shorten")
                    {
                        TurKitShortn receivedObject = serializer.Deserialize<TurKitShortn>(incomingString);
                        //Debug.WriteLine(receivedObject.start + " to " + receivedObject.end);
                        //Debug.WriteLine(receivedObject.paragraph[0]);
                        Debug.WriteLine(receivedObject.options[0]);
                    }
                    Debug.WriteLine("got it!");
                     
                    connection.Socket.BeginReceive(connection.Buffer, 0, 
                        connection.Buffer.Length, SocketFlags.None, 
                        new AsyncCallback(ReceiveCallback), connection);
                }
                else CloseConnection(connection);
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

        private void CloseConnection(ConnectionInfo ci)
        {
            ci.Socket.Close();
            lock (_connections) _connections.Remove(ci);
        }


        public class TurKitStatus
        {
            public int job;
            public string stage;
            public int numCompleted;
            public int totalRequested;
            public int paragraph;
            public double reward;
        }

        public class TurKitShortn
        {
            public int start;
            public int end;
            public List<string> options;
            public List<string> paragraph;
        }
    }
}
