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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServiceCore _service;
        public MainWindow()
        {
            InitializeComponent();
            _service = new ServiceCore();
            _service.PacketReceived += _service_PacketReceived;
        }

        void _service_PacketReceived(Packet p, IPEndPoint endPoint)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                this.MessageDisplay.AppendText(string.Format("Received from {0}:{1}\n", endPoint.Address, endPoint.Port));
                this.MessageDisplay.AppendText(p.ToString());
                this.MessageDisplay.AppendText("\n\n");
                this.MessageDisplay.ScrollToEnd();
            }));
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


        private void Button_Start(object sender, RoutedEventArgs e)
        {
            _service.Start();
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
        }

        private void Button_Save(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Send(object sender, RoutedEventArgs e)
        {
            var wnd = new CreatePacketWindow();
            var result = wnd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var packet = new Packet();
                packet.IsQuery = true;
                packet.Queries.Add(new Query()
                {
                    IsMulticast = true,
                    Record = new Record()
                    {
                        Name = wnd.RequestName,
                        Class = 1,
                        RecordType = (UInt16)wnd.SelectedQueryType,
                    },
                });
                this._service.SendPacket(packet);
            }
        }
    }
}
