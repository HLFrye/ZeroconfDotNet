using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZeroconfPublisher
{
    /// <summary>
    /// Interaction logic for PublishControl.xaml
    /// </summary>
    public partial class PublishControl : UserControl
    {
        public PublishControl()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as NetworkTabVM;
            var service = new ServiceInfoVM();
            var wnd = new GetServiceInfoWindow() { DataContext = service };
            var result = wnd.ShowDialog();
            if (result ?? false)
                vm.AddService(service.Name, service.GetInfo());
        }
    }
}
