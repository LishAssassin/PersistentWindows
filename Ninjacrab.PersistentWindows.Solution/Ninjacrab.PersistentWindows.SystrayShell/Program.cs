﻿using System;
using System.Threading;
using System.Windows.Forms;

using Ninjacrab.PersistentWindows.Common;
using Ninjacrab.PersistentWindows.Common.Diagnostics;

namespace Ninjacrab.PersistentWindows.SystrayShell
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static readonly string ProjectUrl = "https://github.com/kangyu-california/PersistentWindows";

        static PersistentWindowProcessor pwp = null;    
        static SystrayForm systrayForm = null;

        [STAThread]
        static void Main(string[] args)
        {
            bool no_splash = false;
            bool dry_run = false;
            foreach (var arg in args)
            {
                switch(arg)
                {
                    case "-silent":
                        no_splash = true;
                        break;

                    case "-dry_run":
                        Log.Trace("dry_run mode");
                        dry_run = true;
                        break;
                }
            }

            /*
                        Mutex singleInstMutex = new Mutex(true, Application.ProductName);
                        if (!singleInstMutex.WaitOne(TimeSpan.Zero, true))
                        {
                            MessageBox.Show($"Only one inst of {Application.ProductName} can be run!");
                            //Application.Exit();
                            return;
                        }
                        else
                        {
                            singleInstMutex.ReleaseMutex();
                        }
            */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            systrayForm = new SystrayForm();

            pwp = new PersistentWindowProcessor();
            pwp.dryRun = dry_run;
            pwp.showRestoreTip = ShowRestoreTip;
            pwp.hideRestoreTip = HideRestoreTip;
            pwp.enableRestoreMenu = EnableRestoreMenu;

            if (!pwp.Start())
            {
                return;
            }

            if (!no_splash)
            {
                StartSplashForm();
            }

            Application.Run();
        }

        static void ShowRestoreTip()
        {
            var thread = new Thread(() =>
            {
                systrayForm.notifyIconMain.Visible = true;
                systrayForm.notifyIconMain.ShowBalloonTip(30000);
            });

            thread.IsBackground = false;
            thread.Start();
        }

        static void HideRestoreTip()
        {
            systrayForm.notifyIconMain.Visible = false;
            systrayForm.notifyIconMain.Visible = true;
        }

        static void EnableRestoreMenu(bool enable)
        {
            systrayForm.restoreToolStripMenuItem.Enabled = enable;
        }

        static void StartSplashForm()
        {
            var thread = new Thread(() =>
            {
                Application.Run(new SplashForm());
            });
            thread.IsBackground = false;
            thread.Priority = ThreadPriority.Highest;
            thread.Name = "StartSplashForm";
            thread.Start();
        }

        static public void Capture()
        {
            pwp.BatchCaptureApplicationsOnCurrentDisplays(saveToDB : true);
        }

        static public void Restore()
        {
            pwp.restoreFromDB = true;
            pwp.BatchRestoreApplicationsOnCurrentDisplays();
        }

    }
}
