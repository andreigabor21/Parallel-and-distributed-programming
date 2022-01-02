using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Lab4.utils
{
    public class Parser
    {
        public static int PORT = 80; //http default port

        public static string GetRequestString(string hostname, string endpoint)
        {
            return "GET " + endpoint + " HTTP/1.1\n" +
                   "Host: " + hostname + "\n" +
                   "Content-Length: 0\n";
        }

        public static int GetContentLength(string respContent)
        {
            var contentLen = 0;
            var respLines = respContent.Split('\n');
            foreach (string respLine in respLines)
            {
                var headDetails = respLine.Split(':');
                if (String.Compare(headDetails[0], "Content-Length", StringComparison.Ordinal) == 0)
                {
                    contentLen = int.Parse(headDetails[1]);
                }
            }

            return contentLen;
        }

        public static bool ResponseHeaderObtained(string responseContent)
        {
            return responseContent.Contains("\n");
        }
    }
}