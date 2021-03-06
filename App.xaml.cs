﻿//********************************************************************************************
//Developed: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//********************************************************************************************
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;

namespace Cliver.CefSharpController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Log.Initialize(Log.Mode.SESSIONS, null, false);
            InternetDateTime.CHECK_TEST_PERIOD_VALIDITY(2017, 3, 10, true);
            ////Perform dependency check to make sure all relevant resources are in our output directory.
            //var settings = new CefSettings();
            ////settings.EnableInternalPdfViewerOffScreen();
            //// Disable GPU in WPF and Offscreen examples until #1634 has been resolved
            //settings.CefCommandLineArgs.Add("disable-gpu", "1");
            //settings.CachePath = "cache";

            //Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
        }
    }
}
