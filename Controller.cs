//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
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
        static public bool Check(Route route)
        {
            try
            {
                get_queues(route);
                return true;
            }
            catch (Exception e)
            {
                Message.Error2(e);
                return false;
            }
        }

        Controller(Route route)
        {
            queues = get_queues(route);

            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".csv");
            foreach (Queue q in queues)
                headers.InsertRange(0, q.Outputs.Where(o => o is Queue.Output.Field).Select(f => ((Queue.Output.Field)f).Name));
            headers.Insert(0, "Url");
            tw.WriteLine(FieldPreparation.GetCsvLine(headers, FieldPreparation.FieldSeparator.COMMA));
        }
        readonly TextWriter tw = null;
        readonly List<string> headers = new List<string>();

        static List<Queue> get_queues(Route route)
        {
            List<Queue> queues = new List<Queue>();

            List<Route.Queue> rqs = route.GetQueues();
            foreach (Route.Queue rq in rqs)
            {
                Queue q = new Queue();

                List<Queue.InputItem> iis = new List<Queue.InputItem>();
                foreach (Route.InputItem x in rq.InputItems)
                {
                    if (x is Route.InputItem.Url)
                        iis.Add(new Queue.InputItem.Url { Value = x.Value, ParentItem = null, Queue = q });
                    else if (x is Route.InputItem.Element)
                        iis.Add(new Queue.InputItem.Element { Value = x.Value, ParentItem = null, Queue = q });
                    else
                        throw new Exception("Unknown type: " + x.GetType());
                }

                List<Queue.Action> as_ = new List<Queue.Action>();
                foreach (Route.Action x in rq.Actions)
                {
                    if (x is Route.Action.Set)
                    {
                        var a = (Route.Action.Set)x;
                        as_.Add(new Queue.Action.Set { Xpath = a.Xpath, Attribute = a.Attribute, Value = a.Value });
                    }
                    else if (x is Route.Action.Click)
                    {
                        var a = (Route.Action.Click)x;
                        as_.Add(new Queue.Action.Click { Xpath = a.Xpath });
                    }
                    else if (x is Route.Action.WaitDocumentLoaded)
                    {
                        var a = (Route.Action.WaitDocumentLoaded)x;
                        as_.Add(new Queue.Action.WaitDocumentLoaded { MinimalSleepMss = a.MinimalSleepMss });
                    }
                    else
                        throw new Exception("Unknown type: " + x.GetType());
                }

                q.Name = rq.Name;
                q.InputItems = iis;
                q.Actions = as_;
                queues.Add(q);
            }
            foreach (Route.Queue rq in rqs)
            {
                List<Queue.Output> os = new List<Queue.Output>();
                foreach (Route.Output x in rq.Outputs)
                {
                    if (x is Route.Output.UrlCollection)
                    {
                        var a = (Route.Output.UrlCollection)x;
                        os.Add(new Queue.Output.UrlCollection { Queue = queues.Where(z => z.Name == a.Queue).First(), Xpath = a.Xpath, QueuingManner = a.QueuingManner });
                    }
                    else if (x is Route.Output.ElementCollection)
                    {
                        var a = (Route.Output.ElementCollection)x;
                        os.Add(new Queue.Output.ElementCollection { Queue = queues.Where(z => z.Name == a.Queue).First(), Xpath = a.Xpath });
                    }
                    else if (x is Route.Output.Field)
                    {
                        var a = (Route.Output.Field)x;
                        os.Add(new Queue.Output.Field { Name = a.Name, Xpath = a.Xpath, Attribute = a.Attribute });
                    }
                    else
                        throw new Exception("Unknown type: " + x.GetType());
                }

                Queue q = queues.Where(z => z.Name == rq.Name).First();
                q.Outputs = os;
            }
            queues.Reverse();

            return queues;
        }

        public static void Start(Route route)
        {
            t = ThreadRoutines.Start(() =>
             {
                 try
                 {
                    if( Log.IsMainSessionOpen)
                         Log.MainSession.Close();
                     Log.Initialize(Log.Mode.SESSIONS, null, true, 10, route.Name);

                     c = new Controller(route);
                     c.Start();
                 }
                 finally
                 {
                     Log.MainSession.Close();
                     Log.Initialize(Log.Mode.SESSIONS, null, false);
                 }
             });
        }
        static Controller c;
        static Thread t;
        static public bool DebugMode;
        static public bool Pause;

        public static bool Running
        {
            get
            {
                return (t != null && t.IsAlive);
            }
        }

        public static void Stop()
        {
            if (c != null)
                c.run = false;
            if (t != null && t.IsAlive)
                t.Abort();
            if (c.tw != null)
                c.tw.Close();
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
                var ii = get_next_item();
                if (ii == null)
                    return;
                ii.Queue.ProcessItem(ii);
                if (ii.Queue == queues[0])
                {
                    List<string> vs = new List<string>();
                    string url = null;
                    for (; ii != null; ii = ii.ParentItem)
                    {
                        vs.InsertRange(0, ii.OutputValues);
                        if (ii is Queue.InputItem.Url
                            && (url == null || ii.OutputValues.Count > 0)
                            )
                            url = ii.Value;
                        if (ii.ParentItem == null)
                            break;
                        if (queues.IndexOf(ii.Queue) >= queues.IndexOf(ii.ParentItem.Queue))
                            break;
                    }
                    vs.Insert(0, url);
                    tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, true));
                    tw.Flush();
                    WriteLine?.Invoke(headers, vs);
                }
            }
        }
        bool run = false;

        public delegate void OnWriteLine(List<string> headers, List<string> values);
        public static event OnWriteLine WriteLine = null;

        Queue.InputItem get_next_item()
        {
            while (Controller.Pause)
                Thread.Sleep(300);

            foreach (Queue q in queues)
            {
                if (q.InputItems.Count > 0)
                {
                    var i = q.InputItems[0];
                    q.InputItems.Remove(i);
                    return i;
                }
            }
            return null;
        }
        readonly List<Queue> queues = new List<Queue>();

        public class Queue
        {
            public string Name;

            public abstract class InputItem
            {
                public string Value;
                public Queue Queue;
                public InputItem ParentItem = null;
                public List<string> OutputValues = new List<string>();

                public class Url : InputItem
                {
                }

                public class Element : InputItem
                {
                }
            }

            abstract public class Action
            {
                abstract public void Perform();

                public class Set : Action
                {
                    public string Xpath;
                    public string Value;
                    public string Attribute;

                    override public void Perform()
                    {
                        MainWindow.This.Browser.SetValue(Xpath, Value);
                    }
                }

                public class Click : Action
                {
                    public string Xpath;

                    override public void Perform()
                    {
                        MainWindow.This.Browser.Click(Xpath);
                    }
                }

                public class WaitDocumentLoaded : Action
                {
                    public int MinimalSleepMss;

                    override public void Perform()
                    {
                        MainWindow.This.Browser.WaitForCompletion();
                    }
                }
            }

            abstract public class Output
            {
                public class UrlCollection : Output
                {
                    public Queue Queue;
                    public string Xpath;
                    public Route.Output.UrlCollection.QueuingManners QueuingManner = Route.Output.UrlCollection.QueuingManners.FIFO;
                }

                public class ElementCollection : Output
                {
                    public Queue Queue;
                    public string Xpath;
                }

                public class Field : Output
                {
                    public string Name;
                    public string Xpath;
                    public string Attribute;
                }
            }

            public List<InputItem> InputItems = new List<InputItem>();
            public List<Action> Actions = new List<Action>();
            public List<Output> Outputs = new List<Output>();

            public void ProcessItem(InputItem ii)
            {
                string base_xpath = "";
                if (ii is InputItem.Url)
                {
                    MainWindow.This.Browser.Load(ii.Value, true);
                }
                else if (ii is InputItem.Element)
                {
                    base_xpath = ii.Value;
                }
                else
                    throw new Exception("Unknown type: " + ii.GetType());

                foreach (Action a in Actions)
                    a.Perform();

                string url = MainWindow.This.Browser.Url;
                foreach (Output o in Outputs)
                {
                    if (o is Output.UrlCollection)
                    {
                        Output.UrlCollection uc = (Output.UrlCollection)o;
                        foreach (string l in get_links(base_xpath + uc.Xpath))
                        {
                            var i = new Controller.Queue.InputItem.Url { Value = l, Queue = uc.Queue, ParentItem = ii };
                            switch (uc.QueuingManner)
                            {
                                case Route.Output.UrlCollection.QueuingManners.FIFO:
                                    uc.Queue.InputItems.Add(i);
                                    break;
                                case Route.Output.UrlCollection.QueuingManners.LIFO:
                                    uc.Queue.InputItems.Insert(0, i);
                                    break;
                                default:
                                    throw new Exception("Unknown option: " + uc.QueuingManner);
                            }
                            Log.Main.Write("Added url to " + uc.Queue.Name + ": " + l);
                        }
                    }
                    if (o is Output.ElementCollection)
                    {
                        Output.ElementCollection ec = (Output.ElementCollection)o;
                        foreach (string x in get_single_element_xpaths(base_xpath + ec.Xpath))
                        {
                            ec.Queue.InputItems.Add(new Controller.Queue.InputItem.Element { Value = x, Queue = ec.Queue, ParentItem = ii });
                            Log.Main.Write("Added element to " + ec.Queue.Name + ": " + x);
                        }
                    }
                    if (o is Output.Field)
                    {
                        Output.Field f = (Output.Field)o;
                        string v = null;
                        if (f.Attribute == Route.Output.Field.INNER_TEXT)
                        {
                            v = get_value(base_xpath + f.Xpath, Route.Output.Field.INNER_HTML);
                            v = FieldPreparation.Html.Normalize(v);
                        }
                        else
                            v = get_value(base_xpath + f.Xpath, f.Attribute);
                        ii.OutputValues.Add(v);
                    }
                }
            }

            List<string> get_links(string xpath)
            {
                var os = (List<object>)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define__getElementsByXPath() + @"
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

                if (Controller.DebugMode)
                    if (ls.Count > 3)
                    {
                        Log.Main.Warning("While debugging only first 3 links are taken of actual " + ls.Count);
                        ls.RemoveRange(3, ls.Count - 3);
                    }

                return ls;
            }

            List<string> get_single_element_xpaths(string xpath)
            {
                var os = (List<object>)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define__getElementsByXPath() +
                    CefSharpBrowser.Define__createXPathForElement() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
var xs = [];
for(var i = 0; i < es.length; i++){
    xs.push(document.__createXPathForElement(es[i]));
}
return xs;
            ");

                List<string> xs = new List<string>();
                if (os != null)
                {
                    for (int i = 0; i < os.Count; i++)
                        xs.Add((string)os[i]);
                }
                return xs;
            }

            //            int get_elements(string xpath)
            //            {
            //                return (int)MainWindow.This.Browser.ExecuteJavaScript(
            //                    CefSharpBrowser.Define__getElementsByXPath() + @"
            //document.__elementCollection =  document.__getElementsByXPath('" + xpath + @"');
            //return document.__elementCollection.length;
            //            ");
            //            }

            string get_value(string xpath, string attribute)
            {
                return (string)MainWindow.This.Browser.ExecuteJavaScript(
                    CefSharpBrowser.Define__getElementsByXPath() + @"
var es =  document.__getElementsByXPath('" + xpath + @"');
var vs = '';
for(var i = 0; i < es.length; i++){    
    vs += '\r\n' + " + (attribute == Route.Output.Field.INNER_HTML ? @"es[i].innerText" : @"es[i].getAttribute('" + attribute + @"')") + @";
}
return vs;
            ");
            }
        }
    }
}