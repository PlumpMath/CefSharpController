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
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (state.SelectedIndex == 1)
                    route.ProductListNextPageXpath = xpath;
                else if (state.SelectedIndex == 2)
                    route.ProductPagesXpath = xpath;
                else if (state.SelectedIndex == 3)
                {
                }
                xml.Text = route.Xml;
            });
        }

        //        void fillProduct()
        //        {
        //            MainWindow.Execute(@"
        //                document.removeEventListener('click', __onClick, false);

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



                        //            MainWindow.Execute("alert('h');");
                        //Message.Inform("Click List Next Page Link");
                        //route.ProductListNextPageXpath = ;
                        //Message.Inform("Click Product Page Link");
                        //route.ProductPagesXpath = ;}
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
                else if (state.SelectedIndex == 1
                    || state.SelectedIndex == 2)
                {
                    MainWindow.Execute(@"

            function createXPathFromElement(elm) {
                var allNodes = document.getElementsByTagName('*');
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


function __onClick(event){
    try{
      if ( event.preventDefault ) event.preventDefault();
      if ( event.stopPropagation ) event.stopPropagation();
      event.returnValue = false;
        var targetElement = event.target || event.srcElement;
        //alert(targetElement);
    var x = createXPathFromElement(targetElement);
    //alert(x);
        window.JsMapObject.clickedHtml(x);
    }catch(err){
        alert(err.message);
    }
    return false;
}
document.__onClick = __onClick;
document.removeEventListener('click', document.__onClick, false);
document.addEventListener('click', document.__onClick, false);
");

                }
                else if (state.SelectedIndex == 3)
                {
                    MainWindow.Execute(@"
                    document.removeEventListener('click', document.__onClick, false);
                    ");
                    string d = @"

           function lookupElementByXPath(path) {
                        var evaluator = new XPathEvaluator();
                        var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                        return result.singleNodeValue;
                    }

        var e = lookupElementByXPath('" + route.ProductPagesXpath + @"');
        alert(e);
        e.click();



                    ";
                    MainWindow.Execute(d);
                }
            };

            Loaded += delegate
            {
                state.SelectedIndex = 0;
            };
        }
        string xml_name;
        Route route;
    }
}
