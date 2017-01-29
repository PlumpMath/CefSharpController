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

namespace Cliver.CefSharpController
{
    public class Controller
    {
        static void Perform(string xml)
        {
            MainWindow.Load("");



        }
        

        static void ProcessProductListPage(string next_page_list_link_xpath, string product_page_links_xpath)
        {
            string next_page_list_url = (string)MainWindow.Execute("");
            List<string> product_page_urls = (List<string>)MainWindow.Execute("");
            foreach (string ppu in product_page_urls)
            {
                MainWindow.Load(ppu);
            }
            if(!string.IsNullOrWhiteSpace( next_page_list_url))
            {
                Log.Warning("no next page found");
                return;
            }
            MainWindow.Load(next_page_list_url);
            ProcessProductListPage(next_page_list_link_xpath, product_page_links_xpath);
        }

        static void ProcessProductPage(Dictionary<string, string> product_value_names2xpaths)
        {
            List<string> vs = new List<string>();
            foreach (string vn in product_value_names2xpaths.Keys)
            {
                vs.Add((string) MainWindow.Execute(""));
            }
            FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, false);
        }

        static void Save()
        { }

        static void GetProductListUrl()
        { }

        static void GetProductListNextPageXpath()
        { }

        static void GetProductPagesXpath()
        { }
    }
}