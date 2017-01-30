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
        XmlDocument xd = new XmlDocument();

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

        public string ProductListUrl
        {
            get
            {
                return xd.SelectSingleNode("//ProductList/StartUrl").Value;
            }
            set { }
        }

        public string ProductListNextPageXpath
        {
            get
            {
                var xn = xd.SelectSingleNode("//ProductList/NextPageXpath");
                if (xn == null)
                    return null;
                return xn.Value;
            }
            set
            {
                var xn = xd.SelectSingleNode("//ProductList/NextPageXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//ProductList");
                    xn = xd.CreateElement("NextPageXpath");
                    xn.Value = value;
                    x.AppendChild(xn);
                }
            }
        }

        public string ProductPagesXpath
        {
            get
            {
                var xn = xd.SelectSingleNode("//ProductList/ProductPagesXpath");
                if (xn == null)
                    return null;
                return xn.Value;
            }
            set
            {
                var xn = xd.SelectSingleNode("//ProductList/ProductPagesXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//ProductList");
                    xn = xd.CreateElement("ProductPagesXpath");
                    xn.Value = value;
                    x.AppendChild(xn);
                }
            }
        }

        public Dictionary<string, string> ProductValueNames2Xpath
        {
            get
            {
                Dictionary<string, string> d = new Dictionary<string, string>();
                foreach (XmlNode xn in xd.SelectNodes("//ProductList/ProductPage/Value"))
                {
                    d[xn.Attributes["name"].Value] = xn.Attributes["xpath"].Value;
                }
                return d;
            }
            set
            {
                XmlNode xn = xd.SelectSingleNode("//ProductList/ProductPage");
                xn.RemoveAll();
                foreach (string n in value.Keys)
                {
                    XmlNode x = xd.CreateElement("Value");
                    var a = xd.CreateAttribute("name");
                    a.Value = n;
                    x.Attributes.Append(a);
                    a = xd.CreateAttribute("xpath");
                    a.Value = value[n];
                    x.Attributes.Append(a);
                    xn.AppendChild(x);
                }
            }
        }

        public void Save(string file)
        {
            xd.Save(file);
        }
    }
}