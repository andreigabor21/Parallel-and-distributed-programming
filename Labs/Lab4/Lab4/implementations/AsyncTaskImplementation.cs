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
    public class AsyncTaskImplementation : TaskImplementation
    {
        private static List<string> hosts;

        protected override async void StartClient(string host, int id)
        {
            var ipHostInfo = Dns.GetHostEntry(host.Split('/')[0]);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEndpoint = new IPEndPoint(ipAddress, Parser.PORT);

            var client =
                new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp); // create client socket

            var requestSocket = new CustomSocket
            {
                CommunicationSocket = client,
                HostName = host.Split('/')[0],
                Endpoint = host.Contains("/") ? host.Substring(host.IndexOf("/", StringComparison.Ordinal)) : "/",
                RemoteEndPoint = remoteEndpoint,
                Id = id
            }; // state object

            await Connect(requestSocket); // connect to remote server

            await Send(requestSocket,
                Parser.GetRequestString(requestSocket.HostName,
                    requestSocket.Endpoint)); // request data from the server

            await Receive(requestSocket); // receive server response

            Console.WriteLine("Connection {0}: Content length is:{1}", requestSocket.Id,
                Parser.GetContentLength(requestSocket.ResponseContent.ToString()));

            // release the socket
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static async Task Connect(CustomSocket state)
        {
            state.CommunicationSocket.BeginConnect(state.RemoteEndPoint, ConnectCallback, state);

            await Task.FromResult<object>(state.ConnectionFinished.WaitOne()); // block until signaled
        }

        private static async Task Send(CustomSocket state, string data)
        {
            var byteData = Encoding.ASCII.GetBytes(data);

            // send data
            state.CommunicationSocket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, state);

            await Task.FromResult<object>(state.SendFinished.WaitOne());
        }

        private static async Task Receive(CustomSocket state)
        {
            // receive data
            state.CommunicationSocket.BeginReceive(state.Buffer, 0, CustomSocket.BufferSize, 0, ReceiveCallback, state);

            await Task.FromResult<object>(state.ReceiveFinished.WaitOne());
        }
    }
}
