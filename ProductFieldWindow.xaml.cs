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
    /// Interaction logic for ProductFieldWindow.xaml
    /// </summary>
    public partial class ProductFieldWindow : Window
    {
        public ProductFieldWindow(string xpath)
        {
            InitializeComponent();

            Ok.Click += delegate {
                if (string.IsNullOrWhiteSpace(Name.Text))
                {
                    Message.Exclaim("Name is not set!");
                    return;
                }
                DialogResult = true;
                Close();
            };

            Xpath.TextChanged += delegate {
                set_by_xpath();
            };

            Attributes.SelectionChanged += (o, e) =>
            {
                //((Item)Attributes.Items[Attributes.SelectedIndex]).Get = !((Item)Attributes.Items[Attributes.SelectedIndex]).Get;
                //this.Dispatcher.
                //e.Handled = true;
                //Attributes.UnselectAll();
            };

            Attributes.MouseLeftButtonUp += Attributes_MouseLeftButtonUp;

            Xpath.Text = xpath;
        }

        private void Attributes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null)
                return;
            ((Item)row.Item).Get = !((Item)row.Item).Get;
            Attributes.Items.Refresh();
        }

        readonly List<Item> items = new List<Item>();

        void set_by_xpath()
        {
            items.Clear();

            if (string.IsNullOrWhiteSpace(Xpath.Text))
                return;
            Dictionary<string, object> ans2av = get_attributes(Xpath.Text);
            if (ans2av == null)
                return;

            foreach (string a in ans2av.Keys)
                items.Add(new Item() { Get = (a == "INNER_HTML" ? true : false), Attribute = a, Value = (string)ans2av[a] });

            Attributes.ItemsSource = items;

            object name = null;
            if (ans2av.TryGetValue("id", out name) || ans2av.TryGetValue("class", out name))
                Name.Text = (string)name;
        }

        public class Item
        {
            public bool Get { get; set; }
            public string Attribute { get; set; }
            public string Value { get; set; }
        }

        Dictionary<string, object> get_attributes(string xpath)
        {
            var ans2av = (Dictionary<string, object>)MainWindow.This.Browser.ExecuteJavaScript(
                CefSharpBrowser.Define__getElementsByXPath()
                + @"
            var es =  document.__getElementsByXPath('" + xpath + @"');
if(es.length > 1)
    alert('Found more than 1 element!');
else if(es.length < 1)
    alert('Found no element!');

var ans2av = {};
var as = es[0].attributes;
for (var i = 0; i < as.length; i++) {
    var c = as[i].value;
    if(as[i].name == 'class'){
        c = c.replace(/\b__highlight\b/,'').trim();
        if(!c)
            continue;
    }
    ans2av[as[i].name] = c;
}
ans2av['INNER_HTML'] = es[0].innerHTML;
return ans2av;
            ");
            return ans2av;
        }
    }
}
