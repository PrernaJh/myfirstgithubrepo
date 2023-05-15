using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HoneywellScannerTest
{
    class Program
    {
        private static string GetLocalIPAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return localIP;
        }

        public static void TestEndpoint(string ip, int port, string description)
        {
            try
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (!Regex.IsMatch(ip, @"^[0-9.]+$"))
                {
                    foreach (var address in Dns.GetHostAddresses(ip))
                    {
                        ip = address.ToString();
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                            break;
                    }
                }
                var ipAddr = IPAddress.Parse(ip);
                EndPoint ep = new IPEndPoint(ipAddr, port);
                Console.WriteLine($"Connecting to: {ip}:{port} -- {description}");
                s.Connect(ep);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Failed: {ex}");
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"My IP: {GetLocalIPAddress()}");
#if DEBUG
            TestEndpoint("localhost", 4000, "ppg-ring Local");
#endif
            TestEndpoint("52.227.31.232", 80, "ppg-ring Production");
            TestEndpoint("52.227.177.87", 80, "ppg-ring Staging");
        }
    }
}
