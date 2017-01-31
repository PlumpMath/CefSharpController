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
        Controller(Route route)
        {
            this.route = route;
            tw = new StreamWriter(Log.MainSession.Path + "\\output.txt");
            tw.WriteLine(FieldPreparation.GetCsvLine(route.ProductFields.Select(x=>x.Name), FieldPreparation.FieldSeparator.COMMA, false));
        }
        TextWriter tw = null;
        Route route = null;

        public static void Start(Route route)
        {
            t = ThreadRoutines.StartTry(() => {
                c = new Controller(route);
                MainWindow.Load(route.ProductListUrl, false);
                c.ProcessProductListPage();
            });
        }
        static Controller c;
        static Thread t;

        public static void Stop()
        {
            if (t != null && t.IsAlive)
                t.Abort();
            c = null;
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
            foreach (Route.ProductFieldClass p in route.ProductFields)
            {
                vs.Add((string) MainWindow.Execute(""));
            }
            tw.WriteLine(FieldPreparation.GetCsvLine(vs, FieldPreparation.FieldSeparator.COMMA, false));
        }        
    }
}