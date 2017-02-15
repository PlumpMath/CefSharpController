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
    public partial class SaveWindow : Window
    {
        public SaveWindow()
        {
            InitializeComponent();

            XmlName.Text = "test.xml";

            ok.Click += delegate
              {
                  DialogResult = true;
                  Close();
              };
        }
    }
}
