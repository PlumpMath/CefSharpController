using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        /// <summary>
        /// must start with low-case!!!
        /// </summary>
        /// <param name="xpath"></param>
        public void htmlElementSelected(string xpath)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                switch (route_type)
                {
                    case RouteType.DATA_SEPARATED_FROM_LIST:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 0)
                        {
                            route.SetOutputUrlCollection("Start", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductPages
                            string x = find_product_links_xpath(xpath);
                            route.SetOutputUrlCollection("Start", new Route.OutputUrlCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            route.SetOutputUrlCollection("Product" + (step.SelectedIndex - 5), new Route.OutputUrlCollection { Queue = "Product" + (step.SelectedIndex - 4), Xpath = xpath });
                        }
                        else
                        {//SetProduct
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (dynamic o in w.Attributes.Items)
                                    if (o.Get == true)
                                        route.SetOutputField("Product" + (step.SelectedIndex - 3), new Route.OutputField { Name = w.Name.Text + "." + o.Attribute, Xpath = xpath, Attribute = o.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case RouteType.DATA_IS_IN_LIST:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 0)
                        {
                            route.SetOutputUrlCollection("Start", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductBlocks
                            string x = find_product_blocks_xpath(xpath);
                            route.SetOutputElementCollection("Start", new Route.OutputElementCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputElementCollection("ListNextPage", new Route.OutputElementCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                            base_xpath = xpath;
                        }
                        else
                        {
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (dynamic o in w.Attributes.Items)
                                    if (o.Get == true)
                                        route.SetOutputField("Product0", new Route.OutputField { Name = w.Name.Text + "." + o.Attribute, Xpath = xpath.Substring(base_xpath.Length, xpath.Length - base_xpath.Length), Attribute = o.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    default:
                        throw new Exception("No such option: " + route_type);
                }
            }));
        }
        string base_xpath = null;
        
        public RouteControl()
        {
            InitializeComponent();

            save.Click += delegate {
                Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
                d.FileName = "route"; 
                d.DefaultExt = ".xml";
                d.Filter = "Routes (.xml)|*.xml"; 
                if (d.ShowDialog() == true)
                    route.Save(d.FileName);
            };

            run.Click += delegate
            {
                Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
                d.DefaultExt = ".xml";
                d.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                if (d.ShowDialog() != true)
                    return;
                Route r = new Route(d.FileName);
                xml.Text = r.Xml;
                Controller.Start(r);
            };

            new_.Click += delegate
            {
                try
                {
                    var d = new StartWindow();
                    if (d.ShowDialog() != true)
                        return;
                    route = new CefSharpController.Route();
                    string[] urls = d.StartUrl.Text.Split('\n');
                    foreach (string url in urls)
                    {
                        route.AddInputItem("Start", new Route.InputItem { Value = url.Trim(), Type = Route.InputItem.Types.Url });
                        xml.Text = route.Xml;
                    }
                    MainWindow.This.Browser.Load(urls[0], false);

                    step.Items.Clear();
                    switch (d.RouteType.SelectedIndex)
                    {
                        case 0:
                            route_type = RouteType.DATA_SEPARATED_FROM_LIST;
                            step.Items.Add("SetListNextPage");
                            step.Items.Add("SetProductPages");
                            step.Items.Add("SetProduct0");
                            step.Items.Add("SetOneMoreProductPage");
                            break;
                        case 1:
                            route_type = RouteType.DATA_IS_IN_LIST;
                            step.Items.Add("SetListNextPage");
                            step.Items.Add("SetProductBlocks");
                            step.Items.Add("SetProduct");
                            break;
                        //case 2:
                        //    step.Items.Add("SetProductPages");
                        //    step.Items.Add("SetProduct0");
                        //    step.Items.Add("SetOneMoreProductPage");
                        //    break;
                        default:
                            throw new Exception("No such option: " + d.RouteType.SelectedIndex);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            };

            step.SelectionChanged += delegate
            {
                switch (route_type)
                {
                    case RouteType.DATA_SEPARATED_FROM_LIST:
                        MainWindow.This.Browser.HighlightElementsOnHover();
                        listen_clicks();
                        break;
                    case RouteType.DATA_IS_IN_LIST:
                        MainWindow.This.Browser.HighlightElementsOnHover();
                        listen_clicks();
                        break;
                    default:
                        throw new Exception("No such option: " + route_type);
                };
            };

            Loaded += delegate
            {
                //state.SelectedIndex = 0;
            };
        }
        RouteType route_type;
        enum RouteType
        {
            DATA_SEPARATED_FROM_LIST,
            DATA_IS_IN_LIST
        }
        Route route;
        
        void listen_clicks()
        {
            MainWindow.This.Browser.ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath()
                + CefSharpBrowser.Define__createXPathForElement() + @"
if(document.__onElementSelected)
    return;
document.__onElementSelected = function(event){
    try{
        if ( event.preventDefault ) event.preventDefault();
        if ( event.stopPropagation ) event.stopPropagation();
        event.returnValue = false;
        var target = event.target || event.srcElement;
        if(document.__clickedElement == target)
            document.__selectedElement = document.__selectedElement.parentNode;
        else
            document.__selectedElement = target;
        document.__clickedElement = target;
        var x = document.__createXPathForElement(document.__selectedElement);
        window.JsMapObject.htmlElementSelected(x);
    }catch(err){
        alert(err.message);
    }
    return false;
};

document.addEventListener('contextmenu', document.__onElementSelected, false);
     
");
        }

        string find_product_links_xpath(string xpath)
        {
            var mc = Regex.Matches(xpath, @"\[(\d+)\]");
            Match[] ms = new Match[mc.Count];
            mc.CopyTo(ms, 0);
            int max_count = 0;
            string general_xpath = "";
            for (int i = ms.Length - 1; i >= 0; i--)
            {
                string x = xpath.Remove(ms[i].Groups[1].Index, ms[i].Groups[1].Length);
                x = x.Insert(ms[i].Groups[1].Index, "*");
                int count = (int)MainWindow.This.Browser.ExecuteJavaScript(@"
        var es = document.__getElementsByXPath('" + x + @"');
        return es.length;
");
                if (count > max_count)
                {
                    max_count = count;
                    general_xpath = x;
                }
            }
            return general_xpath;
        }

        string find_product_blocks_xpath(string xpath)
        {
            var mc = Regex.Matches(xpath, @"\[(\d+)\]");
            Match[] ms = new Match[mc.Count];
            mc.CopyTo(ms, 0);
            int max_count = 0;
            string general_xpath = "";
            for (int i = ms.Length - 1; i >= 0; i--)
            {
                string x = xpath.Remove(ms[i].Groups[1].Index, ms[i].Groups[1].Length);
                x = x.Insert(ms[i].Groups[1].Index, "*");
                int count = (int)MainWindow.This.Browser.ExecuteJavaScript(@"
        var es = document.__getElementsByXPath('" + x + @"');
        return es.length;
");
                if (count > max_count)
                {
                    max_count = count;
                    general_xpath = x;
                }
            }
            return general_xpath;
        }
    }
}