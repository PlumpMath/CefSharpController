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
                            route.SetOutputUrlCollection(StartStepItemQueue.Name, new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductPages
                            string x = find_product_links_xpath(xpath);
                            route.SetOutputUrlCollection(StartStepItemQueue.Name, new Route.OutputUrlCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            route.SetOutputUrlCollection("Product" + (step.Items.Count - 4), new Route.OutputUrlCollection { Queue = "Product" + (step.Items.Count - 3), Xpath = xpath });
                            step.Items.Insert(step.Items.Count - 1, "SetProduct" + (step.Items.Count - 3));
                        }
                        else
                        {//SetProduct
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (ProductFieldWindow.Item i in w.Items)
                                    if (i.Get)
                                        route.SetOutputField("Product" + (step.SelectedIndex - 2), new Route.OutputField { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath, Attribute = i.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case RouteType.DATA_IS_IN_LIST:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        if (step.SelectedIndex == 0)
                        {
                            route.SetOutputUrlCollection(StartStepItemQueue.Name, new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrlCollection("ListNextPage", new Route.OutputUrlCollection { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 1)
                        {//SetProductBlocks
                            string x = find_product_blocks_xpath(xpath);
                            route.SetOutputElementCollection(StartStepItemQueue.Name, new Route.OutputElementCollection { Queue = "Product0", Xpath = x });
                            route.SetOutputElementCollection("ListNextPage", new Route.OutputElementCollection { Queue = "Product0", Xpath = x });
                            MainWindow.This.Browser.HighlightElements(x);
                            base_xpath = xpath;
                        }
                        else
                        {//SetProduct
                            if (base_xpath==null)
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
                                        route.SetOutputField("Product0", new Route.OutputField { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath.Substring(base_xpath.Length, xpath.Length - base_xpath.Length), Attribute = i.Attribute });
                            }
                        }
                        xml.Text = route.Xml;
                        break;
                    case RouteType.UNIVERSAL:
                        MainWindow.This.Browser.HighlightElements(xpath);
                        StepItem si = ((StepItem)step.SelectedItem);
                        if (si.Step == StepItem.Steps.ListNext)
                        {
                            StepItemQueue nq = find_next_queue_in_steps(si, true);
                            if (nq == null)
                                nq = new StepItemQueue(si.Queue, true);
                            route.SetOutputUrlCollection(si.Queue.Name, new Route.OutputUrlCollection { Queue = nq.Name, Xpath = xpath });
                            route.SetOutputUrlCollection(nq.Name, new Route.OutputUrlCollection { Queue = nq.Name, Xpath = xpath });
                        }
                        else if (si.Step == StepItem.Steps.Children)
                        {
                            string x = find_product_links_xpath(xpath);

                            StepItemQueue nq = find_next_queue_in_steps(si, false);
                            if (nq == null)
                            {
                                nq = new StepItemQueue(si.Queue, false);
                                step.Items.Add(new StepItem(StepItem.Steps.ListNext, nq));
                                step.Items.Add(new StepItem(StepItem.Steps.Data, nq));
                                step.Items.Add(new StepItem(StepItem.Steps.Children, nq));
                            }
                            route.SetOutputUrlCollection(si.Queue.Name, new Route.OutputUrlCollection { Queue = nq.Name, Xpath = x });
                            StepItemQueue lnq = find_next_queue_in_steps(si, true);
                            if (lnq != null)
                                route.SetOutputUrlCollection(lnq.Name, new Route.OutputUrlCollection { Queue = nq.Name, Xpath = xpath });

                            MainWindow.This.Browser.HighlightElements(x);
                        }
                        else if (si.Step == StepItem.Steps.Data)
                        {
                            ProductFieldWindow w = new ProductFieldWindow(xpath);
                            if (w.ShowDialog() == true)
                            {
                                foreach (ProductFieldWindow.Item i in w.Items)
                                    if (i.Get)
                                        route.SetOutputField(si.Queue.Name, new Route.OutputField { Name = w.Name.Text + "." + i.Attribute, Xpath = xpath, Attribute = i.Attribute });
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
        StepItemQueue find_next_queue_in_steps(StepItem si, bool list_next)
        {
            foreach (StepItem i in step.Items)
                if (i.Queue.Level > si.Queue.Level && i.Step == si.Step && i.Queue.NextList == list_next)
                    return i.Queue;
            return null;
        }
        //StepItem find_step_in_steps(StepItem.Steps s, StepItemQueue q)
        //{
        //    foreach (StepItem i in step.Items)
        //        if (i.Queue == q && i.Step == s)
        //            return i;
        //    return null;
        //}

        public RouteControl()
        {
            InitializeComponent();

            //xml.TextChanged+=del

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
                        route.AddInputItem(StartStepItemQueue.Name, new Route.InputUrl { Value = url.Trim() });
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
                            step.Items.Add(new StepItem(StepItem.Steps.ListNext, StartStepItemQueue));
                            step.Items.Add(new StepItem(StepItem.Steps.Data, StartStepItemQueue));
                            step.Items.Add(new StepItem(StepItem.Steps.Children, StartStepItemQueue));
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
                ListNext,
                Data,
                Children
            }

            public string Text { get; private set; }
            public readonly StepItemQueue Queue;
            public readonly Steps Step;
            //public bool Set = false;

            public StepItem(Steps step, StepItemQueue queue)
            {
                Step = step;
                Queue = queue;
                Text = Step.ToString() + Queue.Level;
            }
        }

        public class StepItemQueue
        {
            public readonly string Name;
            public const string BaseName = "Queue";
            public readonly int Level;
            public readonly bool NextList;
            public readonly StepItemQueue ParentQueue;

            public StepItemQueue(StepItemQueue parent_queue, bool next_list)
            {
                ParentQueue = parent_queue;
                NextList = next_list;
                Level = 0;
                for (StepItemQueue siq = this.ParentQueue; siq != null; siq = siq.ParentQueue)
                    Level++;
                Name = BaseName + Level;
                if (next_list)
                    Name += "NextList";
            }
        }

        StepItemQueue StartStepItemQueue = new StepItemQueue(null, false);

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