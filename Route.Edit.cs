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
        public void RemoveOutputField(string field_name)
        {
            foreach (XmlNode xfn in xd.SelectNodes("Route/Queue/Output/Field[@name='" + field_name + "']"))
                xd.RemoveChild(xfn);
        }
    }
}