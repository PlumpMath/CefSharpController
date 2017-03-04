//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//********************************************************************************************

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Cliver.CefSharpController
{
    public partial class Route
    {
        public void RemoveOutputField(string queue_name, string field_name)
        {
            foreach (XmlNode xfn in xd.SelectNodes("Route/Queue[@name='" + queue_name + "']/Outputs/Field[@name='" + field_name + "']"))
                xfn.ParentNode.RemoveChild(xfn);
            Changed?.Invoke(this);
        }

        //public delegate void OnOutputFieldAdded(string name, string value);
        //public event OnOutputFieldAdded OutputFieldAdded;

        public delegate void OnChanged(Route route);
        public event OnChanged Changed;
    }
}