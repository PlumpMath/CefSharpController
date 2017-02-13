using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using System.Windows;
using System.Windows.Threading;

namespace Cliver.CefSharpController
{
    static public class WebDocumentRoutines
    {
        public static string Define_getElementsByXPath()
        {
            return @"
if(!document.__getElementsByXPath){
            document.__getElementsByXPath = function(path) {
                var evaluator = new XPathEvaluator();
                var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                var es = [];
                for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                    es.push(thisNode);
                }
                return es;
            };
};
            ";
        }

      static public  void Click(string xpath)
        {
            MainWindow.Execute(@"
                    document.__getElementsByXPath = function(path) {
                        var evaluator = new XPathEvaluator();
                        var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
                        var es = [];
                        for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
                            es.push(thisNode);
                        }
                        return es;
                    };

            var es =  document.__getElementsByXPath('" + xpath + @"');
if(es.length < 1)
    alert('no element found:' + '" + xpath + @"');
else
    es[0].click();
            ");
        }

        //        static public object Execute(this ChromiumWebBrowser browser, string script)
        //        {
        //            var t = browser.EvaluateScriptAsync(
        //@"(function(){
        //    try{
        //    " + script + @"
        //    }catch(err){
        //        alert(err.message);
        //    }
        //}())");
        //            while (!t.IsCompleted)
        //                DoEvents();
        //            return t.Result.Result;
        //        }

        //        public static void DoEvents()
        //        {
        //            if (Application.Current == null)
        //                return;
        //            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        //        }
    }
}
