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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for RouteControl.xaml
    /// </summary>
    public partial class RouteControl : UserControl
    {
        public RouteControl()
        {
            InitializeComponent();
        }

        public void RecordRoute()
        {
            var d = new StartWindow();
            d.ShowDialog();
            xml_name = d.XmlName.Text;
            route = new CefSharpController.Route();
            route.ProductListUrl = d.StartUrl.Text;
            xml.Text = route.Xml;
            MainWindow.Load(route.ProductListUrl);
            Message.Inform("Click List Next Page Link");
            //route.ProductListNextPageXpath = ;
            Message.Inform("Click Product Page Link");
            //route.ProductPagesXpath = ;}
        }
        string xml_name;
        Route route;
    }
}
