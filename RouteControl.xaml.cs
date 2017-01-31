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
        public void clickedHtml(string xpath)
        {
            Application.Current.Dispatcher.Invoke(() => {
                highlight(xpath);
                if (state.SelectedIndex == 1)
                    route.ProductListNextPageXpath = xpath;
                else if (state.SelectedIndex == 2)
                    route.ProductPagesXpath = xpath;
                else if (state.SelectedIndex == 3)
                {
                    ProductFieldWindow w = new ProductFieldWindow();
                    w.ShowDialog();
                    route.ProductField = new Route.ProductFieldClass { Name = w.FieldName.Text, Xpath = xpath, Attribute = "text" };
                }
                xml.Text = route.Xml;
            });
        }

        //        void fillProduct()
        //        {
        //            MainWindow.Execute(@"
        //                document.__removeEventListener('click', __onClick, false);

        //   function lookupElementByXPath(path) {
        //                var evaluator = new XPathEvaluator();
        //                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
        //                return result.singleNodeValue;
        //            }

        //var ps = lookupElementByXPath('"+ route.ProductPagesXpath + @"');
        //alert(ps);
        //alert(ps[0]);

        //            ");
        //            //MainWindow.Load(route.ProductListUrl);

        //        }

        public RouteControl()
        {
            InitializeComponent();

            state.SelectionChanged += delegate
            {
                if (state.SelectedIndex == 0)
                {
                    try
                    {
                        var d = new StartWindow();
                        d.ShowDialog();
                        xml_name = d.XmlName.Text;
                        route = new CefSharpController.Route();
                        route.ProductListUrl = d.StartUrl.Text;
                        xml.Text = route.Xml;
                        MainWindow.Load(route.ProductListUrl, true);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
                else if (state.SelectedIndex == 1
                    || state.SelectedIndex == 2)
                {
                    listen_clicks();
                }
                else if (state.SelectedIndex == 3)
                {
                    listen_clicks();
                }
            };

            Loaded += delegate
            {
                state.SelectedIndex = 0;
            };
        }
        string xml_name;
        Route route;

        void listen_clicks()
        {
            MainWindow.Execute(@"
if(!document.__onClick){
                document.__createXPathFromElement =  function(elm) {
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

            document.__lookupElementByXPath = function(path) {
                var evaluator = new XPathEvaluator();
                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                return result.singleNodeValue;
            };

            document.__createXPathFromElement =  function(element) {
                var val=element.value;
                var xpath = '';
                for (; element && element.nodeType == 1; element = element.parentNode)
                {
                    //alert(element);
                    var id = $(element.parentNode).children(element.tagName).index(element) + 1;
                    id > 1 ? (id = '[' + id + ']') : (id = '');
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
                    var x = document.__createXPathFromElement(targetElement);
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
try{
                if(!document.__highlightedElement){
                    var style = document.createElement('style');
                    style.type = 'text/css';
                    style.innerHTML = '.__highlight { background-color: #F00; }';
                    document.getElementsByTagName('head')[0].appendChild(style);
                }else
                    document.__highlightedElement.className = document.__highlightedElement.className.replace(/\b__highlight\b/,''); 

                var e = document.__lookupElementByXPath('" + xpath + @"');
//alert(e);
                e.className += ' __highlight';

                document.__highlightedElement = e;
            }catch(err){
                    alert(err.message);
                }
            ");
        }
    }
}