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

            Browser = new CefSharpBrowser(browser, "JsMapObject", route);

            browser.LoadingStateChanged += delegate (object sender, LoadingStateChangedEventArgs e)
            {
                route.Dispatcher.BeginInvoke((Action)(() => { route.IsEnabled = !e.IsLoading; }));

                url.Dispatcher.BeginInvoke((Action)(() => { url.Text = browser.Address; }));
            };

            Closing += delegate
              {
                  browser.Stop();
              };

            back.Click += delegate
            {
                browser.Back();
            };

            forward.Click += delegate
            {
                browser.Forward();
            };

            stop.Click += delegate
            {
                browser.Stop();
            };

            reload.Click += delegate
            {
                browser.Reload();
            };

            url.KeyUp += (object sender, KeyEventArgs e) =>
            {
                if (e.Key == Key.Enter)
                    browser.Load(url.Text);
            };

            load_images.Click += set_load_images;
            load_images.IsChecked = true;
            set_load_images(null, null);
        }

        void set_load_images(object sender, RoutedEventArgs e)
        {
            Browser.LoadImages = load_images.IsChecked == true;
        }

        public static MainWindow This { get; private set; }

        readonly public CefSharpBrowser Browser;


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


        //static public string State
        //{
        //    set { This.state.Content = value; }
        //}
    }
}