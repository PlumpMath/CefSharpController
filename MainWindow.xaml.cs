using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

            browser.Loaded += Browser_Loaded;
            browser.LoadingStateChanged += Browser_LoadingStateChanged;
            browser.RequestHandler = new RequestHandler();



            //IJavaScriptExecutor js = browser as IJavaScriptExecutor;
            //string title = (string)js.ExecuteScript("return document.title");

            browser.RegisterJsObject("JS_OBJECT", this);
            //window.JS_OBJECT.HtmlElementClicked()

            //var jsDriver = (IJavaScriptExecutor)driver;
            //var element = // some element you find;
            //string highlightJavascript = @"arguments[0].style.cssText = ""border-width: 2px; border-style: solid; border-color: red"";";
            //jsDriver.ExecuteScript(highlightJavascript, new object[] { element });
        }
        public static MainWindow This { get; private set; }

        class RequestHandler : IRequestHandler
        {
            public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
            {
                throw new NotImplementedException();
            }

            public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                throw new NotImplementedException();
            }

            public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
            {
                return !Block;
            }
            public bool Block = false;

            public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
                throw new NotImplementedException();
            }

            public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
            {
                throw new NotImplementedException();
            }

            public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
            {
                throw new NotImplementedException();
            }

            public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
            {
                throw new NotImplementedException();
            }

            public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
            {
                throw new NotImplementedException();
            }

            public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
            {
                throw new NotImplementedException();
            }

            public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
            {
                throw new NotImplementedException();
            }

            public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
            {
                throw new NotImplementedException();
            }

            public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
            {
                throw new NotImplementedException();
            }

            public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref string newUrl)
            {
                throw new NotImplementedException();
            }

            public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                throw new NotImplementedException();
            }
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
                return;
        }

        public void HtmlElementClicked(string xpath)
        {
        }

       static public ChromiumWebBrowser Browser
        {
            get
            {
                return This.browser;
            }
        }

        static public bool Load(string url)
        {
            This.browser.Load(url);
            return true;
        }

        static public object Execute(string script)
        {
            var t = This.browser.EvaluateScriptAsync(script);
            if (!t.Wait(1000))
                throw new Exception("Timeout");
            return t.Result.Result;
        }

        static public void Stop()
        {
            WebBrowserExtensions.Stop(This.browser);
        }

        static public string State
        {
            set { This.state.Content = value; }
        }
    }
}
