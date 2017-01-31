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

namespace Cliver.CefSharpController
{
    public class Controller
    {
        public Controller(Route route)
        {
            this.route = route;
            tw = new StreamWriter(Log.MainSession.Path + "\\output.txt");
            tw.WriteLine(FieldPreparation.GetCsvLine(route.ProductValueNames2Xpath.Keys, FieldPreparation.FieldSeparator.COMMA, false));
        }
        TextWriter tw = null;
        Route route = null;

        void Start()
        {
            t = ThreadRoutines.StartTry(() => {
                MainWindow.Load(route.ProductListUrl, false);
                ProcessProductListPage();
            });
        }
        Thread t;

        void Stop()
        {
            if (t != null && t.IsAlive)
                t.Abort();
            MainWindow.Stop();
        }

        void ProcessProductListPage()
        {
            string next_page_list_url = (string)MainWindow.Execute("");
            List<string> product_page_urls = (List<string>)MainWindow.Execute("");
            foreach (string ppu in product_page_urls)
            {
                MainWindow.Load(ppu, false);
                ProcessProductPage();
            }
            if(!string.IsNullOrWhiteSpace( next_page_list_url))
            {
                Log.Warning("no next page found");
                return;
            }
            MainWindow.Load(next_page_list_url, false);
            ProcessProductListPage();
        }

        void ProcessProductPage()
        {
            List<string> vs = new List<string>();
            foreach (string vn in route.ProductValueNames2Xpath.Keys)
            {
                vs.Add((string) MainWindow.Execute(""));
            }
            tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, false));
        }        
    }
}