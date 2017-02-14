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
            List<Route.Queue> rqs = route.GetQueues();
            foreach (Route.Queue rq in rqs)
            {
                Queue q = new Queue();

                List<Queue.Item> li = new List<Queue.Item>();
                foreach (Route.Item x in rq.Items)
                    li.Add(new Queue.Item { Value = x.Value, Type = x.Type, ParentItem = null, Queue = q });

                List<Queue.Field> fs = new List<Queue.Field>();
                foreach (Route.Field x in rq.Fields)
                    fs.Add(new Queue.Field { Name = x.Name, Xpath = x.Xpath, Attribute = x.Attribute });

                q.Name = rq.Name;
                q.Items = li;
                q.Fields = fs;
                queues.Add(q);
            }
            foreach (Route.Queue rq in rqs)
            {
                List<Queue.UrlCollection> ucs = new List<Queue.UrlCollection>();
                foreach (Route.UrlCollection x in rq.UrlCollections)
                    ucs.Add(new Queue.UrlCollection { Queue = queues.Where(z => z.Name == x.Queue).First(), Xpath = x.Xpath });

                Queue q = queues.Where(z => z.Name == rq.Name).First();
                q.UrlCollections = ucs;
            }
            queues.Reverse();

            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".csv");
            List<string> hs = new List<string>();
            foreach (Queue q in queues)
                hs.InsertRange(0, q.Fields.Select(f => f.Name));
            hs.Insert(0, "Url");
            tw.WriteLine(FieldPreparation.GetCsvLine(hs, FieldPreparation.FieldSeparator.COMMA));
        }
        TextWriter tw = null;

        public static void Start(Route route)
        {
            t = ThreadRoutines.StartTry(() =>
            {
                c = new Controller(route);
                c.Start();
            });
        }
        static Controller c;
        static Thread t;

        public static void Stop()
        {
            if (c != null)
                c.run = false;
            if (t != null && t.IsAlive)
                t.Abort();
            c = null;
            MainWindow.This.Browser.Stop();
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

        void Start()
        {
            run = true;
            while (run)
            {
                var i = get_next_item();
                if (i == null)
                    return;
                i.Queue.ProcessItem(i);
                if (i.Queue == queues[0])
                {
                    List<string> vs = new List<string>();
                    string url = null;
                    for (; i != null; i = i.ParentItem)
                    {
                        if (i.OutputValues.Count > 0)
                        {
                            vs.InsertRange(0, i.OutputValues);
                            if (i.Type == Route.Item.Types.URL)
                                url = i.Value;
                        }
                    }
                    vs.Insert(0, url);
                    tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, true));
                    tw.Flush();
                }
            }
        }
        bool run = false;

        Queue.Item get_next_item()
        {
            foreach (Queue q in queues)
            {
                if (q.Items.Count > 0)
                {
                    var i = q.Items[0];
                    q.Items.Remove(i);
                    return i;
                }
            }
            return null;
        }
        readonly List<Queue> queues = new List<Queue>();

        public class Queue
        {
            public string Name;

            public class Item
            {
                public string Value;
                public Route.Item.Types Type;
                public Queue Queue;
                public Item ParentItem = null;
                public List<string> OutputValues = new List<string>();
            }

            public class UrlCollection
            {
                public string Xpath;
                public Queue Queue;
            }

            public class ElementCollection
            {
                public string Xpath;
                public Queue Queue;
            }

            public class Field
            {
                public string Name;
                public string Xpath;
                public string Attribute;
            }

            public List<Item> Items = new List<Item>();
            public List<UrlCollection> UrlCollections = new List<UrlCollection>();
            public List<ElementCollection> ElementCollections = new List<ElementCollection>();
            public List<Field> Fields = new List<Field>();

            public void ProcessItem(Item item)
            {
                string base_xpath = "";
                switch(item.Type)
                {
                    case Route.Item.Types.URL:
                        MainWindow.This.Browser.Load(item.Value, true);
                        break;
                    case Route.Item.Types.XPATH:
                        base_xpath = item.Value;
                        break;
                    default:
                        throw new Exception("Unknown type: " + item.Type);
                }
                string url = MainWindow.This.Browser.Url;
                foreach (UrlCollection uc in UrlCollections)
                {
                    foreach (string l in get_links(base_xpath + uc.Xpath))
                        uc.Queue.Items.Add(new Controller.Queue.Item { Type = Route.Item.Types.URL, Value = l, Queue = uc.Queue, ParentItem = item });
                }

                foreach (ElementCollection ec in ElementCollections)
                {
                    //int el = get_elements(base_xpath + ec.Xpath);
                    //for (int i = 0; i < el; i++)
                    //ec.Queue.Items.Add(new Controller.Queue.Item { Type = Route.Item.Types.HTML_ELEMENT_KEY, Value = i.ToString(), Queue = ec.Queue, ParentItem = item });
                    foreach (string x in get_xpaths(base_xpath + ec.Xpath))
                        ec.Queue.Items.Add(new Controller.Queue.Item { Type = Route.Item.Types.XPATH, Value = x, Queue = ec.Queue, ParentItem = item });
                }

                foreach (Field f in Fields)
                {
                    item.OutputValues.Add(get_value(base_xpath + f.Xpath, f.Attribute));
                }
            }

            List<string> get_links(string xpath)
            {
                var os = (List<object>)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define_getElementsByXPath() + @"
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
                    string parent_url = MainWindow.This.Browser.Url;
                    for (int i = 0; i < os.Count; i++)
                        ls.Add(GetAbsoluteUrl((string)os[i], parent_url));
                }

                if (ls.Count > 3)
                {
                    Log.Warning("While debugging only first 3 links are taken of actual " + ls.Count);
                    ls.RemoveRange(3, ls.Count - 3);
                }
                return ls;
            }

            List<string> get_xpaths(string xpath)
            {
                var os = (List<object>)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define_getElementsByXPath() + 
                    CefSharpBrowser.Define_createXPathForElement() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
var xs = [];
for(var i = 0; i < es.length; i++){
    xs.push(document.__createXPathForElement(es[i]);
}
return xs;
            ");

                List<string> xs = new List<string>();
                if (os != null)
                {
                    for (int i = 0; i < os.Count; i++)
                        xs.Add((string)os[i]);
                }

                if (xs.Count > 3)
                {
                    Log.Warning("While debugging only first 3 xpaths are taken of actual " + xs.Count);
                    xs.RemoveRange(3, xs.Count - 3);
                }
                return xs;
            }

            //            int get_elements(string xpath)
            //            {
            //                return (int)MainWindow.This.Browser.ExecuteJavaScript(
            //                    CefSharpBrowser.Define_getElementsByXPath() + @"
            //document.__elementCollection =  document.__getElementsByXPath('" + xpath + @"');
            //return document.__elementCollection.length;
            //            ");
            //            }

            string get_value(string xpath, string attribute)
            {
                return (string)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define_getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
var vs = '';
for(var i = 0; i < es.length; i++){    
    vs += '\r\n' + " + (attribute == "INNER_HTML" ? @"es[i].innerText" : @"es[i].getAttribute('" + attribute + @"')") + @";
}
return vs;
            ");
            }
        }
    }
}