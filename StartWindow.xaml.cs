using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for StartUrlWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

            ok.Click += delegate
              {
                  DialogResult = true;
                  Close();
              };

            RouteType.SelectionChanged += delegate
            {
                switch (RouteType.SelectedIndex)
                {
                    case 0:
                        StartUrl.Text = "http://boston.craigslist.org/search/ata";
                        break;
                    case 1:
                        StartUrl.Text = "https://www.google.com/search?q=js+get+all+elements";
                        break;
                }
            };
        }
    }
}