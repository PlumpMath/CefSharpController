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
    public class Controller
    {
        Controller(Route route)
        {
            this.route = route;
            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".txt");
            tw.WriteLine(FieldPreparation.GetCsvLine(route.ProductFields.Select(x=>x.Name), FieldPreparation.FieldSeparator.COMMA, false));
        }
        TextWriter tw = null;
        Route route = null;

        public static void Start(Route route)
        {
            t = ThreadRoutines.StartTry(() => {
                c = new Controller(route);
                MainWindow.Load(route.ProductListUrl, false);
                c.ProcessProductListPage();
            });
        }
        static Controller c;
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
            var ls = (List<string>)MainWindow.Execute(@"
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
var ls = [];
for(var i = 0; i < es.length; i++){
    ls.push(es[i].href);
}
return ls;
            ");

            for (int i = 0; i < ls.Count; i++)
                ls[i] = GetAbsoluteUrl(ls[i], MainWindow.Browser.Address);
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
            List<string> product_page_urls = get_links(route.ProductPagesXpath);
            foreach (string ppu in product_page_urls)
            {
                MainWindow.Load(ppu, false);
                ProcessProductPage();
            }
            var ls = get_links(route.ProductListNextPageXpath);
            if(ls.Count<1)
            {
                Log.Warning("no next page found");
                return;
            }
            MainWindow.Load(ls[0], false);
            ProcessProductListPage();
        }

        void ProcessProductPage()
        {
            List<string> vs = new List<string>();
            foreach (Route.ProductFieldClass p in route.ProductFields)
            {
                vs.Add(get_value(p.Xpath));
            }
            tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, false));
        }        
    }
}