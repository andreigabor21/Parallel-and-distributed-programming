using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lab4.domain
{
    class CustomSocket
    { 
        public Socket ConnectionSocket = null;
        
        public int Id;
        public string Hostname; 
        public string Endpoint;

        public readonly byte[] Buffer = new byte[BufferSize];
        
        public const int BufferSize = 1024; // 1024 bytes 
        
        public readonly StringBuilder ResponseContent = new ();

        public IPEndPoint RemoteEndPoint; 

        public readonly ManualResetEvent ConnectionFinished = new ManualResetEvent(false);
        public readonly ManualResetEvent SendFinished = new ManualResetEvent(false);
        public readonly ManualResetEvent ReceiveFinished = new ManualResetEvent(false);
    }
}