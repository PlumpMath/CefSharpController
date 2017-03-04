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
using System.Xml;

/*TBD

    - prevent looping (not load old urls);
    - click instead of load (keep the previous document if reused)
    - allow repeating right-click when picking up data field
    - recognize if an output is url or element


*/

namespace Cliver.CefSharpController
{
    public partial class Route
    {
        public Route(string file)
        {
            Xml = File.ReadAllText(file);
        }

        public Route()
        {
        }
        readonly XmlDocument xd = new XmlDocument();

        public string Xml
        {
            set
            {
                xd.LoadXml(value);
            }
            get
            {
                return xd.OuterXml;
            }
        }

        public string Name
        {
            get
            {
                var xn = xd.SelectSingleNode("Route");
                if (xn == null)
                    return null;
                var n = xn.Attributes["name"];
                if (n == null)
                    return null;
                return n.Value;
            }
            set
            {
                var xn = xd.SelectSingleNode("Route");
                if (xn == null)
                {
                    xn = xd.CreateElement("Route");
                    xd.AppendChild(xn);
                }
                XmlAttribute a = xd.CreateAttribute("name");
                a.Value = value;
                xn.Attributes.Append(a);
            }
        }

        public void SetOutput(string queue_name, Output output)
        {
            XmlNode xo = get_queue_child_node(queue_name, QueueNodes.Outputs);
            if (output is Output.UrlCollection)
            {
                Output.UrlCollection ucs = (Output.UrlCollection)output;
                XmlNode xu = xo.SelectSingleNode(ucs.Tag + "[@queue='" + ucs.Queue + "']");
                if (xu == null)
                {
                    xu = xd.CreateElement(ucs.Tag);
                    xo.AppendChild(xu);
                    XmlAttribute a = xd.CreateAttribute("queue");
                    a.Value = ucs.Queue;
                    xu.Attributes.Append(a);

                    get_queue_node(ucs.Queue);//create the accepting queue
                }
                {
                    XmlAttribute a = xd.CreateAttribute("queuing_manner");
                    a.Value = ucs.QueuingManner.ToString();
                    xu.Attributes.Append(a);

                    a = xd.CreateAttribute("xpath");
                    a.Value = ucs.Xpath;
                    xu.Attributes.Append(a);
                }
            }
            else if (output is Output.ElementCollection)
            {
                Output.ElementCollection ucs = (Output.ElementCollection)output;
                XmlNode xu = xo.SelectSingleNode(ucs.Tag + "[@queue='" + ucs.Queue + "']");
                if (xu == null)
                {
                    xu = xd.CreateElement(ucs.Tag);
                    xo.AppendChild(xu);
                    XmlAttribute a = xd.CreateAttribute("queue");
                    a.Value = ucs.Queue;
                    xu.Attributes.Append(a);

                    get_queue_node(ucs.Queue);//create the accepting queue
                }
                {
                    XmlAttribute a = xd.CreateAttribute("xpath");
                    a.Value = ucs.Xpath;
                    xu.Attributes.Append(a);
                }
            }
            else if (output is Output.Field)
            {
                Output.Field d = (Output.Field)output;
                XmlNode xn = xo.SelectSingleNode(d.Tag + "[@xpath='" + d.Xpath + "']");
                if (xn == null)
                {
                    xn = xd.CreateElement(d.Tag);
                    xo.AppendChild(xn);
                    XmlAttribute a = xd.CreateAttribute("xpath");
                    a.Value = d.Xpath;
                    xn.Attributes.Append(a);
                }
                {
                    XmlAttribute a = xd.CreateAttribute("attribute");
                    a.Value = d.Attribute;
                    xn.Attributes.Append(a);

                    a = xd.CreateAttribute("name");
                    a.Value = d.Name;
                    xn.Attributes.Append(a);
                }
            }
        }

        abstract public class Output
        {
            public string Tag
            {
                get
                {
                    return this.GetType().Name;
                }
            }

            public class UrlCollection : Output
            {
                public string Queue;
                public string Xpath;
                public QueuingManners QueuingManner = QueuingManners.FIFO;
                public enum QueuingManners
                {
                    FIFO,
                    LIFO
                }
            }

            public class ElementCollection : Output
            {
                public string Queue;
                public string Xpath;
            }

            public class Field : Output
            {
                public string Name;
                public string Xpath;
                public string Attribute;

