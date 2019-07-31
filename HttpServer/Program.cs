using HttpUtilities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

namespace HttpServer
{
    class Program
    {
        static Socket serverSocket;
        const int MESSAGESIZE = 1024 * 1024;
        const string PATH = "D:";
        static void Main(string[] args)
        {
            string hostName = Dns.GetHostName();

            IPAddress iPAddress = Dns.GetHostEntry(hostName).AddressList[1];

            int portNumber = 3000;

            Console.WriteLine("IP Address : " + iPAddress.ToString());
            Console.WriteLine("Port Number : " + portNumber);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNumber);

            serverSocket.Bind(ipEndPoint);

            serverSocket.Listen(100);

            Console.WriteLine("Server has started");

            Thread listenerThread = new Thread(new ThreadStart(Listener));
            //listenerThread.IsBackground = false;
            listenerThread.Start();

            //Console.Read();

        }
        static void Listener()
        {
            Socket client = serverSocket.Accept();

            Byte[] buffName = new Byte[MESSAGESIZE];

            HttpRequestParser httpRequest;

            try
            {
                int nBuffSize = client.Receive(buffName);

                String requestMessage = Encoding.ASCII.GetString(buffName, 0, nBuffSize);

                Console.WriteLine(requestMessage);

                HttpRequestParser.TryParse(requestMessage,out httpRequest);

                HttpResponse httpResponse = GetHttpResponse(httpRequest);

                //string responseMessage = GetHttpResponseString(httpResponse);

                //string responseMessage = File.ReadAllText("D:/ResponseFile.txt");

                Console.WriteLine(Encoding.UTF8.GetString(GetHttpResponseByte(httpResponse)));

                //client.Send(Encoding.UTF8.GetBytes(responseMessage));
                client.Send(GetHttpResponseByte(httpResponse));

            }
            catch
            {
                client.Close();
            }

            Thread listenerThread = new Thread(new ThreadStart(Listener));
            listenerThread.Start();
        }
        static void Receiver(Socket client)
        {
            
        }
        static HttpResponse GetHttpResponse(HttpRequestParser httpRequest)
        {
            //string response;
            HttpResponse httpResponse = new HttpResponse();
            if (httpRequest.RequestURI == "/")
            {
                httpResponse.Body = File.ReadAllBytes(PATH + "/index.html");
            }
            else
            { 
                httpResponse.Body = File.ReadAllBytes(PATH + httpRequest.RequestURI);      
           }
            if(httpRequest.RequestURI.Contains('.'))
            {
                string extension = "." + httpRequest.RequestURI.Split('.')[1]; 
                if (HttpResponse.ExtensionContentTypeMapper.ContainsKey(extension))
                {
                    httpResponse.ResponseHeaders["Content-Type"] = HttpResponse.ExtensionContentTypeMapper[extension];
                }
            }
            httpResponse.HTTPversion = httpRequest.HTTPversion;
            httpResponse.ReasonPhrase = "OK";
            httpResponse.StatusCode = "200";
            //httpResponse.ResponseHeaders["Content-Type"] = "text/html";
            httpResponse.ResponseHeaders["Content-Length"] = httpResponse.Body.Length.ToString();

            return httpResponse;

        }
        static byte[] GetHttpResponseByte(HttpResponse httpResponse)
        {
            Byte[] response = new Byte[MESSAGESIZE];
            string header = String.Empty;
            header += httpResponse.HTTPversion + " ";
            header += httpResponse.StatusCode + " ";
            header += httpResponse.ReasonPhrase + " ";

            foreach(var responseHeaderName in httpResponse.ResponseHeaders.Keys)
                header += "\n" + responseHeaderName + ":" + httpResponse.ResponseHeaders[responseHeaderName];

            header = header.Replace("\r", "");
            header += "\n\n";

            List<byte> responseList = new List<byte>();

            responseList.AddRange(Encoding.ASCII.GetBytes(header));
            responseList.AddRange(httpResponse.Body);

            response = responseList.ToArray();
            
            return response;
        }
    }
}
