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
                set();
            };

            Attributes.SelectionChanged += delegate {
                Attributes.UnselectAll();
            };

            Xpath.Text = xpath;
        }

        readonly List<Item> items = new List<Item>();

        void set()
        {
            items.Clear();

            if (string.IsNullOrWhiteSpace(Xpath.Text))
                return;
            Dictionary<string, object> ans2av = get_attributes(Xpath.Text);

            foreach (string a in ans2av.Keys)
                items.Add(new Item() { Get = false, Attribute = a, Value = (string)ans2av[a] });

            Attributes.ItemsSource = items;
        }

        public class Item
        {
            public bool Get { get; set; }
            public string Attribute { get; set; }
            public string Value { get; set; }
        }

        Dictionary<string, object> get_attributes(string xpath)
        {
            var ans2av = (Dictionary<string, object>)MainWindow.Execute(
                WebDocumentRoutines.Define_getElementsByXPath()
                + @"
            var es =  document.__getElementsByXPath('" + xpath + @"');
if(es.length > 1)
    alert('Found more than 1 element!');
else if(es.length < 1)
    alert('Found no element!');

var ans2av = {};
var as = es[0].attributes;
for (var i = 0; i < as.length; i++) {
    ans2av[as[i].name] = as[i].value;
}
ans2av['_innerHtml'] = es[0].innerHTML;
return ans2av;
            ");
            return ans2av;
        }
    }
}
