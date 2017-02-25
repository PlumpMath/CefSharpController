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
        Controller(Route route)
        {
            List<Route.Queue> rqs = route.GetQueues();
            foreach (Route.Queue rq in rqs)
            {
                Queue q = new Queue();

                List<Queue.InputItem> iis = new List<Queue.InputItem>();
                foreach (Route.InputItem x in rq.InputItems)
                {
                    if (x is Route.InputItem.Url)
                        iis.Add(new Queue.InputUrl { Value = x.Value, ParentItem = null, Queue = q });
                    else if (x is Route.InputItem.Element)
                        iis.Add(new Queue.InputElement { Value = x.Value, ParentItem = null, Queue = q });
                    else
                        throw new Exception("Unknown type: " + x.GetType());
                }

                List<Queue.OutputField> ofs = new List<Queue.OutputField>();
                foreach (Route.OutputField x in rq.OutputFields)
                    ofs.Add(new Queue.OutputField { Name = x.Name, Xpath = x.Xpath, Attribute = x.Attribute });

                q.Name = rq.Name;
                q.InputItems = iis;
                q.OutputFields = ofs;
                queues.Add(q);
            }
            foreach (Route.Queue rq in rqs)
            {
                List<Queue.OutputUrlCollection> ucs = new List<Queue.OutputUrlCollection>();
                foreach (Route.OutputUrlCollection x in rq.OutputUrlCollections)
                    ucs.Add(new Queue.OutputUrlCollection { Queue = queues.Where(z => z.Name == x.Queue).First(), Xpath = x.Xpath });

                List<Queue.OutputElementCollection> ecs = new List<Queue.OutputElementCollection>();
                foreach (Route.OutputElementCollection x in rq.OutputElementCollections)
                    ecs.Add(new Queue.OutputElementCollection { Queue = queues.Where(z => z.Name == x.Queue).First(), Xpath = x.Xpath });

                Queue q = queues.Where(z => z.Name == rq.Name).First();
                q.OutputUrlCollections = ucs;
                q.OutputElementCollections = ecs;
            }
            queues.Reverse();

            tw = new StreamWriter(Log.MainSession.Path + "\\output_" + route.Name + ".csv");
            List<string> hs = new List<string>();
            foreach (Queue q in queues)
                hs.InsertRange(0, q.OutputFields.Select(f => f.Name));
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
                        if (ii is Queue.InputUrl
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
                }
            }
        }
        bool run = false;

        Queue.InputItem get_next_item()
        {
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
            }

            public class InputUrl : InputItem
            {
            }

            public class InputElement : InputItem
            {
            }
            
          abstract  public class Action
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

            public class OutputUrlCollection
            {
                public string Xpath;
                public Queue Queue;
            }

            public class OutputElementCollection
            {
                public string Xpath;
                public Queue Queue;
            }

            public class OutputField
            {
                public string Name;
                public string Xpath;
                public string Attribute;
                //public bool StripHtml;
            }

            public List<InputItem> InputItems = new List<InputItem>();
            public List<Action> Actions = new List<Action>();
            public List<OutputUrlCollection> OutputUrlCollections = new List<OutputUrlCollection>();
            public List<OutputElementCollection> OutputElementCollections = new List<OutputElementCollection>();
            public List<OutputField> OutputFields = new List<OutputField>();

            public void ProcessItem(InputItem ii)
            {
                string base_xpath = "";
                if (ii is InputUrl)
                {
                    MainWindow.This.Browser.Load(ii.Value, true);
                }
                else if (ii is InputElement)
                {
                    base_xpath = ii.Value;
                }
                else
                    throw new Exception("Unknown type: " + ii.GetType());

                foreach (Action a in Actions)
                    a.Perform();

                string url = MainWindow.This.Browser.Url;
                foreach (OutputUrlCollection uc in OutputUrlCollections)
                {
                    foreach (string l in get_links(base_xpath + uc.Xpath))
                        uc.Queue.InputItems.Add(new Controller.Queue.InputUrl { Value = l, Queue = uc.Queue, ParentItem = ii });
                }

                foreach (OutputElementCollection ec in OutputElementCollections)
                {
                    //int el = get_elements(base_xpath + ec.Xpath);
                    //for (int i = 0; i < el; i++)
                    //ec.Queue.Items.Add(new Controller.Queue.Item { Type = Route.Item.Types.HTML_ELEMENT_KEY, Value = i.ToString(), Queue = ec.Queue, ParentItem = item });
                    foreach (string x in get_single_element_xpaths(base_xpath + ec.Xpath))
                        ec.Queue.InputItems.Add(new Controller.Queue.InputElement { Value = x, Queue = ec.Queue, ParentItem = ii });
                }

                foreach (OutputField f in OutputFields)
                {
                    string v = null;
                    if (f.Attribute == Route.OutputField.INNER_TEXT)
                    {
                        v = get_value(base_xpath + f.Xpath, Route.OutputField.INNER_HTML);
                        v = FieldPreparation.Html.Normalize(v);
                    }
                    else
                        v = get_value(base_xpath + f.Xpath, f.Attribute);
                    ii.OutputValues.Add(v);
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

                if (ls.Count > 3)
                {
                    Log.Warning("While debugging only first 3 links are taken of actual " + ls.Count);
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
    vs += '\r\n' + " + (attribute == Route.OutputField.INNER_HTML ? @"es[i].innerText" : @"es[i].getAttribute('" + attribute + @"')") + @";
}
return vs;
            ");
            }
        }
    }
}