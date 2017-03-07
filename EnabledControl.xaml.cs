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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for EnabledControl.xaml
    /// </summary>
    public partial class EnabledControl : UserControl
    {
        public EnabledControl()
        {
            InitializeComponent();
            
            output.Click += delegate
            {
                if (Controller.Running)
                {
                    DataFieldsWindow.This.Hide();
                    OutputWindow.This.Show();
                    OutputWindow.This.Activate();
                }
                else
                {
                    OutputWindow.This.Hide();
                    DataFieldsWindow.This.Show();
                    DataFieldsWindow.This.Activate();
                }
            };

            hide_browser.Click += delegate
            {
                MainWindow.This.ShowBrowser(hide_browser.IsChecked != true);
            };

            debug_mode.Click += delegate
            {
                Controller.DebugMode = debug_mode.IsChecked == true;
            };
            Controller.DebugMode = debug_mode.IsChecked == true;

            pause.Click += delegate
            {
                Controller.Pause = pause.IsChecked == true;
            };
        }
    }
}
