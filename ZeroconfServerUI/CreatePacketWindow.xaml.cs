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
using System.Windows.Shapes;

namespace ZeroconfServerUI
{
    /// <summary>
    /// Interaction logic for CreatePacketWindow.xaml
    /// </summary>
    public partial class CreatePacketWindow : Window
    {
        public CreatePacketWindow()
        {
            InitializeComponent();
        }

        public int SelectedQueryType
        {
            get
            {
                var queryType = (ComboBoxItem)reqBox.SelectedItem;
                var tag = (string)queryType.Tag;
                return Int32.Parse(tag);
            }
        }

        public string RequestName
        {
            get
            {
                return nameInput.Text;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
