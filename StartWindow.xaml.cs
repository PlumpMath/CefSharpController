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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for StartUrlWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();

            ok.Click += delegate
              {
                  DialogResult = true;
                  Close();
              };

            RouteType.SelectionChanged += delegate
            {
                if (!string.IsNullOrWhiteSpace(StartUrl.Text))
                    return;
                switch (RouteType.SelectedIndex)
                {
                    case 0:
                        StartUrl.Text = "http://boston.craigslist.org/search/ata";
                        break;
                    case 1:
                        StartUrl.Text = "https://www.google.com/search?q=js+get+all+elements";
                        break;
                    case 2:
                        StartUrl.Text = "https://translate.pentairpool.com/en/products";
                        //StartUrl.Text = "http://boston.craigslist.org/search/ata";
                        StartUrl.Text = "https://www.amazon.com/ap/signin?_encoding=UTF8&openid.assoc_handle=usflex&openid.claimed_id=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.identity=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0%2Fidentifier_select&openid.mode=checkid_setup&openid.ns=http%3A%2F%2Fspecs.openid.net%2Fauth%2F2.0&openid.ns.pape=http%3A%2F%2Fspecs.openid.net%2Fextensions%2Fpape%2F1.0&openid.pape.max_auth_age=0&openid.return_to=https%3A%2F%2Fwww.amazon.com%2Fb%2Fref%3Dnav_signin%3F_encoding%3DUTF8%26node%3D5782443011%26pf_rd_m%3DATVPDKIKX0DER%26pf_rd_s%3Dmerchandised-search-4%26pf_rd_r%3DH32HQDT2T42FCMTBY75X%26pf_rd_t%3D101%26pf_rd_p%3D5cdf0554-3673-50da-a889-915a59a075a0%26pf_rd_i%3D16318651";
                        StartUrl.Text = "https://www.chainreactioncycles.com/signin?targetpage=/myaccount";
                        break;
                }
            };

            input_file.Click += delegate
            {
                Microsoft.Win32.OpenFileDialog d = new Microsoft.Win32.OpenFileDialog();
                d.DefaultExt = ".csv";
                d.Filter = "CSV Files (*.csv)|*.csv|TXT Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (d.ShowDialog() != true)
                    return;

                string[] us = System.IO.File.ReadAllLines(d.FileName);
                StartUrl.Text = string.Join("\r\n", us);
            };
        }
    }
}