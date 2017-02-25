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

namespace Cliver.CefSharpController
{
    public class Route
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

        public void SetOutputUrlCollection(string queue_name, OutputUrlCollection url_collection)
        {
            XmlNode xo = get_queue_node(queue_name, QueueNodes.Output);
            XmlNode xu = xo.SelectSingleNode("UrlCollection[@queue='" + url_collection.Queue + "']");
            if (xu == null)
            {
                xu = xd.CreateElement("UrlCollection");
                xo.AppendChild(xu);
                XmlAttribute a = xd.CreateAttribute("queue");
                a.Value = url_collection.Queue;
                xu.Attributes.Append(a);
            }
            {
                XmlAttribute a = xd.CreateAttribute("xpath");
                a.Value = url_collection.Xpath;
                xu.Attributes.Append(a);
            }
        }

        public class OutputUrlCollection
        {
            public string Queue;
            public string Xpath;
        }

        public void SetOutputElementCollection(string queue_name, OutputElementCollection url_collection)
        {
            XmlNode xo = get_queue_node(queue_name, QueueNodes.Output);
            XmlNode xu = xo.SelectSingleNode("ElementCollection[@queue='" + url_collection.Queue + "']");
            if (xu == null)
            {
                xu = xd.CreateElement("ElementCollection");
                xo.AppendChild(xu);
                XmlAttribute a = xd.CreateAttribute("queue");
                a.Value = url_collection.Queue;
                xu.Attributes.Append(a);
            }
            {
                XmlAttribute a = xd.CreateAttribute("xpath");
                a.Value = url_collection.Xpath;
                xu.Attributes.Append(a);
            }
        }

        public class OutputElementCollection
        {
            public string Queue;
            public string Xpath;
        }

        public void SetOutputField(string queue_name, OutputField of)
        {
            XmlNode xo = get_queue_node(queue_name, QueueNodes.Output);
            XmlNode xf = xo.SelectSingleNode("Field[@name='" + of.Name + "']");
            if (xf == null)
            {
                xf = xd.CreateElement("Field");
                xo.AppendChild(xf);
                XmlAttribute a = xd.CreateAttribute("name");
                a.Value = of.Name;
                xf.Attributes.Append(a);
            }
            {
                XmlAttribute a = xd.CreateAttribute("xpath");
                a.Value = of.Xpath;
                xf.Attributes.Append(a);
                a = xd.CreateAttribute("attribute");
                a.Value = of.Attribute;
                xf.Attributes.Append(a);
                //a = xd.CreateAttribute("strip_html");
                //a.Value = of.StripHtml.ToString();
                //xf.Attributes.Append(a);
            }
        }

        public class OutputField
        {
            public string Name;
            public string Xpath;
            public string Attribute;
            //public bool StripHtml;

            public const string INNER_HTML = "INNER_HTML";
            public const string INNER_TEXT = "INNER_TEXT";
        }

        public List<OutputField> OutputFields
        {
            get
            {
                List<OutputField> ps = new List<OutputField>();
                foreach (XmlNode xq in xd.SelectNodes("Route/Queue"))
                    foreach (XmlNode xn in xq.SelectNodes("Field"))
                        ps.Add(new OutputField { Name = xn.Attributes["name"].Value, Xpath = xn.Attributes["xpath"].Value, Attribute = xn.Attributes["attribute"].Value });
                return ps;
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
            XmlNode xin = get_queue_node(queue_name, QueueNodes.Actions);
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
            XmlNode xin = get_queue_node(queue_name, QueueNodes.Input);
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

        XmlNode get_queue_node(string queue_name, QueueNodes node)
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
            Input,
            Actions,
            Output
        }

        public List<Queue> GetQueues()
        {
            List<Queue> qs = new List<Queue>();
            foreach (XmlNode xq in xd.SelectNodes("Route/Queue"))
            {
                List<InputItem> iis = new List<InputItem>();
                XmlNode xi = xq.SelectSingleNode("Input");
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

                XmlNode xo = xq.SelectSingleNode("Output");

                List<OutputUrlCollection> ucs = new List<OutputUrlCollection>();
                if (xo != null)
                    foreach (XmlNode x in xo.SelectNodes("UrlCollection"))
                        ucs.Add(new OutputUrlCollection { Queue = x.Attributes["queue"].Value, Xpath = x.Attributes["xpath"].Value });

                List<OutputElementCollection> ecs = new List<OutputElementCollection>();
                if (xo != null)
                    foreach (XmlNode x in xo.SelectNodes("ElementCollection"))
                        ecs.Add(new OutputElementCollection { Queue = x.Attributes["queue"].Value, Xpath = x.Attributes["xpath"].Value });

                List<OutputField> fs = new List<OutputField>();
                if (xo != null)
                    foreach (XmlNode x in xo.SelectNodes("Field"))
                        fs.Add(new OutputField { Attribute = x.Attributes["attribute"].Value, Xpath = x.Attributes["xpath"].Value, Name = x.Attributes["name"].Value });

                Queue q = new Queue
                {
                    Name = xq.Attributes["name"].Value,
                    OutputFields = fs,
                    OutputUrlCollections = ucs,
                    OutputElementCollections = ecs,
                    InputItems = iis
                };
                qs.Add(q);
            }
            return qs;
        }
        public class Queue
        {
            public string Name;
            public List<InputItem> InputItems = new List<InputItem>();
            public List<OutputUrlCollection> OutputUrlCollections = new List<OutputUrlCollection>();
            public List<OutputElementCollection> OutputElementCollections = new List<OutputElementCollection>();
            public List<OutputField> OutputFields = new List<OutputField>();
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
