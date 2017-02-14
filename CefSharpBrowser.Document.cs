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
    public partial class CefSharpBrowser
    {
        public object ExecuteJavaScript(string script)
        {
            var t = browser.EvaluateScriptAsync(
@"(function(){
            try{
            " + script + @"
            }catch(err){
                alert(err.message);
            }
        }())");
            while (!t.IsCompleted)
                DoEvents();
            return t.Result.Result;
        }

        public void HighlightElements(string xpath)
        {
            ExecuteJavaScript(@"
                if(!document.__highlightedElements){
                    var style = document.createElement('style');
                    style.type = 'text/css';
                    style.innerHTML = '.__highlight { background-color: #F00 !important; }';
                    document.getElementsByTagName('head')[0].appendChild(style);
                }else{
                    for(var i = 0; i < document.__highlightedElements.length; i++)
                        document.__highlightedElements[i].className = document.__highlightedElements[i].className.replace(/\b__highlight\b/,''); 
                }

                document.__highlightedElements = [];
                var es = document.__getElementsByXPath('" + xpath + @"');
                for(var i = 0; i < es.length; i++){
                    es[i].className += ' __highlight';
                    document.__highlightedElements.push(es[i]);
                }
            ");
        }
    }
}
