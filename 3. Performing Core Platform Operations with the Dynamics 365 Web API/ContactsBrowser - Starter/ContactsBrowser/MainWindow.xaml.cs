using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ContactsBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
      
        private HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          
        }

        private void listboxContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
 

        }
    }

    public class contact
    {
        public Guid contactid { get; set; }
        public string fullname { get; set; }
        public string lastname { get; set; }
        public string firstname { get; set; }
        public decimal? creditlimit { get; set; }
        public int? numberofchildren { get; set; }
    }

}
