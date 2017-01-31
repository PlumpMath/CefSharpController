//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

//namespace Cliver.CefSharpController
//{
//    /// <summary>
//    /// Interaction logic for RouteWindow.xaml
//    /// </summary>
//    public partial class RouteWindow : Window
//    {
//        public RouteWindow()
//        {
//            InitializeComponent();

//            Loaded += delegate
//            {
//                var d = new StartWindow();
//                d.ShowDialog();
//                xml_name = d.XmlName.Text;
//                route = new CefSharpController.Route();
//                MainWindow.Load(d.StartUrl.Text);
//                Message.Inform("Click List Next Page Link");
//                Hide();
//                //route.ProductListNextPageXpath = ;
//                Message.Inform("Click Product Page Link");
//                //route.ProductPagesXpath = ;
//            };
//        }
//        string xml_name;
//        Route route;


//    }
//}
