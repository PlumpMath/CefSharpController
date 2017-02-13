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
    public class Route2
    {
        public static Route2 LoadFromFile(string file)
        {
            Route2 r = new Route2();
            r.Xml = File.ReadAllText(file);
            return r;
        }
        Route2() { }

        public Route2(string name)
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
                var xn = xd.SelectSingleNode("//Route");
                if (xn == null)
                    return null;
                return xn.Attributes["name"].Value;
            }
            set
            {
                var xn = xd.SelectSingleNode("//Route");
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

        public string ProductListUrl
        {
            get
            {
                var xn = xd.SelectSingleNode("//Route/StartUrl");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                try
                {
                    var xn = xd.SelectSingleNode("//Route/StartUrl");
                    if (xn == null)
                    {
                        var x = xd.SelectSingleNode("//Route");
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
                var xn = xd.SelectSingleNode("//Route/NextPageXpath");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                var xn = xd.SelectSingleNode("//Route/NextPageXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//Route");
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
                var xn = xd.SelectSingleNode("//Route/ProductPagesXpath");
                if (xn == null)
                    return null;
                return xn.InnerText;
            }
            set
            {
                var xn = xd.SelectSingleNode("//Route/ProductPagesXpath");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//Route");
                    xn = xd.CreateElement("ProductPagesXpath");
                    x.AppendChild(xn);
                }
                xn.InnerText = value;
            }
        }

        public class ProductField
        {
            public string Name;
            public string Xpath;
            public string Attribute;
        }

        public List<ProductField> ProductFields
        {
            get
            {
                List<ProductField> ps = new List<ProductField>();
                foreach (XmlNode xn in xd.SelectNodes("//Route/ProductPage/Field"))
                {
                    ps.Add(new ProductField { Name = xn.Attributes["name"].Value, Xpath = xn.Attributes["xpath"].Value, Attribute = xn.Attributes["attribute"].Value });
                }
                return ps;
            }
        }

        public void SetProductField(ProductField pf)
        {
            //get
            //{
            //    Dictionary<string, string> d = new Dictionary<string, string>();
            //    foreach (XmlNode xn in xd.SelectNodes("//Route/ProductPage/Value"))
            //    {
            //        d[xn.Attributes["name"].Value] = xn.Attributes["xpath"].Value;
            //    }
            //    return d;
            //}
            XmlNode xn = xd.SelectSingleNode("//Route/ProductPage");
                if (xn == null)
                {
                    var x = xd.SelectSingleNode("//Route");
                    xn = xd.CreateElement("ProductPage");
                    x.AppendChild(xn);
                }
                XmlNode xf = xd.SelectSingleNode("//Route/ProductPage/Field[@name='" + pf.Name + "']");
                if (xf == null)
                {
                    xf = xd.CreateElement("Field");
                    xn.AppendChild(xf);
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

        public void Save()
        {
            xd.Save(Log.WorkDir + "\\" + Name);
        }
    }
}

/*
<Route name="test.xml">
    <StartUrl>
        http://boston.craigslist.org/search/ata
    </StartUrl>
    <NextPageXpath>
        /html/body/section/form/div[3]/div[3]/span[2]/a[3]
    </NextPageXpath>
    <ProductPagesXpath>
        /html/body/section/form/div[4]/ul/li[*]/p/a
    </ProductPagesXpath>
    <ProductPage>
        <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
        <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
    </ProductPage>
</Route>     
*/

/*
<Route name="test.xml">
    <Queue name="Start">
        <Items>
            <Url value=""/>
            <Url value=""/>
            <Url value=""/>
        </Items>
        <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
        <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
    </Queue>
    <Queue name="NextPageList">
        <Url xpath="/html/body/section/form/div[3]/div[3]/span[2]/a[3]" queue="NextPageList"/>
        <Url xpath="/html/body/section/form/div[4]/ul/li[*]/p/a" queue="Product"/>
    </Queue>
    <Queue name="Product">
        <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
        <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
        <Url xpath="" queue="Product2"/>
    </Queue>
    <Queue name="Product2">
        <Field name="postingbody." xpath="/html/body/section/section/section/section" attribute="" />
        <Field name="postingbody.class" xpath="/html/body/section/section/section/section" attribute="class" />
    </Queue>
</Route>     
*/
