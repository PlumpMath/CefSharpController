using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            This = this;

            InitializeComponent();

            Closing += delegate
              {
                  browser.Stop();
              };

            //browser = new ChromiumWebBrowser();
            //browser.RegisterJsObject("JsMapObject", route);
            //browser.HorizontalAlignment = HorizontalAlignment.Stretch;
            //browser.VerticalAlignment = VerticalAlignment.Stretch;
            //Grid.SetColumn(browser, 1);
            //grid.Children.Add(browser);
            browser.RegisterAsyncJsObject("JsMapObject", route);

            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            //browser.RequestHandler = new RequestHandler();



            //IJavaScriptExecutor js = browser as IJavaScriptExecutor;
            //string title = (string)js.ExecuteScript("return document.title");


            //var jsDriver = (IJavaScriptExecutor)driver;
            //var element = // some element you find;
            //string highlightJavascript = @"arguments[0].style.cssText = ""border-width: 2px; border-style: solid; border-color: red"";";
            //jsDriver.ExecuteScript(highlightJavascript, new object[] { element });

        }
        //ChromiumWebBrowser browser;
        public static MainWindow This { get; private set; }
        
     static public   string Url
        {
            get
            {
                return (string)MainWindow.This.Dispatcher.Invoke(() =>
                {
                    return MainWindow.Browser.Address;
                });
            }
        }

        //class RequestHandler : IRequestHandler
        //{
        //    public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }

        //    public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        //    {
        //        return null;
        //        throw new NotImplementedException();
        //    }

        //    public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
        //    {
        //        return !Block;
        //    }
        //    public bool Block = false;

        //    public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        //    {
        //        return new CefReturnValue();
        //        throw new NotImplementedException();
        //    }

        //    public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }

        //    public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }

        //    public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        //    {
        //        //throw new NotImplementedException();
        //    }

        //    public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }

        //    public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }

        //    public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        //    {
        //        return;
        //        throw new NotImplementedException();
        //    }

        //    public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        //    {
        //        //throw new NotImplementedException();
        //    }

        //    public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        //    {
        //        //throw new NotImplementedException();
        //    }

        //    public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref string newUrl)
        //    {
        //        //throw new NotImplementedException();
        //    }

        //    public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        //    {
        //        return true;
        //        throw new NotImplementedException();
        //    }
        //}

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;
            completed = true;
        }
        bool completed = false;

       static public ChromiumWebBrowser Browser
        {
            get
            {
                return This.browser;
            }
        }

        static public void Load(string url, bool sync)
        {
            This.Dispatcher.Invoke(() =>
            {
                This.completed = false;
                This.browser.Load(url);
                if (!sync)
                    return;
            });
            WaitForCompletion();
        }

        static public void WaitForCompletion()
        {
            This.Dispatcher.Invoke(() =>
            {
                while (!This.completed || This.browser.IsLoading)
                    DoEvents();
                    //Thread.Sleep(100);
            });
        }

        public static void DoEvents()
        {
            if (Application.Current == null)
                return;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        static public object Execute(string script)
        {
            var t = This.browser.EvaluateScriptAsync(
@"(function(){
    try{
    " + script + @"
    }catch(err){
        alert(err.message);
    }
}())");
            while(!t.IsCompleted)
                DoEvents();
            return t.Result.Result;
        }

        void h()
        {
            string js = @"
            function createXPathFromElement(elm) {
                var allNodes = document.__getElementsByTagName('*');
                for (var segs = []; elm && elm.nodeType == 1; elm = elm.parentNode)
                {
                    if (elm.hasAttribute('id'))
                    {
                        var uniqueIdCount = 0;
                        for (var n = 0; n < allNodes.length; n++)
                        {
                            if (allNodes[n].hasAttribute('id') && allNodes[n].id == elm.id) uniqueIdCount++;
                            if (uniqueIdCount > 1) break;
                        };
                        if (uniqueIdCount == 1)
                        {
                            segs.unshift('id(""' + elm.getAttribute('id') + '"")');
                            return segs.join('/');
                        }
                        else
                        {
                            segs.unshift(elm.localName.toLowerCase() + '[@id=""' + elm.getAttribute('id') + '""]');
                        }
                    }
                    else if (elm.hasAttribute('class'))
                    {
                        segs.unshift(elm.localName.toLowerCase() + '[@class=""' + elm.getAttribute('class') + '""]');
                    }
                    else
                    {
                        for (i = 1, sib = elm.previousSibling; sib; sib = sib.previousSibling)
                        {
                            if (sib.localName == elm.localName) i++;
                        };
                        segs.unshift(elm.localName.toLowerCase() + '[' + i + ']');
                    };
                };
                return segs.length ? '/' + segs.join('/') : null;
            };

            function lookupElementByXPath(path) {
                var evaluator = new XPathEvaluator();
                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null);
                return result.singleNodeValue;
            }
        }";
        }

        static public void Stop()
        {
            This.browser.Stop();
        }

        //static public string State
        //{
        //    set { This.state.Content = value; }
        //}
    }
}
