using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4.utils
{
    class Parser
    {
        public static int Port = 80;

        public static string GetRequestString(string hostname, string endpoint)
        {
            return "GET " + endpoint + " HTTP/1.1\r\n" +
                   "Host: " + hostname + "\r\n" + 
                   "Content-Length: 0\r\n\r\n";
        }

        public static int GetContentLength(string respContent)
        {
            var contentLength = 0;
            var responseLines = respContent.Split('\r', '\n');
            foreach (string respLine in responseLines)
            {
                var headDetails = respLine.Split(':');

                if (String.Compare(headDetails[0], "Content-Length", StringComparison.Ordinal) == 0)
                {
                    contentLength = int.Parse(headDetails[1]);
                }
            }
            return contentLength;
        }

        public static bool ResponseHeaderObtained(string responseContent)
        {
            return responseContent.Contains("\r\n\r\n");
        }
    }
}