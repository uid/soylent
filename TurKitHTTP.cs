using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;

namespace Soylent
{
    class TurKitHTTP
    {
        private int PORT = 11000;
        private Thread serverThread;

        public TurKitHTTP()
        {
            serverThread = new Thread(ListenLoop);
        }

        /// <summary>
        /// Connects to a socket on the local machine and begins listening on the socket.
        /// </summary>
        public void Listen()
        {
            serverThread.Start();
        }
        
        private void ListenLoop() 
        {
            string[] prefixes = { "http://localhost:" + PORT + "/" };

            if (!HttpListener.IsSupported)
            {
                Debug.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
                return;
            }
            // URI prefixes are required,
            // for example "http://contoso.com:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // Create a listener.
            HttpListener listener = new HttpListener();
            // Add the prefixes.
            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            Debug.WriteLine("Listening...");

            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request. 
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                HandleRequestData(request);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = "received by c#";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
                
            }
        }

        public static void HandleRequestData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                Debug.WriteLine("No client data was sent with the request.");
                return;
            }
            System.IO.Stream body = request.InputStream;
            System.Text.Encoding encoding = request.ContentEncoding;
            System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
            if (request.ContentType != null)
            {
                Debug.WriteLine("Client data content type " + request.ContentType);
            }
            Debug.WriteLine("Client data content length " + request.ContentLength64);

            // Convert the data to a string and display it on the Debug.
            string message = reader.ReadToEnd();
            Debug.WriteLine(message);

            TurKitSocKit.HandleSocketMessage(message);

            body.Close();
            reader.Close();
            // If you are finished with the request, it should be closed also.
        }
    }
}
