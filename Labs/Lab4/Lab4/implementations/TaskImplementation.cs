using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lab4.domain;
using Lab4.utils;

namespace Lab4.implementations
{
    public class TaskImplementation
    {
         private static List<string> hosts;

        public void run(List<string> hostnames)
        {
            hosts = hostnames;
            var tasks = new List<Task>();

            for (var i = 0; i < hostnames.Count; i++)
            {
                tasks.Add(Task.Factory.StartNew(DoStart, i));
            }

            Task.WaitAll(tasks.ToArray());
        }
        
        private void DoStart(object idObject)
        {
            var id = (int)idObject;

            StartClient(hosts[id], id);
        }

        protected virtual void StartClient(string host, int id) {}
        
        protected static void ConnectCallback(IAsyncResult ar)
        {
            // retrieve the details from the connection information wrapper
            var resultSocket = (CustomSocket)ar.AsyncState;
            var clientSocket = resultSocket.CommunicationSocket;
            var clientId = resultSocket.Id;
            var hostname = resultSocket.HostName;

            clientSocket.EndConnect(ar); // complete connection

            Console.WriteLine("Connection {0}: Socket connected to {1} ({2})", clientId, hostname, clientSocket.RemoteEndPoint);

            resultSocket.ConnectionFinished.Set(); // signal connection is up
        }

        protected static void SendCallback(IAsyncResult ar)
        {
            var resultSocket = (CustomSocket)ar.AsyncState;
            var clientSocket = resultSocket.CommunicationSocket;
            var clientId = resultSocket.Id;

            var bytesSent = clientSocket.EndSend(ar); // complete sending the data to the server  
            Console.WriteLine("Connection {0}: Sent {1} bytes to server.", clientId, bytesSent);

            resultSocket.SendFinished.Set(); // signal that all bytes have been sent
        }

        protected static void ReceiveCallback(IAsyncResult ar)
        {
            // retrieve the details from the connection information wrapper
            var resultSocket = (CustomSocket)ar.AsyncState;
            var clientSocket = resultSocket.CommunicationSocket;

            try
            {
                // read data from the remote device.  
                var bytesRead = clientSocket.EndReceive(ar);

                // get from the buffer, a number of characters <= to the buffer size, and store it in the responseContent
                resultSocket.ResponseContent.Append(Encoding.ASCII.GetString(resultSocket.Buffer, 0, bytesRead));

                // if the response header has not been fully obtained, get the next chunk of data
                if (!Parser.ResponseHeaderObtained(resultSocket.ResponseContent.ToString()))
                {
                    clientSocket.BeginReceive(resultSocket.Buffer, 0, CustomSocket.BufferSize, 0, ReceiveCallback, resultSocket);
                }
                else
                {
                    resultSocket.ReceiveFinished.Set(); // signal that all bytes have been received       
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}