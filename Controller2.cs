//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey_stoyan@yahoo.com
//        http://www.cliversoft.com
//        26 September 2006
//Copyright: (C) 2006, Sergey Stoyan
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Web;
using System.Net;

namespace Cliver.CefSharpController
{
    public class Controller2
    {
        Controller2(Route2 route)
        {
            this.route = route;
            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".csv");
            List<string> hs = route.ProductFields.Select(x => x.Name).ToList();
            hs.Insert(0, "Url");
            tw.WriteLine(FieldPreparation.GetCsvLine(hs, FieldPreparation.FieldSeparator.COMMA));
        }
        TextWriter tw = null;
        Route2 route = null;

        public static void Start(Route2 route)
        {
            t = ThreadRoutines.StartTry(() =>
            {
                c = new Controller2(route);
                MainWindow.Load(route.ProductListUrl, true);
                c.ProcessProductListPage();
            });
        }
        static Controller2 c;
        static Thread t;

        public static void Stop()
        {
            if (t != null && t.IsAlive)
                t.Abort();
            c = null;
            MainWindow.Stop();
        }

        void click(string xpath)
        {
            MainWindow.Execute(@"
                    document.__getElementsByXPath = function(path) {
                        var evaluator = new XPathEvaluator();
                        var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                        var es = [];
                        for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                            es.push(thisNode);
                        }
                        return es;
                    };

            var es =  document.__getElementsByXPath('" + xpath + @"');
if(es.length < 1)
    alert('no element found:' + '" + xpath + @"');
else
    es[0].click();
            ");
        }

        List<string> get_links(string xpath)
        {
            var os = (List<object>)MainWindow.Execute(
                WebDocumentRoutines.Define_getElementsByXPath()
                + @"
            var es =  document.__getElementsByXPath('" + xpath + @"');
var ls = [];
for(var i = 0; i < es.length; i++){
    var e = es[i];
    while(e && e.tagName != 'A')
        e = e.parentNode;
    if(e)
        ls.push(e.href);
}
return ls;
            ");

            List<string> ls = new List<string>();
            if (os != null)
            {
                string parent_url = MainWindow.Url;
                for (int i = 0; i < os.Count; i++)
                    ls.Add(GetAbsoluteUrl((string)os[i], parent_url));
            }
            return ls;
        }

        public static string GetAbsoluteUrl(string link, string parent_url)
        {
            try
            {
                if (link == null)
                    return null;
                link = HttpUtility.HtmlDecode(link);
                Uri u = new Uri(parent_url);
                Uri ulink = new Uri(u, link);
                return ulink.ToString();
            }
            //catch (Exception e)
            catch
            {
                //Log.Error(e.Message + "\n" + e.StackTrace + "\nlink=" + link + "\nparent_url=" + parent_url);
                return null;
            }
        }

        string get_value(string xpath)
        {
            return (string)MainWindow.Execute(@"
                    document.__getElementsByXPath = function(path) {
                        var evaluator = new XPathEvaluator();
                        var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                        var es = [];
                        for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                            es.push(thisNode);
                        }
                        return es;
                    };

            var es =  document.__getElementsByXPath('" + xpath + @"');
var vs = '';
for(var i = 0; i < es.length; i++){
    vs += '\r\n' + es[i].innerText;
}
return vs;
            ");
        }

        void ProcessProductListPage()
        {
            string npu = null;
            var ls = get_links(route.ProductListNextPageXpath);
            if (ls.Count > 0)
                npu = ls[0];
            List<string> product_page_urls = get_links(route.ProductPagesXpath);
            if (product_page_urls.Count > 5)
            {
                Log.Inform("While testing only up to 5 products per page is processed.");
                product_page_urls.RemoveRange(5, product_page_urls.Count - 6);
            }
            foreach (string ppu in product_page_urls)
            {
                MainWindow.Load(ppu, true);
                ProcessProductPage();
            }
            if (npu == null)
            {
                Log.Warning("no next page found");
                return;
            }
            MainWindow.Load(npu, true);
            ProcessProductListPage();
        }

        void ProcessProductPage()
        {
            string url = null;
            MainWindow.This.Dispatcher.Invoke(() =>
            {
                url = MainWindow.Browser.Address;
            });
            List<string> vs = new List<string>();
            foreach (Route2.ProductField p in route.ProductFields)
            {
                vs.Add(get_value(p.Xpath));
            }
            vs.Insert(0, url);
            tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, true));
            tw.Flush();
        }
    }
}