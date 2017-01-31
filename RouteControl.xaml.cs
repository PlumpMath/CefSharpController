﻿using System;
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                highlight(xpath);
                if (state.SelectedIndex == 1)
                    route.ProductListNextPageXpath = xpath;
                else if (state.SelectedIndex == 2)
                {
                    route.ProductPagesXpath = find_product_links_xpath(xpath);
                    highlight(route.ProductPagesXpath);
                }
                else if (state.SelectedIndex == 3)
                {
                    ProductFieldWindow w = new ProductFieldWindow();
                    w.ShowDialog();
                    route.ProductField = new Route.ProductFieldClass { Name = w.FieldName.Text, Xpath = xpath, Attribute = "text" };
                }
                xml.Text = route.Xml;
            });
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
                if (d.ShowDialog() == true)
                    Controller.Start(Route.LoadFromFile(d.FileName));
            };

            state.SelectionChanged += delegate
            {
                if (state.SelectedIndex == 0)
                {
                    try
                    {
                        var d = new StartWindow();
                        if (d.ShowDialog() == true)
                        {
                            route = new CefSharpController.Route(d.XmlName.Text);
                            route.ProductListUrl = d.StartUrl.Text;
                            xml.Text = route.Xml;
                            MainWindow.Load(route.ProductListUrl, true);
                        }
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
                    var cs = $(element.parentNode).children(element.tagName);
                    var id = '';
                    if(cs.length > 1)
                        id = '[' + (cs.index(element) + 1) + ']';
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
                    style.innerHTML = '.__highlight { background-color: #F00; }';
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