                public const string INNER_HTML = "INNER_HTML";
                public const string INNER_TEXT = "INNER_TEXT";
            }
        }

        public void Save(string file)
        {
            if (Name == null)
                Name = PathRoutines.GetFileNameWithoutExtentionFromPath(file);
            xd.Save(file);
        }

        public void AddAction(string queue_name, Action action)
        {
            XmlNode xin = get_queue_child_node(queue_name, QueueNodes.Actions);
            XmlNode xi = xd.CreateElement(action.Tag);
            xin.AppendChild(xi);

            if (action is Action.Set)
            {
                Action.Set s = (Action.Set)action;
                XmlAttribute a = xd.CreateAttribute("xpath");
                if (string.IsNullOrEmpty(s.Xpath))
                    throw new Exception("Xpath is empty");
                a.Value = s.Xpath;
                xi.Attributes.Append(a);

                a = xd.CreateAttribute("attribute");
                a.Value = s.Attribute;
                xi.Attributes.Append(a);

                a = xd.CreateAttribute("value");
                a.Value = s.Value;
                xi.Attributes.Append(a);
            }
            else if (action is Action.Click)
            {
                Action.Click c = (Action.Click)action;
                XmlAttribute a = xd.CreateAttribute("xpath");
                if (string.IsNullOrEmpty(c.Xpath))
                    throw new Exception("Xpath is empty");
                a.Value = c.Xpath;
                xi.Attributes.Append(a);
            }
            else if (action is Action.WaitDocumentLoaded)
            {
                Action.WaitDocumentLoaded w = (Action.WaitDocumentLoaded)action;
                XmlAttribute a = xd.CreateAttribute("minimal_sleep_mss");
                a.Value = w.MinimalSleepMss.ToString();
                xi.Attributes.Append(a);
            }
            else
                throw new Exception("Unknown type: " + action.GetType());
        }

        public abstract class Action
        {
            public string Tag
            {
                get
                {
                    return this.GetType().Name;
                }
            }

            public class Set : Action
            {
                public string Xpath;
                public string Value;
                public string Attribute;
            }

            public class Click : Action
            {
                public string Xpath;
            }

            public class WaitDocumentLoaded : Action
            {
                public int MinimalSleepMss;
            }
        }

        public void AddInputItem(string queue_name, InputItem ii)
        {
            XmlNode xin = get_queue_child_node(queue_name, QueueNodes.InputItems);
            XmlNode xi = xd.CreateElement(ii.Tag);
            xin.AppendChild(xi);
            XmlAttribute a = xd.CreateAttribute("value");
            if (string.IsNullOrEmpty(ii.Value))
                throw new Exception("Value is empty");
            a.Value = ii.Value;
            xi.Attributes.Append(a);
        }

        public abstract class InputItem
        {
            public string Tag
            {
                get
                {
                    return this.GetType().Name;
                }
            }

            public string Value;
            //public enum Types
            //{
            //    Url,
            //    Element
            //}

            public class Url : InputItem
            {
            }

            public class Element : InputItem
            {
            }
        }

        XmlNode get_queue_node(string queue_name)
        {
            XmlNode xq = xd.SelectSingleNode("Route/Queue[@name='" + queue_name + "']");
            if (xq == null)
            {
                var xr = xd.SelectSingleNode("Route");
                if (xr == null)
                {
                    xr = xd.CreateElement("Route");
                    xd.AppendChild(xr);
                }
                xq = xd.CreateElement("Queue");
                xr.AppendChild(xq);
                XmlAttribute a = xd.CreateAttribute("name");
                a.Value = queue_name;
                xq.Attributes.Append(a);
            }
            return xq;
        }

        XmlNode get_queue_child_node(string queue_name, QueueNodes node)
        {
            XmlNode xq = get_queue_node(queue_name);
            string node_tag = node.ToString();
            XmlNode xi = xq.SelectSingleNode(node_tag);
            if (xi == null)
            {
                xi = xd.CreateElement(node_tag);
                xq.AppendChild(xi);
            }
            return xi;
        }

        public enum QueueNodes
        {
            InputItems,
            Actions,
            Outputs
        }

