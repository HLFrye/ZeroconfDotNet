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

namespace ZeroconfWatcher
{
    /// <summary>
    /// Interaction logic for NetworkTabControl.xaml
    /// </summary>
    public partial class NetworkTabControl : UserControl
    {
        public NetworkTabControl()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as NetworkTab;
            vm.AddSearch(searchInput.Text);
            searchInput.Text = "";
        }
    }
}
