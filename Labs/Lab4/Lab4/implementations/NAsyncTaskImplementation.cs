using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lab4.domain;
using Lab4.utils;

namespace Lab4.implementations
{
    public class NAsyncTaskImplementation : TaskImplementation
    {
        private static List<string> hosts;

        protected override void StartClient(string host, int id)
        {
            var ipHostInfo = Dns.GetHostEntry(host.Split('/')[0]);
            var ipAddr = ipHostInfo.AddressList[0];
            var remEndPoint = new IPEndPoint(ipAddr, Parser.PORT);

            var client = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            var requestSocket = new CustomSocket
            {
                CommunicationSocket = client,
                HostName = host.Split('/')[0],
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/", StringComparison.Ordinal)) : "/",
                RemoteEndPoint = remEndPoint,
                Id = id
            };

            Connect(requestSocket).Wait(); // connect to remote server
            Send(requestSocket, Parser.GetRequestString(requestSocket.HostName, requestSocket.Endpoint))
                .Wait(); // request data from server
            Receive(requestSocket).Wait(); // receive server response

            Console.WriteLine("Connection {0}: Content length is:{1}", requestSocket.Id,
                Parser.GetContentLength(requestSocket.ResponseContent.ToString()));

            // release the socket
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static Task Connect(CustomSocket state)
        {
            state.CommunicationSocket.BeginConnect(state.RemoteEndPoint, ConnectCallback, state);

            return Task.FromResult(state.ConnectionFinished.WaitOne()); // block until signaled
        }

        private static Task Send(CustomSocket state, string data)
        {
            // convert the string data to byte data using ASCII encoding.  
            var byteData = Encoding.ASCII.GetBytes(data);

            // send data  
            state.CommunicationSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, state);

            return Task.FromResult(state.SendFinished.WaitOne());
        }

        private static Task Receive(CustomSocket state)
        {
            // receive data
            state.CommunicationSocket.BeginReceive(state.Buffer, 0, CustomSocket.BufferSize, 0, ReceiveCallback, state);

            return Task.FromResult(state.ReceiveFinished.WaitOne());
        }
    }
}