﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using ServiceStack;
using ServiceStack.Text;
using Squirrel;

namespace DefaultApp.AppWinForms
{
    static class Program
    {
        public static string HostUrl = "http://localhost:2337/";
        public static AppHost AppHost;
        public static FormMain Form;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppHost = new AppHost();
            SquirrelAwareApp.HandleEvents(
                OnInitialInstall,
                OnAppUpdate,
                onAppUninstall: OnAppUninstall,
                onFirstRun: OnFirstRun);

            Cef.EnableHighDPISupport();

            var cacheFolder = Path.Combine(Path.GetTempPath(), "cefsharp-cache");
            Cef.Initialize(new CefSettings
            {
                CachePath = cacheFolder
            });

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppHost.Init().Start("http://*:2337/");
            "ServiceStack SelfHost listening at {0} ".Fmt(HostUrl).Print();
            Form = new FormMain();
            Form.Disposed += (sender, args) => AppUpdater.Dispose();
            Application.Run(Form);
        }

        public static void OnInitialInstall(Version version)
        {
            // Hook for first install
            AppUpdater.CreateShortcutForThisExe();
        }

        public static void OnAppUpdate(Version version)
        {
            // Hook for application update, CheckForUpdates() initiates this.
            AppUpdater.CreateShortcutForThisExe();
        }

        public static void OnAppUninstall(Version version)
        {
            // Hook for application uninstall
            AppUpdater.RemoveShortcutForThisExe();
        }

        public static void OnFirstRun()
        {
            // Hook for first run
        }
    }
}
