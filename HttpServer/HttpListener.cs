using HttpUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HttpServer
{
    internal class HttpListener
    {
        static Socket serverSocket;
        const int portNumber = 3000;

        internal static void Initialize(int backlog=100)
        {
            string hostName = Dns.GetHostName();

            IPAddress iPAddress = Dns.GetHostEntry(hostName).AddressList[1];

            Console.WriteLine("IP Address : " + iPAddress.ToString());
            Console.WriteLine("Port Number : " + portNumber);

            IPEndPoint ipEndPoint = new IPEndPoint(iPAddress, portNumber);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(ipEndPoint);

            serverSocket.Listen(backlog);

            Console.WriteLine("Server has started...");
        }

        internal static void Listen()
        {
            Socket client = serverSocket.Accept();

            //Byte[] buffRequestMessage = new Byte[HttpResponse.REQUESTMESSAGESIZE];
            //int nBuffRequestMessageSize = client.Receive(buffRequestMessage);

            //String requestMessage = Encoding.ASCII.GetString(buffRequestMessage, 0, nBuffRequestMessageSize);

            //Console.WriteLine("\nRequest Message:\n" + requestMessage);

            //Thread sendResponseThread = new Thread(new ThreadStart(() => SendHttpResponse(client, requestMessage)));
            //sendResponseThread.Start();
            //SendHttpResponse(client, requestMessage);

            Thread receiveThread = new Thread(new ThreadStart(() => Receive(client)));
            receiveThread.Start();

            Thread httpListenerThread = new Thread(new ThreadStart(HttpListener.Listen));
            httpListenerThread.Start();
        }
        static void Receive(Socket client)
        {
            try
            {


                while (true)
                {
                    Byte[] buffRequestMessage = new Byte[HttpResponse.REQUESTMESSAGESIZE];
                    int nBuffRequestMessageSize = client.Receive(buffRequestMessage);

                    String requestMessage = Encoding.ASCII.GetString(buffRequestMessage, 0, nBuffRequestMessageSize);

                    Console.WriteLine("\nRequest Message:\n" + requestMessage);

                    SendHttpResponse(client, requestMessage);

                }
            }
            catch
            {
                client.Close();
            }
        }
        static void SendHttpResponse(Socket client, String requestMessage)
        {
            
            
            HttpRequestParser httpRequest;

            if (HttpRequestParser.TryParse(requestMessage, out httpRequest))
            {
                HttpResponse httpResponse;
                httpResponse = HttpResponse.GetHttpResponse(httpRequest);
                Console.WriteLine("\nResponse Message:\n" + Encoding.ASCII.GetString(HttpResponse.ConvertHttpResponsToByte(httpResponse)));
                //client.Send(HttpResponse.ConvertHttpResponsToByte(httpResponse));

                NetworkStream connectionStream = new NetworkStream(client);
                BinaryWriter outStream = new BinaryWriter(connectionStream);
                outStream.Write(HttpResponse.ConvertHttpResponsToByte(httpResponse));
            }
            else
            {
                HttpResponse httpErrorResponse = new HttpResponse();
                httpErrorResponse.HTTPversion = "HTTP/1.1";
                httpErrorResponse.StatusCode = "400";
                httpErrorResponse.ReasonPhrase = "Bad Request";
                httpErrorResponse.Body = File.ReadAllBytes(HttpResponse.PATH + "/badrequest.html");
                client.Send(HttpResponse.ConvertHttpResponsToByte(httpErrorResponse));
            }
        }
    }
}
