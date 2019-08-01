using HttpUtilities;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpServer
{
    class Program
    {     
        static void Main(string[] args)
        {
            HttpListener.Initialize(100);

            HttpListener.Listen();           
        }       
    }
}
