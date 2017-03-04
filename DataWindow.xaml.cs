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
    public partial class DataWindow : Window
    {
        public DataWindow(Route route)
        {
            InitializeComponent();
                        
            List<string> hs = new List<string>();
            foreach (Route.Queue q in route.GetQueues())
                hs.InsertRange(0, q.Outputs.Where(o => o is Route.Output.Field).Select(f => ((Route.Output.Field)f).Name));

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

                Items.Remove((Item)row.Item);
                fields.ItemsSource = items;
                //fields.Items.Refresh();
            };
        }

        readonly List<Item> items = new List<Item>();

        public List<Item> Items
        {
            get
            {
                return items;
            }
        }

        public class Item
        {
            public string Field { get; set; }
            public string Value { get; set; }
        }
    }
}
