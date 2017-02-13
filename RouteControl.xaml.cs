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
        public void clickedHtml(string xpath)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                switch (route_type.SelectedIndex)
                {
                    case 0:
                        highlight(xpath);
                        if (step.SelectedIndex == 1)
                        {
                            route.SetOutputUrl("Start", new Route.Url { Queue = "ListNextPage", Xpath = xpath });
                            route.SetOutputUrl("ListNextPage", new Route.Url { Queue = "ListNextPage", Xpath = xpath });
                        }
                        else if (step.SelectedIndex == 2)
                        {
                            string x = find_product_links_xpath(xpath);
                            route.SetOutputUrl("Start", new Route.Url { Queue = "Product", Xpath = x });
                            route.SetOutputUrl("ListNextPage", new Route.Url { Queue = "Product", Xpath = x });
                            highlight(x);
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            route.SetOutputUrl("Product" + (step.SelectedIndex - 4), new Route.Url { Queue = "Product" + (step.SelectedIndex - 3), Xpath = xpath });
                        }
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
                                    route.AddInputItem("Start", new Route.Item { Url = url });
                                    xml.Text = route.Xml;
                                    MainWindow.Load(url, false);
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Error(e);
                            }
                        }
                        else if (step.SelectedIndex == step.Items.Count - 1)
                        {
                            step.Items.Insert(step.SelectedIndex, "SetProduct" + (step.SelectedIndex - 3));
                        }
                        else
                        {
                            listen_clicks();
                        }
                        break;
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
            MainWindow.Execute(@"
if(!document.__onClick){
                document.__createXPathForElement =  function(elm) {
                var allNodes = document.__getElementsByTagName('*');
                for (var segs = []; elm && elm.nodeType == 1; elm = elm.parentNode)
                {
                    if (elm.hasAttribute('id'))
                    {
                        var uniqueIdCount = 0;
                        for (var n = 0; n < allNodes.length; n++)
                        {
                            if (allNodes[n].hasAttribute('id') && allNodes[n].id == elm.id) uniqueIdCount++;
                            if (uniqueIdCount > 1) break;
                        };
                        if (uniqueIdCount == 1)
                        {
                            segs.unshift('id(""' + elm.getAttribute('id') + '"")');
                            return segs.join('/');
                        }
                        else
                        {
                            segs.unshift(elm.localName.toLowerCase() + '[@id=""' + elm.getAttribute('id') + '""]');
                        }
                    }
                    else if (elm.hasAttribute('class'))
                    {
                        segs.unshift(elm.localName.toLowerCase() + '[@class=""' + elm.getAttribute('class') + '""]');
                    }
                    else
                    {
                        for (i = 1, sib = elm.previousSibling; sib; sib = sib.previousSibling)
                        {
                            if (sib.localName == elm.localName) i++;
                        };
                        segs.unshift(elm.localName.toLowerCase() + '[' + i + ']');
                    };
                };
                return segs.length ? '/' + segs.join('/') : null;
            };

            document.__getElementsByXPath = function(path) {
                var evaluator = new XPathEvaluator();
                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                var es = [];
                for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                    es.push(thisNode);
                }
                return es;
            };

            document.__createXPathForElement = function(element) {
                var xpath = '';
                for (; element && element.nodeType == 1; element = element.parentNode) {
                    //alert(element);
                    var cs = element.parentNode.children;
                    var j = 0;
                    var k = 0;
                    for(var i = 0; i < cs.length; i++){
                        if (cs[i].tagName == element.tagName){
                            j++;
                            if(cs[i] == element){
                                k = j;
                                //break;
                            }
                        } 
                    }
                    var id = '';
                    if(j > 1)
                        id = '[' + k + ']';
                    xpath = '/' + element.tagName.toLowerCase() + id + xpath;
                }
                return xpath;
            };

            function __onClick(event){
                try{
                  if ( event.preventDefault ) event.preventDefault();
                  if ( event.stopPropagation ) event.stopPropagation();
                  event.returnValue = false;
                  var targetElement = event.target || event.srcElement;
                  //alert(targetElement);
                    var x = document.__createXPathForElement(targetElement);
               // alert(x);
                    window.JsMapObject.clickedHtml(x);
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

        void highlight(string xpath)
        {
            MainWindow.Execute(@"
                if(!document.__highlightedElements){
                    var style = document.createElement('style');
                    style.type = 'text/css';
                    style.innerHTML = '.__highlight { background-color: #F00 !important; }';
                    document.getElementsByTagName('head')[0].appendChild(style);
                }else{
                    for(var i = 0; i < document.__highlightedElements.length; i++)
                        document.__highlightedElements[i].className = document.__highlightedElements[i].className.replace(/\b__highlight\b/,''); 
                }

                document.__highlightedElements = [];
                var es = document.__getElementsByXPath('" + xpath + @"');
                for(var i = 0; i < es.length; i++){
                    es[i].className += ' __highlight';
                    document.__highlightedElements.push(es[i]);
                }
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
                int count = (int)MainWindow.Execute(@"
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