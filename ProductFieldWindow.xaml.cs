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

            //strip_html.IsChecked = Settings.Default.StripHtml;

            //strip_html.Checked += delegate
            //  {
            //      Settings.Default.StripHtml = strip_html.IsChecked == true;
            //      Settings.Default.Save();
            //      set_by_xpath();
            //  };

            Ok.Click += delegate 
            {
                if (string.IsNullOrWhiteSpace(Name.Text))
                {
                    Message.Exclaim("Name is not set!");
                    return;
                }
                DialogResult = true;
                Close();
            };

            Xpath.TextChanged += delegate 
            {
                set_by_xpath();
            };

            //attributes.SelectionChanged += (o, e) =>
            //{
            //    ((Item)Attributes.Items[Attributes.SelectedIndex]).Get = !((Item)Attributes.Items[Attributes.SelectedIndex]).Get;
            //    this.Dispatcher.
            //    e.Handled = true;
            //    Attributes.UnselectAll();
            //};

            attributes.MouseLeftButtonUp += delegate (object sender, MouseButtonEventArgs e)
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
                if (cell.Column.DisplayIndex == 0)
                    ((Item)row.Item).Get = !((Item)row.Item).Get;
                //if (cell.Column.DisplayIndex == 3)
                //{
                //    Item i = ((Item)row.Item);
                //    i.StripHtml = !i.StripHtml;
                //    i.Value = i.StripHtml ? FieldPreparation.Html.Normalize(i.RawValue) : i.RawValue;
                //}

                attributes.Items.Refresh();
            };

            Xpath.Text = xpath;

            //attributes.CellEditEnding += delegate (object sender, DataGridCellEditEndingEventArgs e)
            //{
            //    if (e.EditAction == DataGridEditAction.Commit)
            //    {
            //        var column = e.Column as DataGridBoundColumn;
            //        if (column != null)
            //        {
            //            var bindingPath = (column.Binding as Binding).Path.Path;
            //            if (bindingPath == "Col2")
            //            {
            //                int rowIndex = e.Row.GetIndex();
            //                var el = e.EditingElement as TextBox;
            //                // rowIndex has the row index
            //                // bindingPath has the column's binding
            //                // el.Text has the new, user-entered value
            //            }
            //        }
            //    }
            //};
        }
        
        readonly List<Item> items = new List<Item>();

      public  List<Item> Items
        {
            get
            {
                return items;
            }
        }

        void set_by_xpath()
        {
            //Dictionary<string, bool> attributes2strip_html = new Dictionary<string, bool>();
            //foreach (Item i in attributes.Items)
            //    attributes2strip_html[i.Attribute] = i.StripHtml;

            items.Clear();

            if (string.IsNullOrWhiteSpace(Xpath.Text))
                return;
            Dictionary<string, object> ans2av = get_attributes(Xpath.Text);
            if (ans2av == null)
                return;

            foreach (string a in ans2av.Keys)
            {
                //Item i = new Item() { Get = (a == Route.OutputField.INNER_HTML ? true : false), Attribute = a, RawValue = (string)ans2av[a] };
                //bool sh;
                //i.StripHtml = attributes2strip_html.TryGetValue(a, out sh) ? sh : true;
                //i.Value = i.StripHtml ? FieldPreparation.Html.Normalize(i.RawValue) : i.RawValue;
                //items.Add(i);
                items.Add(new Item() { Get = false, Attribute = a, Value = (string)ans2av[a] });
            }
            items.Insert(0, new Item() { Get = true, Attribute = Route.OutputField.INNER_TEXT, Value = FieldPreparation.Html.Normalize((string)ans2av[Route.OutputField.INNER_HTML]) });

            attributes.ItemsSource = items;

            object name = null;
            if (ans2av.TryGetValue("id", out name) || ans2av.TryGetValue("class", out name))
                Name.Text = (string)name;
        }

        public class Item
        {
            public bool Get { get; set; }
            public string Attribute { get; set; }
            public string Value { get; set; }
            //public string RawValue { get; set; }
            //public bool StripHtml { get; set; }
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
ans2av['" + Route.OutputField.INNER_HTML + @"'] = es[0].innerHTML;
return ans2av;
            ");
            return ans2av;
        }
    }
}
