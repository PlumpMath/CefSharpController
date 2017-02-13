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
                    li.Add(new Queue.Item { Url = x.Url, Xpath = x.Xpath, ParentItem = null, Queue = q });

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
                List<Queue.Url> us = new List<Queue.Url>();
                foreach (Route.Url x in rq.Urls)
                    us.Add(new Queue.Url { Queue = queues.Where(z => z.Name == x.Queue).First(), Xpath = x.Xpath });

                Queue q = queues.Where(z => z.Name == rq.Name).First();
                q.Urls = us;
            }
            queues.Reverse();

            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".csv");
            List<string> hs = route.Fields.Select(x => x.Name).ToList();
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
            MainWindow.Stop();
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
                    List<string> vs = i.OutputValues;
                    for (Queue.Item pi = i.ParentItem; pi != null; pi = pi.ParentItem)
                        vs.InsertRange(0, pi.OutputValues);
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
                public string Url;
                public string Xpath;
                public Queue Queue;
                public Item ParentItem = null;
                public List<string> OutputValues = new List<string>();
            }

            public class Url
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
            public List<Url> Urls = new List<Url>();
            public List<Field> Fields = new List<Field>();

            public void ProcessItem(Item item)
            {
                if (!string.IsNullOrWhiteSpace(item.Url))
                    MainWindow.Load(item.Url, true);
                if (!string.IsNullOrWhiteSpace(item.Xpath))
                    get_value(new Field { Name = "", Attribute = "", Xpath = item.Xpath });
                string url = MainWindow.Url;
                foreach (Url u in Urls)
                {
                    foreach (string l in get_links(u.Xpath))
                        u.Queue.Items.Add(new Controller.Queue.Item { Url = l, Queue = u.Queue, ParentItem = item });
                }

                foreach (Field f in Fields)
                {
                    item.OutputValues.Add(get_value(f));
                }
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

            string get_value(Field field)
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

            var es =  document.__getElementsByXPath('" + field.Xpath + @"');

var vs = '';
for(var i = 0; i < es.length; i++){    
    vs += '\r\n' + " + (field.Attribute == "__innerHtml__" ? @"es[i].innerText" : @"es[i].getAttribute('" + field.Attribute + @"')") + @";
}
return vs;
            ");
            }
        }
    }
}