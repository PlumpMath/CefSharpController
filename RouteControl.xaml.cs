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
        public void htmlElementClicked(string xpath)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                switch (route_type.SelectedIndex)
                {
                    case 0:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 1)
                        {
                            route.SetOutputUrlCollection("Start", new Route.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 2)
                        {//SetProductPages
                            string x = find_product_links_xpath(xpath);
                            route.SetOutputUrlCollection("Start", new Route.UrlCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputUrlCollection("ListNextPage", new Route.UrlCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            route.SetOutputUrlCollection("Product" + (step.SelectedIndex - 5), new Route.UrlCollection { Queue = "Product" + (step.SelectedIndex - 4), Xpath = xpath });
                        }
                        else
                        {//SetProduct
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (dynamic o in w.Attributes.Items)
                                    if (o.Get == true)
                                        route.SetOutputField("Product" + (step.SelectedIndex - 3), new Route.Field { Name = w.Name.Text + "." + o.Attribute, Xpath = xpath, Attribute = o.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case 1:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 1)
                        {
                            route.SetOutputUrlCollection("Start", new Route.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 2)
                        {//SetProductBlocks
                            string x = find_product_blocks_xpath(xpath);
                            route.SetOutputElementCollection("Start", new Route.ElementCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputElementCollection("ListNextPage", new Route.ElementCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        //else if (step.SelectedIndex == step.Items.Count - 1)
                        //{
                        //    route.SetOutputUrl("Product" + (step.SelectedIndex - 5), new Route.Url { Queue = "Product" + (step.SelectedIndex - 4), Xpath = xpath });
                        //}
                        else
                        {
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (dynamic o in w.Attributes.Items)
                                    if (o.Get == true)
                                        route.SetOutputField("Product" + (step.SelectedIndex - 3), new Route.Field { Name = w.Name.Text + "." + o.Attribute, Xpath = xpath, Attribute = o.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    default:
                        throw new Exception("No such option: " + route_type.SelectedIndex);
                }
            }));
        }

        public RouteControl()
        {
            InitializeComponent();

            save.Click += delegate { route.Save(); };
            run.Click += delegate
            {
                Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
                d.DefaultExt = ".xml";
                d.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                if (d.ShowDialog() != true)
                    return;
                Route r = Route.LoadFromFile(d.FileName);
                xml.Text = r.Xml;
                Controller.Start(r);
            };

            route_type.SelectionChanged += delegate
            {
                step.Items.Clear();
                switch (route_type.SelectedIndex)
                {
                    case 0:
                        step.Items.Add("SetSite");
                        step.Items.Add("SetListNextPage");
                        step.Items.Add("SetProductPages");
                        step.Items.Add("SetProduct0");
                        step.Items.Add("SetOneMoreProductPage");
                        break;
                    case 1:
                        step.Items.Add("SetSite");
                        step.Items.Add("SetListNextPage");
                        step.Items.Add("SetProductBlocks");
                        step.Items.Add("SetProduct");
                        break;
                    case 2:
                        step.Items.Add("SetProductPages");
                        step.Items.Add("SetProduct0");
                        step.Items.Add("SetOneMoreProductPage");
                        break;
                    default:
                        throw new Exception("No such option: " + route_type.SelectedIndex);
                }
            };

            step.SelectionChanged += delegate
            {
                switch (route_type.SelectedIndex)
                {
                    case 0:
                        if (step.SelectedIndex == 0)
                        {
                            try
                            {
                                var d = new StartWindow();
                                if (d.ShowDialog() == true)
                                {
                                    route = new CefSharpController.Route(d.XmlName.Text);
                                    string url = d.StartUrl.Text;
                                    route.AddInputItem("Start", new Route.Item { Value = url, Type = Route.Item.Types.URL });
                                    xml.Text = route.Xml;
                                    MainWindow.This.Browser.Load(url, false);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            listen_clicks();
                        }
                        else
                        {
                            listen_clicks();
                        }
                        break;
                    case 1:
                        if (step.SelectedIndex == 0)
                        {
                            try
                            {
                                var d = new StartWindow();
                                if (d.ShowDialog() == true)
                                {
                                    route = new CefSharpController.Route(d.XmlName.Text);
                                    string url = d.StartUrl.Text;
                                    route.AddInputItem("Start", new Route.Item { Value = url, Type = Route.Item.Types.URL });
                                    xml.Text = route.Xml;
                                    MainWindow.This.Browser.Load(url, false);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        }
                        //else if (step.SelectedIndex == 1)
                        //{
                        //    step.Items.Insert(step.SelectedIndex, "SetProduct" + (step.SelectedIndex - 3));
                        //}
                        //else if (step.SelectedIndex == 2)
                        //{
                        //    step.Items.Insert(step.SelectedIndex, "SetProduct" + (step.SelectedIndex - 3));
                        //}
                        else
                        {
                            listen_clicks();
                        }
                        break;
                    default:
                        throw new Exception("No such option: " + route_type.SelectedIndex);
                };
            };

            Loaded += delegate
            {
                //state.SelectedIndex = 0;
            };
        }
        Route route;
        
        void listen_clicks()
        {
            MainWindow.This.Browser.ExecuteJavaScript(
                CefSharpBrowser.Define_getElementsByXPath()
                + CefSharpBrowser.Define_createXPathForElement() + @"
if(!document.__onClick){
            function __onClick(event){
                try{
                  if ( event.preventDefault ) event.preventDefault();
                  if ( event.stopPropagation ) event.stopPropagation();
                  event.returnValue = false;
                  var targetElement = event.target || event.srcElement;
                  var x = document.__createXPathForElement(targetElement);
                  window.JsMapObject.htmlElementClicked(x);
                }catch(err){
                    alert(err.message);
                }
                return false;
            };

            document.__onClick = __onClick;
            document.addEventListener('contextmenu', document.__onClick, false);
};
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