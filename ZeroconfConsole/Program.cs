using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;

namespace ZeroconfConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var ip in nic.GetIPProperties().UnicastAddresses)
                {
                    Console.WriteLine("Address: {1} {0}", ip.Address, ip.Address.AddressFamily);
                }
            }
            Console.ReadLine();
        }
    }
}
