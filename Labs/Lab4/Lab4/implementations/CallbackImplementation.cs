using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Lab4.domain;
using Lab4.utils;

namespace Lab4.implementations
{
    public class CallbackImplementation
    {
        public void Run(List<String> hostList)
        {
            for (int i = 0; i < hostList.Count; i++)
            {
                StartClient(hostList[i], i);
                Thread.Sleep(50);
            }
        }

        private static void StartClient(String host, int id)
        {
            String webSite = host.Split('/')[0];
            
            //get object containing the ip and the port from link
            IPHostEntry ipHostInfo = Dns.GetHostEntry(webSite);
            //get IP Address from ipHostEntry
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            
            //create C# object containing IP + Port
            IPEndPoint remoteEndpoint = new IPEndPoint(ipAddress, Parser.PORT); 
            
            //create TCP socket
            Socket clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); 

            CustomSocket requestSocket = new CustomSocket()
            {
                CommunicationSocket = clientSocket,
                HostName = webSite,
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/")) : "/",
                RemoteEndPoint = remoteEndpoint,
                Id = id
            };

            //connect to the endpoint; after connection, the OnConnected() function will be called
            requestSocket.CommunicationSocket.BeginConnect(requestSocket.RemoteEndPoint, OnConnected, requestSocket);
        }

        private static void OnConnected(IAsyncResult asyncResult)
        {
            CustomSocket resultSocket = (CustomSocket)asyncResult.AsyncState;
            Socket clientSocket = resultSocket.CommunicationSocket;
            int clientId = resultSocket.Id;
            String hostname = resultSocket.HostName;

            clientSocket.EndConnect(asyncResult);
            Console.WriteLine("Connection {0} -> Socket connected to {1} ({2})", clientId, hostname, clientSocket.RemoteEndPoint);

            //request to send to the server
            String requestString = Parser.GetRequestString(resultSocket.HostName, resultSocket.Endpoint);
            // Console.WriteLine(requestString);
            byte[] byteData = Encoding.ASCII.GetBytes(requestString);

            //the OnSent() function will be called 
            resultSocket.CommunicationSocket
                .BeginSend(byteData, 0, byteData.Length, 0, OnSent, resultSocket);
        }

        private static void OnSent(IAsyncResult asyncResult)
        {
            CustomSocket resultSocket = (CustomSocket)asyncResult.AsyncState;
            Socket clientSocket = resultSocket.CommunicationSocket;
            int clientId = resultSocket.Id;
            
            //send data to server
            int bytesSent = clientSocket.EndSend(asyncResult);
            Console.WriteLine("Connection {0} -> Sent {1} bytes to server", clientId, bytesSent);

            //the OnReceiving() function will be called
            resultSocket.CommunicationSocket
                .BeginReceive(resultSocket.Buffer, 0, CustomSocket.BufferSize, 0, OnReceiving, resultSocket);
        }

        private static void OnReceiving(IAsyncResult asyncResult)
        {
            //get received content
            CustomSocket resultSocket = (CustomSocket)asyncResult.AsyncState;
            Socket clientSocket = resultSocket.CommunicationSocket;

            try
            {
                int bytesRead = clientSocket.EndReceive(asyncResult); //read response data
                string value = Encoding.ASCII.GetString(resultSocket.Buffer, 0, bytesRead);
                resultSocket.ResponseContent.Append(value);

                //if the response header has not been fully obtained, get the next chunk of data
                if (!Parser.ResponseHeaderObtained(resultSocket.ResponseContent.ToString()))
                {
                    clientSocket.BeginReceive(resultSocket.Buffer, 0, CustomSocket.BufferSize, 0, OnReceiving,
                        resultSocket);
                }
                else
                {
                    Console.WriteLine("Content length: {0}",
                        Parser.GetContentLength(resultSocket.ResponseContent.ToString()));
                    Console.WriteLine(resultSocket.ResponseContent.ToString());

                    clientSocket.Shutdown(SocketShutdown.Both); //free socket
                    clientSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}