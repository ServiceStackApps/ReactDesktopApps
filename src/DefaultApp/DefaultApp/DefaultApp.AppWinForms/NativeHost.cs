﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms.Internals;
using ServiceStack.Configuration;
using Squirrel;

namespace DefaultApp.AppWinForms
{
    public class NativeHost
    {
        private readonly FormMain formMain;

        public NativeHost(FormMain formMain)
        {
            this.formMain = formMain;
            //Enable Chrome Dev Tools when debugging WinForms
#if DEBUG
            formMain.ChromiumBrowser.KeyboardHandler = new KeyboardHandler();
#endif
        }

        public string Platform
        {
            get { return "winforms"; }
        }

        public void Quit()
        {
            formMain.InvokeOnUiThreadIfRequired(() =>
            {
                formMain.Close();
            });
        }

        public void ShowAbout()
        {
            MessageBox.Show(@"ServiceStack Winforms with CefSharp + React", @"DefaultApp.AppWinForms", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ToggleFormBorder()
        {
            formMain.InvokeOnUiThreadIfRequired(() =>
            {
                formMain.FormBorderStyle = formMain.FormBorderStyle == FormBorderStyle.None
                    ? FormBorderStyle.Sizable
                    : FormBorderStyle.None;
            });
        }

        public void Ready()
        {
            //Invoke on DOM ready
            var appSettings = new AppSettings();
            var checkForUpdates = appSettings.Get<bool>("EnableAutoUpdate");
            if (!checkForUpdates)
                return;

            var releaseFolderUrl = appSettings.GetString("UpdateManagerUrl");
            try
            {
                var updatesAvailableTask = AppUpdater.CheckForUpdates(releaseFolderUrl);
                updatesAvailableTask.ContinueWith(isAvailable =>
                {
                    isAvailable.Wait(TimeSpan.FromMinutes(1));
                    bool updatesAvailable = isAvailable.Result;
                    //Only check once one launch then release UpdateManager.
                    if (!updatesAvailable)
                    {
                        AppUpdater.Dispose();
                        return;
                    }
                    if (formMain == null)
                    {
                        return;
                    }
                    // Notify web client updates are available.
                    formMain.InvokeOnUiThreadIfRequired(() =>
                    {
                        formMain.ChromiumBrowser.GetMainFrame().ExecuteJavaScriptAsync("window.updateAvailable();");
                    });
                });
            }
            catch (Exception)
            {
                // Error reaching update server
            }
        }

        public void PerformUpdate()
        {
            AppUpdater.ApplyUpdates(new AppSettings().GetString("UpdateManagerUrl")).ContinueWith(t =>
            {
                formMain.InvokeOnUiThreadIfRequired(() =>
                {
                    formMain.Close();
                    Cef.Shutdown();
                    UpdateManager.RestartApp();
                });
            });
        }
    }

#if DEBUG
    public class KeyboardHandler : CefSharp.IKeyboardHandler
    {
        public bool OnPreKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode,
            CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            if (windowsKeyCode == (int)Keys.F12)
            {
                Program.Form.ChromiumBrowser.ShowDevTools();
            }
            return false;
        }

        public bool OnKeyEvent(IWebBrowser browserControl, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode,
            CefEventFlags modifiers, bool isSystemKey)
        {
            return false;
        }
    }
#endif
}
