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

namespace Cliver.Outscrape
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
    }
}
