using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using lab4.domain;
using lab4.utils;

namespace lab4.impl
{
    class CallbackSession
    {
    
        public static void Run(List<string> hostnames)
        {
            for (int i = 0; i < hostnames.Count; i++)
            {
                StartClient(hostnames[i], i);
                Thread.Sleep(1000);
            }
        }

        private static void StartClient(string host, int id)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host.Split('/')[0]);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEndpoint = new IPEndPoint(ipAddress, Parser.Port);

            Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CustomSocket requestSocket = new CustomSocket
            {
                ConnectionSocket = client,
                Hostname = host.Split('/')[0],
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/")) : "/",
                RemoteEndPoint = remoteEndpoint,
                Id = id
            }; 

            requestSocket.ConnectionSocket.BeginConnect(requestSocket.RemoteEndPoint, OnConnectionEstablished, requestSocket); // connect to the remote endpoint  
        }

        private static void OnConnectionEstablished(IAsyncResult asyncResult)
        {
            CustomSocket resultSocket = (CustomSocket)asyncResult.AsyncState; // conn state
            Socket clientSocket = resultSocket.ConnectionSocket;
            int clientId = resultSocket.Id;
            String hostname = resultSocket.Hostname;

            clientSocket.EndConnect(asyncResult); // end connection
            Console.WriteLine("Connection {0} -> Socket connected to {1} ({2})", clientId, hostname, clientSocket.RemoteEndPoint);

            byte[] byteData = Encoding.ASCII.GetBytes(Parser.GetRequestString(resultSocket.Hostname, resultSocket.Endpoint));

            resultSocket.ConnectionSocket.BeginSend(byteData, 0, byteData.Length, 0, OnSending, resultSocket);
        }

        private static void OnSending(IAsyncResult ar)
        {
            CustomSocket resultSocket = (CustomSocket)ar.AsyncState;
            Socket clientSocket = resultSocket.ConnectionSocket;
            int clientId = resultSocket.Id;

            int bytesSent = clientSocket.EndSend(ar);
            Console.WriteLine("Connection {0} -> Sent {1} bytes to server.", clientId, bytesSent);

            // Start receiving data from server and store it in buffer
            resultSocket.ConnectionSocket.BeginReceive(resultSocket.Buffer, 0, CustomSocket.BufferSize, 0, OnReceiving, resultSocket);
        }

        private static void OnReceiving(IAsyncResult ar)
        {
            CustomSocket resultSocket = (CustomSocket)ar.AsyncState;
            Socket clientSocket = resultSocket.ConnectionSocket;
            int clientId = resultSocket.Id;

            try
            {
                int bytesRead = clientSocket.EndReceive(ar); // read response data

                resultSocket.ResponseContent.Append(Encoding.ASCII.GetString(resultSocket.Buffer, 0, bytesRead));
                if (!Parser.ResponseHeaderObtained(resultSocket.ResponseContent.ToString()))
                {
                    clientSocket.BeginReceive(resultSocket.Buffer, 0, CustomSocket.BufferSize, 0, OnReceiving, resultSocket);
                }
                else
                {                    
                    Console.WriteLine("Connection {0} -> Content length is: {1}", clientId, Parser.GetContentLength(resultSocket.ResponseContent.ToString()));
                    Console.WriteLine(resultSocket.ResponseContent.ToString());
                    clientSocket.Shutdown(SocketShutdown.Both); // free socket
                    clientSocket.Close();                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}