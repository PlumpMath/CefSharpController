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
    /// Interaction logic for DataWindow.xaml
    /// </summary>
    public partial class DataFieldsWindow : Window
    {
        public DataFieldsWindow(RouteControl route_control)
        {
            InitializeComponent();

            route_control.OutputFieldAdded += delegate(string name, string value)
            {
                fields.Items.Add(new Field { Name = name, Value = value });
            };

            fields.MouseRightButtonUp += delegate (object sender, MouseButtonEventArgs e)
            {
                DataGridCell cell = null;
                DataGridRow row = null;
                for (DependencyObject d = (DependencyObject)e.OriginalSource; d != null; d = VisualTreeHelper.GetParent(d))
                {
                    if (cell == null)
                        cell = d as DataGridCell;
                    if (row == null)
                        row = d as DataGridRow;
                    if (cell != null && row != null)
                        break;
                }
                if (cell == null || row == null)
                    return;

                if (!Message.YesNo("Remove this field?"))
                    return;

                //Items.Remove((Field)row.Item);
                //fields.ItemsSource = items;
                fields.Items.Remove((Field)row.Item);
                //fields.Items.Refresh();
                route_control.RemoveOutputField(((Field)row.Item).Name);
            };
        }

        //readonly List<Field> items = new List<Field>();

        //public List<Field> Items
        //{
        //    get
        //    {
        //        return items;
        //    }
        //}

        public class Field
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public void Add(Field field)
        {
            fields.Items.Add(field);
        }
    }
}
