﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace Cliver.CefSharpController
{
    public partial class CefSharpBrowser
    {
        public CefSharpBrowser(ChromiumWebBrowser browser, string js_callback_object, object object_accessed_by_js)
        {
            this.browser = browser;
            //browser.RegisterAsyncJsObject("JsMapObject", objectToBind);
            if (js_callback_object != null)
                browser.RegisterAsyncJsObject(js_callback_object, object_accessed_by_js);
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
        }
        ChromiumWebBrowser browser = null;

        public string Url
        {
            get
            {
                return (string)browser.Dispatcher.Invoke(() =>
                {
                    return browser.Address;
                });
            }
        }

        public static void DoEvents()
        {
            if (Application.Current == null)
                return;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        public void Load(string url, bool sync)
        {
            browser.Dispatcher.Invoke(() =>
            {
                completed = false;
                browser.Load(url);
            });
            if (!sync)
                return;
            WaitForCompletion();
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;
            completed = true;
        }
        bool completed = false;

        public void WaitForCompletion()
        {
            browser.Dispatcher.Invoke(() =>
            {
                while (!completed || browser.IsLoading)
                    DoEvents();
                //Thread.Sleep(100);
            });
        }

        public void Stop()
        {
            browser.Stop();
        }
    }
}
