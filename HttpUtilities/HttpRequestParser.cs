using System;
using System.Collections.Generic;
using System.Text;

namespace HttpUtilities
{
    public class HttpRequestParser
    {
        public string RequestMethodName { get; set; } = String.Empty;

        public string RequestURI { get; set; } = String.Empty;

        public string HTTPversion { get; set; } = String.Empty;

        public string Body { get; set; } = String.Empty;

        public Dictionary<string, string> RequestHeaders = new Dictionary<string, string>();



        public static bool TryParse(string requestMessage, out HttpRequestParser httpRequest)
        {
            httpRequest = new HttpRequestParser();
            try
            {
                string[] requestMessageHeaders = requestMessage.Split('\n');
                string requestLine = requestMessageHeaders[0];

                httpRequest.RequestMethodName = requestLine.Split(' ')[0];
                httpRequest.RequestURI = requestLine.Split(' ')[1];
                httpRequest.HTTPversion = requestLine.Split(' ')[2];

                int lineCount;

                for (lineCount = 1; lineCount < requestMessageHeaders.Length; lineCount++)
                {
                    if (requestMessageHeaders[lineCount].Length == 0)
                        break;

                    string requestHeader = requestMessageHeaders[lineCount];
                    string requestHeaderName = requestHeader.Split(':')[0];
                    string requestHeaderValue = requestHeader.Split(':')[1];

                    httpRequest.RequestHeaders[requestHeaderName] = requestHeaderValue;
                }

                for (lineCount = lineCount + 1; lineCount < requestMessageHeaders.Length; lineCount++)
                {
                    httpRequest.Body += requestMessageHeaders[lineCount];
                }


            }
            catch
            {
                return false;
            }
            return true;

        }

    }
}
