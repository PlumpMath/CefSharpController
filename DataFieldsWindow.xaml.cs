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

            this.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
              {
                  e.Cancel = true;
                  this.Hide();
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

                row.IsSelected = true;

                if (!Message.YesNo("Remove this field?"))
                    return;

                //Items.Remove((Field)row.Item);
                //fields.ItemsSource = items;
                Field f = (Field)row.Item;
                fields.Items.Remove(f);
                //fields.Items.Refresh();
                route?.RemoveOutputField(f.Queue, f.Name);
            };
        }
        Route route = null;

        public class Field
        {
            public string Queue { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public void OutputFieldAdded(string queue_name, string field_name, string field_value)
        {
            fields.Items.Add(new Field { Queue = queue_name, Name = field_name, Value = field_value });
        }

        public void SetRoute(Route route)
        {
            this.route = route;

            route.Changed += delegate (Route r)
            {//check if deleted
                List<Route.Queue> qs = r.GetQueues();
                for (int i = fields.Items.Count - 1; i >= 0; i--)
                {
                    Field f = (Field)fields.Items[i];
                    Route.Queue q = qs.Where(x => x.Name == f.Queue).FirstOrDefault();
                    if (q != null)
                        if (null != q.Outputs.Where(o => o is Route.Output.Field && ((Route.Output.Field)o).Name == f.Name).FirstOrDefault())
                            continue;
                    fields.Items.RemoveAt(i);
                }
            };
        }
    }
}