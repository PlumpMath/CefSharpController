//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//********************************************************************************************
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
                            route.SetOutput(Queue.StartQueueName, new Route.Output.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutput("ListNextPage", new Route.Output.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductPages
                            string x = find_product_links_xpath(xpath);
                            route.SetOutput(Queue.StartQueueName, new Route.Output.UrlCollection { Queue = "Product0", Xpath = x });
                            route.SetOutput("ListNextPage", new Route.Output.UrlCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            route.SetOutput("Product" + (step.Items.Count - 4), new Route.Output.UrlCollection { Queue = "Product" + (step.Items.Count - 3), Xpath = xpath });
                            step.Items.Insert(step.Items.Count - 1, "SetProduct" + (step.Items.Count - 3));
                        }
                        else
                        {//SetProduct
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (ProductFieldWindow.Item i in w.Items)
                                    if (i.Get)
                                        route.SetOutput("Product" + (step.SelectedIndex - 2), new Route.Output.Field { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath, Attribute = i.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case RouteType.DATA_IS_IN_LIST:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 0)
                        {
                            route.SetOutput(Queue.StartQueueName, new Route.Output.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutput("ListNextPage", new Route.Output.UrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductBlocks
                            string x = find_product_blocks_xpath(xpath);
                            route.SetOutput(Queue.StartQueueName, new Route.Output.ElementCollection { Queue = "Product0", Xpath = x });
                            route.SetOutput("ListNextPage", new Route.Output.ElementCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                            base_xpath = xpath;
                        }
                        else
                        {//SetProduct
                            if (base_xpath == null)
                            {
                                Message.Error("No product block was set.");
                                return;
                            }
                            if (!xpath.StartsWith(base_xpath))
                            {
                                MainWindow.This.Browser.HighlightElements(base_xpath);
                                Message.Error("Your selection must be inside the product block you picked up previously.");
                                return;
                            }
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (ProductFieldWindow.Item i in w.Items)
                                    if (i.Get)
                                        route.SetOutput("Product0", new Route.Output.Field { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath.Substring(base_xpath.Length, xpath.Length - base_xpath.Length), Attribute = i.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case RouteType.UNIVERSAL:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        StepItem si = ((StepItem)step.SelectedItem);
                        if (si.Step == StepItem.Steps.ListNext)
                        {
                            route.SetOutput(si.QueueName, new Route.Output.UrlCollection { Queue = si.QueueName, Xpath = xpath, QueuingManner = Route.Output.UrlCollection.QueuingManners.LIFO });
                        }
                        else if (si.Step == StepItem.Steps.Children)
                        {
                            string x = find_product_links_xpath(xpath);

                            string output_queue_name = null;
                            bool passed = false;
                            foreach (Route.Queue q in route.GetQueues())
                            {
                                if (passed)
                                {
                                    output_queue_name = q.Name;
                                    break;
                                }
                                if (q.Name == si.QueueName)
                                    passed = true;
                            }
                            if (output_queue_name == null)
                            {
                                output_queue_name = Queue.BaseName + (Queue.GetLevel(si.QueueName) + 1);
                                step.Items.Add(new StepItem(StepItem.Steps.ListNext, output_queue_name));
                                step.Items.Add(new StepItem(StepItem.Steps.Data, output_queue_name));
                                step.Items.Add(new StepItem(StepItem.Steps.Children, output_queue_name));
                            }
                            route.SetOutput(si.QueueName, new Route.Output.UrlCollection { Queue = output_queue_name, Xpath = x });

                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (si.Step == StepItem.Steps.Data)
                        {
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (ProductFieldWindow.Item i in w.Items)
                                    if (i.Get)
                                        route.SetOutput(si.QueueName, new Route.Output.Field { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath, Attribute = i.Attribute });
                            }
                        }
                        else if (si.Step == StepItem.Steps.Submit)
                        {
                            string action = null;
                            if (Regex.IsMatch(xpath, @"/input(\[\d+\])?$", RegexOptions.IgnoreCase))
                            {
                                string type = MainWindow.This.Browser.GetAttribute(xpath, "type")[0];
                                if (Regex.IsMatch(type, @"text|email|password", RegexOptions.IgnoreCase))
                                    action = "set";
                                else
                                    action = "click";
                            }
                            else if (Regex.IsMatch(xpath, @"/textarea(\[\d+\])?$", RegexOptions.IgnoreCase))
                                action = "set";
                            else if (Regex.IsMatch(xpath, @"/button(\[\d+\])?$", RegexOptions.IgnoreCase))
                                action = "click";
                            else
                                return;

                            switch (action)
                            {
                                case "set":
                                    route.AddAction(si.QueueName, new Route.Action.Set { Xpath = xpath, Attribute = "value", Value = MainWindow.This.Browser.GetValue(xpath)[0] });
                                    break;
                                case "click":
                                    route.AddAction(si.QueueName, new Route.Action.Click { Xpath = xpath });
                                    route.AddAction(si.QueueName, new Route.Action.WaitDocumentLoaded { MinimalSleepMss = 500 });

                                    MainWindow.This.Browser.Click(xpath);
                                    //MainWindow.This.Browser.WaitForCompletion();
                                    break;
                                default:
                                    throw new Exception("Undefined action: " + action);
                            }
                        }
                        else
                            throw new Exception("No such option: " + si.Step);
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

            //xml.TextChanged+=del

            save.Click += delegate {
                if (!Controller.Check(route))
                {
                    if (!Message.YesNo("The current route is uncompleted. Would you like to save it anyway?"))
                        return;
                }
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
                        route.AddInputItem(Queue.StartQueueName, new Route.InputItem.Url { Value = url.Trim() });
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
                        case 2:
                            route_type = RouteType.UNIVERSAL;
                            step.DisplayMemberPath = "Text";
                            step.SelectedValuePath = "Value";
                            step.Items.Add(new StepItem(StepItem.Steps.Submit, Queue.StartQueueName));
                            step.Items.Add(new StepItem(StepItem.Steps.ListNext, Queue.StartQueueName));
                            step.Items.Add(new StepItem(StepItem.Steps.Data, Queue.StartQueueName));
                            step.Items.Add(new StepItem(StepItem.Steps.Children, Queue.StartQueueName));
                            break;
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
                    case RouteType.UNIVERSAL:
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
            DATA_IS_IN_LIST, 
            UNIVERSAL
        }
        Route route;

        public class StepItem
        {
            public enum Steps
            {
                Submit,
                ListNext,
                Data,
                Children
            }

            public string Text { get; private set; }
            public readonly string QueueName;
            public readonly Steps Step;

            public StepItem(Steps step, string queue_name)
            {
                Step = step;
                QueueName = queue_name;
                Text = Step.ToString() + Queue.GetLevel(queue_name);
            }
        }

        public class Queue
        {
            public const string BaseName = "Queue";
            public const string StartQueueName = BaseName + "0";

            public static int GetLevel(string queue_name)
            {
                return int.Parse(Regex.Replace(queue_name, @"[^\d]+", ""));
            }
        }

        void listen_clicks()
        {
            MainWindow.This.Browser.ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath()
                + CefSharpBrowser.Define__createXPathForElement() + @"
if(document.__onElementSelected){
    document.__clickedElement = null;
    return;
}
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