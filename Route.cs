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
using System.Xml;

namespace Cliver.CefSharpController
{
    public class Route
    {
        public static Route LoadFromFile(string file)
        {
            Route r = new Route();
            r.Xml = File.ReadAllText(file);
            return r;
        }
        Route() { }

        public Route(string name)
        {
            Name = name;
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
                return xn.Attributes["name"].Value;
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

        public void SetOutputUrl(string queue_name, Url url)
        {
            XmlNode xo = get_output_node(queue_name);
            XmlNode xu = xo.SelectSingleNode("Url[@queue='" + url.Queue + "']");
            if (xu == null)
            {
                xu = xd.CreateElement("Url");
                xo.AppendChild(xu);
                XmlAttribute a = xd.CreateAttribute("queue");
                a.Value = url.Queue;
                xu.Attributes.Append(a);
            }
            {
                XmlAttribute a = xd.CreateAttribute("xpath");
                a.Value = url.Xpath;
                xu.Attributes.Append(a);
            }
        }

        public class Url
        {
            public string Queue;
            public string Xpath;
        }

        public void SetOutputField(string queue_name, Field pf)
        {
            XmlNode xo = get_output_node(queue_name);
            XmlNode xf = xo.SelectSingleNode("Field[@name='" + pf.Name + "']");
            if (xf == null)
            {
                xf = xd.CreateElement("Field");
                xo.AppendChild(xf);
                XmlAttribute a = xd.CreateAttribute("name");
                a.Value = pf.Name;
                xf.Attributes.Append(a);
            }
            {
                XmlAttribute a = xd.CreateAttribute("xpath");
                a.Value = pf.Xpath;
                xf.Attributes.Append(a);
                a = xd.CreateAttribute("attribute");
                a.Value = pf.Attribute;
                xf.Attributes.Append(a);
            }
        }

        public class Field
        {
            public string Name;
            public string Xpath;
            public string Attribute;
        }

        public List<Field> Fields
        {
            get
            {
                List<Field> ps = new List<Field>();
                foreach (XmlNode xq in xd.SelectNodes("Route/Queue"))
                    foreach (XmlNode xn in xq.SelectNodes("Field"))
                        ps.Add(new Field { Name = xn.Attributes["name"].Value, Xpath = xn.Attributes["xpath"].Value, Attribute = xn.Attributes["attribute"].Value });
                return ps;
            }
        }

        public void Save()
        {
            xd.Save(Log.WorkDir + "\\" + Name);
        }

        public void AddInputItem(string queue_name, Item item)
        {
            XmlNode xin = get_input_node(queue_name);
            //XmlNode xq = xd.SelectSingleNode("Item[@name='" + queue_name + "']");
            XmlNode xi = xd.CreateElement("Item");
            xin.AppendChild(xi);
            XmlAttribute a = xd.CreateAttribute("url");
            a.Value = item.Url;
            xi.Attributes.Append(a);
            a = xd.CreateAttribute("xpath");
            a.Value = item.Xpath;
            xi.Attributes.Append(a);
        }

        public class Item
        {
            public string Url;
            public string Xpath;
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

        XmlNode get_input_node(string queue_name)
        {
            XmlNode xq = get_queue_node(queue_name);
            XmlNode xi = xq.SelectSingleNode("Input");
            if (xi == null)
            {
                xi = xd.CreateElement("Input");
                xq.AppendChild(xi);
            }
            return xi;
        }

        XmlNode get_output_node(string queue_name)
        {
            XmlNode xq = get_queue_node(queue_name);
            XmlNode xo = xq.SelectSingleNode("Output");
            if (xo == null)
            {
                xo = xd.CreateElement("Output");
                xq.AppendChild(xo);
            }
            return xo;
        }

        public class Queue
        {
            public string Name;
            public List<Item> Items = new List<Item>();
            public List<Url> Urls = new List<Url>();
            public List<Field> Fields = new List<Field>();
        }

        public List<Queue> GetQueues()
        {
            List<Queue> qs = new List<Queue>();
            foreach (XmlNode xq in xd.SelectNodes("Route/Queue"))
            {
                List<Item> li = new List<Item>();
                XmlNode xi = xq.SelectSingleNode("Input");
                if (xi != null)
                {
                    foreach (XmlNode x in xi.SelectNodes("Item"))
                        li.Add(new Item { Url = x.Attributes["url"].Value, Xpath = x.Attributes["xpath"].Value });
                }

                List<Url> us = new List<Url>();
                XmlNode xo = xq.SelectSingleNode("Output");
                foreach (XmlNode x in xo.SelectNodes("Url"))
                    us.Add(new Url { Queue = x.Attributes["queue"].Value, Xpath = x.Attributes["xpath"].Value });

                List<Field> fs = new List<Field>();
                foreach (XmlNode x in xo.SelectNodes("Field"))
                    fs.Add(new Field { Attribute = x.Attributes["attribute"].Value, Xpath = x.Attributes["xpath"].Value, Name = x.Attributes["name"].Value });

                var q = new Queue
                {
                    Name= xq.Attributes["name"].Value,
                    Fields = fs,
                    Urls = us,
                    Items = li
                };
                qs.Add(q);
            }
            return qs;
        }
    }
}

/*
#0
<Route name="test.xml">
    <Queue name="Start">
        <Input>
            <Item url="" xpath=""/>
            <Item url="" xpath=""/>
        </Input>
        <Output>
            <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="ListNextPage">
        <Output>
            <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="Product">
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
            <Url xpath="" queue="Product2"/>
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
            <Item url="" xpath=""/>
            <Item url="" xpath=""/>
        </Input>
        <Output>
            <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="ListNextPage">
        <Output>
            <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
            <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
        </Output>
    </Queue>
    <Queue name="Product">
        <Input>
            <Item url="" xpath=""/>
            <Item url="" xpath=""/>
        </Input>
        <Output>
            <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
            <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
            <Url xpath="" queue="Product2"/>
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
