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

        public static string Define_createXPathForElement()
        {
            return @"
if(!document.__createXPathForElement){
            document.__createXPathForElement = function(element) {
                var xpath = '';
                for (; element && element.nodeType == 1; element = element.parentNode) {
                    //alert(element);
                    var cs = element.parentNode.children;
                    var j = 0;
                    var k = 0;
                    for(var i = 0; i < cs.length; i++){
                        if (cs[i].tagName == element.tagName){
                            j++;
                            if(cs[i] == element){
                                k = j;
                                //break;
                            }
                        } 
                    }
                    var id = '';
                    if(j > 1)
                        id = '[' + k + ']';
                    xpath = '/' + element.tagName.toLowerCase() + id + xpath;
                }
                return xpath;
            };
};
            ";
        }

//        static public  void Click(string xpath)
//        {
//            CefSharpRoutines.Execute(MainWindow.Browser, @"
//                    document.__getElementsByXPath = function(path) {
//                        var evaluator = new XPathEvaluator();
//                        var result = evaluator.evaluate(path, document.documentElement, null, XPathResult.UNORDERED_NODE_ITERATOR_TYPE, null);
//                        var es = [];
//                        for(var thisNode = result.iterateNext(); thisNode; thisNode = result.iterateNext()){
//                            es.push(thisNode);
//                        }
//                        return es;
//                    };

//            var es =  document.__getElementsByXPath('" + xpath + @"');
//if(es.length < 1)
//    alert('no element found:' + '" + xpath + @"');
//else
//    es[0].click();
//            ");
//        }
    }
}