        public List<Queue> GetQueues()
        {
            List<Queue> qs = new List<Queue>();
            foreach (XmlNode xq in xd.SelectNodes("Route/Queue"))
            {
                List<InputItem> iis = new List<InputItem>();
                {
                    XmlNode xi = xq.SelectSingleNode(QueueNodes.InputItems.ToString());
                    if (xi != null)
                    {
                        foreach (XmlNode x in xi.SelectNodes("*"))
                        {
                            switch (x.Name)
                            {
                                case "Url":
                                    iis.Add(new InputItem.Url { Value = x.Attributes["value"].Value });
                                    break;
                                case "Element":
                                    iis.Add(new InputItem.Element { Value = x.Attributes["value"].Value });
                                    break;
                                default:
                                    throw new Exception("Unknown option!");
                            }
                        }
                    }
                }

                List<Action> as_ = new List<Action>();
                {
                    XmlNode xa = xq.SelectSingleNode(QueueNodes.Actions.ToString());
                    if (xa != null)
                    {
                        foreach (XmlNode x in xa.SelectNodes("*"))
                        {
                            switch (x.Name)
                            {
                                case "Set":
                                    as_.Add(new Action.Set { Xpath = x.Attributes["xpath"].Value, Attribute = x.Attributes["attribute"].Value, Value = x.Attributes["value"].Value });
                                    break;
                                case "Click":
                                    as_.Add(new Action.Click { Xpath = x.Attributes["xpath"].Value });
                                    break;
                                case "WaitDocumentLoaded":
                                    as_.Add(new Action.WaitDocumentLoaded { MinimalSleepMss = int.Parse(x.Attributes["minimal_sleep_mss"].Value) });
                                    break;
                                default:
                                    throw new Exception("Unknown option!");
                            }
                        }
                    }
                }

                List<Output> os = new List<Output>();
                {
                    XmlNode xo = xq.SelectSingleNode(QueueNodes.Outputs.ToString());
                    if (xo != null)
                    {
                        foreach (XmlNode x in xo.ChildNodes)
                        {
                            Output o;
                            if (x.Name == "UrlCollection")
                                o = new Output.UrlCollection { Queue = x.Attributes["queue"].Value, Xpath = x.Attributes["xpath"].Value, QueuingManner = (Output.UrlCollection.QueuingManners)Enum.Parse(typeof(Output.UrlCollection.QueuingManners), x.Attributes["queuing_manner"].Value) };
                            else if (x.Name == "ElementCollection")
                                o = new Output.ElementCollection { Queue = x.Attributes["queue"].Value, Xpath = x.Attributes["xpath"].Value };
                            else if (x.Name == "Field")
                                o = new Output.Field { Attribute = x.Attributes["attribute"].Value, Xpath = x.Attributes["xpath"].Value, Name = x.Attributes["name"].Value };
                            else
                                throw new Exception("Unknown type: " + x.Name);
                            os.Add(o);
                        }
                    }
                }

                Queue q = new Queue
                {
                    Name = xq.Attributes["name"].Value,
                    InputItems = iis, 
                    Actions = as_,
                    Outputs = os
                };
                qs.Add(q);
            }
            return qs;
        }
        public class Queue
        {
            public string Name;
            public List<InputItem> InputItems = new List<InputItem>();
            public List<Action> Actions = new List<Action>();
            public List<Output> Outputs = new List<Output>();
        }
    }
}

/*
#0
<Route name="test.xml">
    <Queue name="Start">
        <Input>
            <Url value=""/>
        </Input>
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <UrlCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="ListNextPage">
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <UrlCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="Product">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
            <UrlCollection xpath="" queue="Product2"/>
        </Output>
    </Queue>
    <Queue name="Product2">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
        </Output>
    </Queue>
</Route>    
 
#1
<Route name="test.xml">
    <Queue name="Start">
        <Input>
            <Url value=""/>
            <Element value=""/>
        </Input>
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <ElementCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="ListNextPage">
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <ElementCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="Product">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
        </Output>
    </Queue>
</Route>

#3
<Route name="test.xml">
    <Queue name="Start">
        <Input>
            <Url value=""/>
        </Input>
        <Actions>
            <Set xpath="/html/body/section/form/div[3]/div[3]/span[2]/input[3]" value="" />
            <Click xpath="/html/body/section/form/div[3]/div[3]/span[2]/input[4]" />
            <WaitDocumentLoaded minimal_sleep_mss="500" />
        </Actions>
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <UrlCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="ListNextPage">
        <Output>
            <UrlCollection xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <UrlCollection xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="Product">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
            <UrlCollection xpath="" queue="Product2"/>
        </Output>
    </Queue>
    <Queue name="Product2">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
        </Output>
    </Queue>
</Route>         
*/
