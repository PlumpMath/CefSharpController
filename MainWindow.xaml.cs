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

            Dispatcher.ShutdownStarted += delegate (object sender, EventArgs e)
            {
            };

            Closing += delegate
              {
                  browser.Stop();
                  browser.Dispose();
                  Application.Current.Shutdown();
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

        public void ShowBrowser(bool show)
        {
            Visibility v = show ? Visibility.Visible : Visibility.Collapsed;
            browser.Visibility = v;
            browser_controls.Visibility = v;
        }

        public static MainWindow This { get; private set; }

        readonly public CefSharpBrowser Browser;
    }
}