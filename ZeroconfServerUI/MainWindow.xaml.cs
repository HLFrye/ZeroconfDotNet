using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using ZeroconfDotNet.DNS;

namespace ZeroconfServerUI
{
    delegate void RequestReceivedDelegate(Packet request, IPEndPoint endPoint);
    interface IMDNSService
    {
        void Start();
        void Stop();
        event RequestReceivedDelegate RequestReceived;
    }

    class MDNSService : IMDNSService
    {
        readonly Thread _thread;

        public MDNSService()
        {
            _thread = new Thread(new ThreadStart(ServiceMethod));
            _thread.Name = "MDNS Service Thread";
            _thread.IsBackground = true;
        }

        public void ServiceMethod()
        {
            var client = new UdpClient();
            client.Client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Parse("192.168.16.1"), 5353));

            client.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));

            SendTestRequest(client);

            IPEndPoint remoteEndPoint = null;
            while (true)
            {
                var received = client.Receive(ref remoteEndPoint);
                try
                {
                    RequestReceived(PacketReader.Read(received), remoteEndPoint);
                }
                catch (Exception)
                {}
            }
        }

        public void SendTestRequest(UdpClient client)
        {
            //var packet = new DNSPacket();
            //packet.Add(new DNSQuery("_pubtest._tcp", 12, 1));
            //var data = packet.Serialize();
            //client.Send(data, data.Length, new IPEndPoint(IPAddress.Parse("224.0.0.241"), 5353));
        }

        public void Start()
        {
            if (!_thread.IsAlive)
            {
                _thread.Start();   
            }
        }

        public void Stop()
        {
            if (_thread.IsAlive)
            {
                _thread.Abort();
            }
        }

        public event RequestReceivedDelegate RequestReceived = delegate { };
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IMDNSService _service;
        public MainWindow()
        {
            InitializeComponent();
            _service = new MDNSService();
            _service.RequestReceived += _service_RequestReceived;
        }

        string CreateString(byte[] request)
        {
            var sb = new StringBuilder();
            sb.Append(string.Format("Received {0} bytes:\n", request.Length));
            for (var i = 0; i < request.Length; ++i)
            {
                var b = request[i];
                sb.Append(string.Format("{0:X} ", b));
                if (i % 8 == 7)
                {
                    sb.Append("\n");
                }
            }
            sb.Append('-', 24);
            return sb.ToString();
        }

        void _service_RequestReceived(Packet request, IPEndPoint endPoint)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                this.MessageDisplay.AppendText(string.Format("Received from {0}:{1}\n", endPoint.Address, endPoint.Port));
                this.MessageDisplay.AppendText(string.Format("Questions = {0}\n", request.Questions));
                foreach (var i in request.Queries)
                {
                    this.MessageDisplay.AppendText(string.Format("Request for {0}\n", string.Join(".", i.Record.Name)));
                    this.MessageDisplay.AppendText(string.Format("Request type {0}\n", i.Record.RecordType));
                }
                this.MessageDisplay.ScrollToEnd();
            }));
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            _service.Start();
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            _service.Stop();
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {

        }
    }
}
