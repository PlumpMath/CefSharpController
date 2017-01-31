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
        public Route()
        {
            xd = new XmlDocument();
            var xn = xd.CreateElement("ProductList");
            xd.AppendChild(xn);
        }
        XmlDocument xd = null;

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
                var xn = xd.SelectSingleNode("//ProductList/StartUrl");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                try
                {
                    var xn = xd.SelectSingleNode("//ProductList/StartUrl");
                    if (xn == null)
                    {
                        var x = xd.SelectSingleNode("//ProductList");
                        xn = xd.CreateElement("StartUrl");
                        x.AppendChild(xn);
                    }
                    xn.InnerText = value;
                }
                catch (Exception e)
                {

                }
            }
        }

        public string ProductListNextPageXpath
        {
            get
            {
                var xn = xd.SelectSingleNode("//ProductList/NextPageXpath");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                var xn = xd.SelectSingleNode("//ProductList/NextPageXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//ProductList");
                    xn = xd.CreateElement("NextPageXpath");
                    x.AppendChild(xn);
                }
                xn.InnerText = value;
            }
        }

        public string ProductPagesXpath
        {
            get
            {
                var xn = xd.SelectSingleNode("//ProductList/ProductPagesXpath");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                var xn = xd.SelectSingleNode("//ProductList/ProductPagesXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//ProductList");
                    xn = xd.CreateElement("ProductPagesXpath");
                    x.AppendChild(xn);
                }
                xn.InnerText = value;
            }
        }

        public class ProductFieldClass
        {
            public string Name;
            public string Xpath;
            public string Attribute;
        }

        public List<ProductFieldClass> ProductFields
        {
            get
            {
                List<ProductFieldClass> ps = new List<ProductFieldClass>();
                foreach (XmlNode xn in xd.SelectNodes("//ProductList/ProductPage/Field"))
                {
                    ps.Add(new ProductFieldClass { Name = xn.Attributes["name"].Value, Xpath = xn.Attributes["xpath"].Value, Attribute = xn.Attributes["attribute"].Value });
                }
                return ps;
            }
        }

        public ProductFieldClass ProductField
        {
            //get
            //{
            //    Dictionary<string, string> d = new Dictionary<string, string>();
            //    foreach (XmlNode xn in xd.SelectNodes("//ProductList/ProductPage/Value"))
            //    {
            //        d[xn.Attributes["name"].Value] = xn.Attributes["xpath"].Value;
            //    }
            //    return d;
            //}
            set
            {
                XmlNode xn = xd.SelectSingleNode("//ProductList/ProductPage");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//ProductList");
                    xn = xd.CreateElement("ProductPage");
                    x.AppendChild(xn);
                }
                XmlNode xf = xd.SelectSingleNode("//ProductList/ProductPage/Field[@name='"+value.Name+"']");
                if (xf == null)
                {
                    xf = xd.CreateElement("Field");
                    xn.AppendChild(xf);
                    XmlAttribute a = xd.CreateAttribute("name");
                    a.Value = value.Name;
                    xf.Attributes.Append(a);
                }
                {
                    XmlAttribute a = xd.CreateAttribute("xpath");
                    a.Value = value.Xpath;
                    xf.Attributes.Append(a);
                    a = xd.CreateAttribute("attribute");
                    a.Value = value.Attribute;
                    xf.Attributes.Append(a);
                }
            }
        }

        public void Save(string file)
        {
            xd.Save(file);
        }
    }
}