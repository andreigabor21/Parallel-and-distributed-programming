using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Lab4.domain
{
    public class CustomSocket
    {
        public Socket CommunicationSocket = null;

        public const int BufferSize = 1024;

        public readonly byte[] Buffer = new byte[BufferSize];

        public readonly StringBuilder ResponseContent = new ();

        public int Id;
        public string HostName;
        public string Endpoint;

        public IPEndPoint RemoteEndPoint; //ip of website (address + port)
        
        public readonly ManualResetEvent ConnectionFinished = new ManualResetEvent(false);
        public readonly ManualResetEvent SendFinished = new ManualResetEvent(false);
        public readonly ManualResetEvent ReceiveFinished = new ManualResetEvent(false);
    }
}