//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace Cliver.CefSharpController
{
    public partial class CefSharpBrowser
    {
        public CefSharpBrowser(ChromiumWebBrowser browser, string js_callback_object, object object_accessed_by_js)
        {
            this.browser = browser;
            browser.RequestHandler = new RequestHandler();
            //browser.BrowserSettings.ImageLoading = load_images ? CefState.Enabled : CefState.Disabled;
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
                Log.Main.Inform("Loading: " + Url);
            completed = !e.IsLoading;
        }
        bool completed = false;

        public void WaitForCompletion()
        {
            while (!completed)
            {
                DoEvents();
                Thread.Sleep(50);
            };
        }

        public void Stop()
        {
            browser.Stop();
        }

        public bool LoadImages
        {
            get
            {
                return ((RequestHandler)browser.RequestHandler).LoadImages;
            }
            set
            {
                ((RequestHandler)browser.RequestHandler).LoadImages = value;
            }
        }

        public class RequestHandler : IRequestHandler
        {
            public bool LoadImages;

            public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, ref string newUrl)
            {
                //throw new NotImplementedException();
            }

            public static readonly string VersionNumberString = String.Format("Chromium: {0}, CEF: {1}, CefSharp: {2}",
                Cef.ChromiumVersion, Cef.CefVersion, Cef.CefSharpVersion);

            bool IRequestHandler.OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
            {
                return false;
            }

            bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
            {
                return OnOpenUrlFromTab(browserControl, browser, frame, targetUrl, targetDisposition, userGesture);
            }

            protected virtual bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
            {
                return false;
            }

            bool IRequestHandler.OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
            {
                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        callback.Continue(true);
                        return true;
                    }
                }

                return false;
            }

            void IRequestHandler.OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
            {
                // TODO: Add your own code here for handling scenarios where a plugin crashed, for one reason or another.
            }

            CefReturnValue IRequestHandler.OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
            {
               if(!LoadImages && request.ResourceType == ResourceType.Image)
                    return CefReturnValue.Cancel;

                return CefReturnValue.Continue;
            }

            bool IRequestHandler.GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
            {
                callback.Dispose();
                return false;
            }

            void IRequestHandler.OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
            {
            }

            bool IRequestHandler.OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
            {
                //NOTE: If you do not wish to implement this method returning false is the default behaviour
                // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
                //callback.Dispose();
                //return false;

                //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        //Accept Request to raise Quota
                        //callback.Continue(true);
                        //return true;
                    }
                }

                return false;
            }

            bool IRequestHandler.OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
            {
                return url.StartsWith("mailto");
            }

            void IRequestHandler.OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
            {

            }

            bool IRequestHandler.OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                //NOTE: You cannot modify the response, only the request
                // You can now access the headers
                //var headers = response.ResponseHeaders;

                return false;
            }

            IResponseFilter IRequestHandler.GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
            {
                var url = new Uri(request.Url);
                return null;
            }

            void IRequestHandler.OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
            {

            }
        }
    }
}