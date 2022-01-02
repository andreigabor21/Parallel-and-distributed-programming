using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using lab4.domain;
using lab4.utils;

namespace lab4.impl
{
    class TaskSession
    {
        private static List<string> hostList;

        public static void Run(List<string> hostnames, bool toBeAsync)
        {
            hostList = hostnames;
            var tasks = new List<Task>();

            for (var i = 0; i < hostnames.Count; i++)
            {
                if (toBeAsync)
                {
                    tasks.Add(Task.Factory.StartNew(DoStartAsync, i));
                }
                else
                {
                    tasks.Add(Task.Factory.StartNew(DoStart, i));
                }
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static void DoStartAsync(object idObject)
        {
            var id = (int)idObject;

            StartAsyncClient(hostList[id], id);
        }

        private static void DoStart(object idObject)
        {
            var id = (int)idObject;

            StartClient(hostList[id], id);
        }

        private static void StartClient(string host, int id)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host.Split('/')[0]); 
            IPAddress ipAddres = ipHostInfo.AddressList[0];
            IPEndPoint remEndPoint = new IPEndPoint(ipAddres, Parser.Port); 

            Socket clientSocket = new Socket(ipAddres.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CustomSocket requestSocket = new CustomSocket
            {
                ConnectionSocket = clientSocket,
                Hostname = host.Split('/')[0],
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/", StringComparison.Ordinal)) : "/",
                RemoteEndPoint = remEndPoint,
                Id = id
            }; 

            Connect(requestSocket).Wait();
            Send(requestSocket, Parser.GetRequestString(requestSocket.Hostname, requestSocket.Endpoint))
                .Wait();
            Receive(requestSocket).Wait();

            Console.WriteLine("Connection {0} > Content length is:{1}", requestSocket.Id, Parser.GetContentLength(requestSocket.ResponseContent.ToString()));

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private static async void StartAsyncClient(string host, int id)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(host.Split('/')[0]);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEndpoint = new IPEndPoint(ipAddress, Parser.Port);

            // create the TCP socket
            Socket clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            CustomSocket requestSocket = new CustomSocket
            {
                ConnectionSocket = clientSocket,
                Hostname = host.Split('/')[0],
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/", StringComparison.Ordinal)) : "/",
                RemoteEndPoint = remoteEndpoint,
                Id = id
            }; // state object

            await ConnectAsync(requestSocket); 

            await SendAsync(requestSocket,
                Parser.GetRequestString(requestSocket.Hostname, requestSocket.Endpoint)); 

            await ReceiveAsync(requestSocket);

            Console.WriteLine("Connection {0} -> Content length is:{1}", requestSocket.Id, Parser.GetContentLength(requestSocket.ResponseContent.ToString()));

            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        private static async Task ConnectAsync(CustomSocket state)
        {
            state.ConnectionSocket.BeginConnect(state.RemoteEndPoint, ConnectCallback, state);

            // block task until signaled
            await Task.FromResult<object>(state.ConnectionFinished.WaitOne());
        }

        private static Task Connect(CustomSocket state)
        {
            state.ConnectionSocket.BeginConnect(state.RemoteEndPoint, ConnectCallback, state);

            // block until signaled
            return Task.FromResult(state.ConnectionFinished.WaitOne());
        }

        private static void ConnectCallback(IAsyncResult asyncResult)
        {
            CustomSocket resultSocket = (CustomSocket)asyncResult.AsyncState;
            Socket clientSocket = resultSocket.ConnectionSocket;
            int clientId = resultSocket.Id;
            String hostname = resultSocket.Hostname;

            clientSocket.EndConnect(asyncResult);

            Console.WriteLine("Connection {0} -> Socket connected to {1} ({2})", clientId, hostname, clientSocket.RemoteEndPoint);

            // signal connection is up
            resultSocket.ConnectionFinished.Set();
        }

        private static async Task SendAsync(CustomSocket state, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);

            // send data
            state.ConnectionSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, state);

            await Task.FromResult<object>(state.SendFinished.WaitOne());
        }
        
        private static Task Send(CustomSocket state, string data)
        {
            // convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes(data);

            // send data  
            state.ConnectionSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, state);

            return Task.FromResult(state.SendFinished.WaitOne());
        }

        private static void SendCallback(IAsyncResult ar)
        {
            CustomSocket resultSocket = (CustomSocket)ar.AsyncState;
            Socket clientSocket = resultSocket.ConnectionSocket;
            int clientId = resultSocket.Id;

            int bytesSent = clientSocket.EndSend(ar); // complete sending the data to the server  

            Console.WriteLine("Connection {0} -> Sent {1} bytes to server.", clientId, bytesSent);

            resultSocket.SendFinished.Set(); // signal that all bytes have been sent
        }

        private static async Task ReceiveAsync(CustomSocket state)
        {
            // receive data
            state.ConnectionSocket.BeginReceive(state.Buffer, 0, CustomSocket.BufferSize, 0, ReceiveCallback, state);

            await Task.FromResult<object>(state.ReceiveFinished.WaitOne());
        }

        private static Task Receive(CustomSocket state)
        {
            // receive data
            state.ConnectionSocket.BeginReceive(state.Buffer, 0, CustomSocket.BufferSize, 0, ReceiveCallback, state);

            return Task.FromResult(state.ReceiveFinished.WaitOne());
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            // retrieve the details from the connection information wrapper
            CustomSocket resultSocket = (CustomSocket)ar.AsyncState;
            Socket clientSocket = resultSocket.ConnectionSocket;

            try
            {
                // read data from the remote device.  
                int bytesRead = clientSocket.EndReceive(ar);

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