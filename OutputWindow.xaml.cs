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
using System.Dynamic;

namespace Cliver.CefSharpController
{
    public partial class OutputWindow : Window
    {
        public readonly static OutputWindow This = new OutputWindow();

        OutputWindow()
        {
            InitializeComponent();

            this.Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                e.Cancel = true;
                this.Hide();
            };

            Controller.WriteLine += Controller_WriteLine;
        }

        private void Controller_WriteLine(List<string> headers, List<string> values)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (output.Columns.Count == 0)
                {
                    output.Columns.Clear();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        DataGridTextColumn c = new DataGridTextColumn();
                        Style s = new Style(typeof(TextBlock));
                        s.Setters.Add(
                            new Setter(
                                TextBlock.TextWrappingProperty,
                                TextWrapping.Wrap
                                ));
                        c.ElementStyle = s;
                        c.Header = headers[i];
                        c.IsReadOnly = true;
                        c.Binding = new Binding(i.ToString());
                        output.Columns.Add(c);
                    }
                }
                if (Settings.Default.OutputWindowStackSize <= 0)
                    return;
                if (output.Items.Count >= Settings.Default.OutputWindowStackSize)
                    output.Items.RemoveAt(0);
                dynamic row = new ExpandoObject();
                for (int i = 0; i < values.Count; i++)
                    ((IDictionary<String, Object>)row)[i.ToString()] = values[i];
                output.Items.Add(row);
            }));
        }

        public void Clear()
        {
            output.Columns.Clear();
        }

        //public void OutputFieldAdded(string queue_name, string field_name, string field_value)
        //{
        //    Field f = null;
        //    foreach(Field x in fields.Items)
        //        if(x.Queue == queue_name && x.Name==field_name)
        //        {
        //            f = x;
        //            break;
        //        }
        //    if (f != null)
        //    {
        //        f.Value += field_name;
        //        fields.Items.Refresh();
        //    }
        //    else
        //        fields.Items.Add(new Field { Queue = queue_name, Name = field_name, Value = field_value });
        //}
    }
}
