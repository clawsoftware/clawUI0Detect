using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace clawUI0Detect
{
    internal partial class Service : ServiceBase
    {
        private static readonly string Outfile = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\clawSoft\clawUI0Detect.txt";

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\clawSoft\");
            }
            catch (Exception e)
            {
            }

            UI0Detect();
        }

        protected override void OnStop()
        {
            // TODO: Hier Code zum Ausführen erforderlicher Löschvorgänge zum Anhalten des Dienstes hinzufügen.
        }

        internal void UserInteractive(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("{0,-30}{1,20}", "Copyright © 2019 Andrew Hess // clawSoft", "");
            Console.WriteLine("{0,-30}{1,20}", "Released under the terms of the GNU General Public License", "");
            Console.WriteLine("{0,-30}{1,20}", "See LICENSE for details", "");
            Process.Start("sc", "stop clawUI0Detect")?.WaitForExit();
            Process.Start("sc", "delete clawUI0Detect")?.WaitForExit();
            try
            {
            ManagedInstallerClass.InstallHelper(new[]
                {"/LogToConsole=false /i", Assembly.GetExecutingAssembly().Location});
            }
            catch (Exception e)
            {
            }

            while (!File.Exists(Outfile)) Thread.Sleep(100);
            var output = File.ReadAllText(Outfile);
            Console.Write(output);
            Process.Start("sc", "stop clawUI0Detect")?.WaitForExit();
            Process.Start("sc", "delete clawUI0Detect")?.WaitForExit();
            File.Delete(Outfile);
        }

        private static void UI0Detect()
        {
            var detected = new List<detectedprocesses>();

            // well known processes
            var services = Process.GetProcessesByName("services");
            var wininit = Process.GetProcessesByName("wininit");
            var wlanext = Process.GetProcessesByName("wlanext");
            var svchost = -1;
            int ppid;

            foreach (var procesInfo in Process.GetProcesses())
            {
                try
                {
                    ppid = Win32Api.ParentProcessUtilities.GetParentProcess(procesInfo.Handle).Id;
                }
                catch
                {
                    ppid = -1;
                }

                if (procesInfo.SessionId == 0 && services[0].Id != ppid && ppid != -1 && wininit[0].Id != ppid &&
                    svchost != ppid && procesInfo.Id != Process.GetCurrentProcess().Id &&
                    procesInfo.Id != wlanext[0].Id && procesInfo.ProcessName != "clawUI0Detect")
                {
                    var window = 0;
                    foreach (ProcessThread threadInfo in procesInfo.Threads)
                    {
                        var windows = Win32Api.GetWindowHandlesForThread(threadInfo.Id);
                        if (windows != null && windows.Length > 0)
                            foreach (var hWnd in windows)
                                if (Win32Api.GetPlacement(hWnd).showCmd != Win32Api.ShowWindowCommands.Minimized)
                                {
                                    window++;
                                    try
                                    {
                                        detected.Add(new detectedprocesses
                                        {
                                            pid = procesInfo.Id.ToString(), process = procesInfo.ProcessName,
                                            title = Win32Api.GetWindowTitel(hWnd), window = window.ToString()
                                        });
                                    }
                                    catch (Exception e)
                                    {
                                    }
                                }
                    }
                }
            }

            if (detected.Count == 0)
            {
                File.AppendAllText(Outfile, Environment.NewLine);
                File.AppendAllText(Outfile, "!!! No UI in Session 0 detected !!!" + Environment.NewLine);
                File.AppendAllText(Outfile, Environment.NewLine);
            }
            else
            {
                foreach (var row in detected)
                {
                    File.AppendAllText(Outfile, Environment.NewLine);
                    File.AppendAllText(Outfile, "[window " + row.window + "]" + Environment.NewLine);
                    File.AppendAllText(Outfile, "pid:\t\t " + row.pid + Environment.NewLine);
                    File.AppendAllText(Outfile, "process:\t " + row.process + Environment.NewLine);
                    File.AppendAllText(Outfile, "titel:\t\t " + row.title + Environment.NewLine);
                    File.AppendAllText(Outfile, Environment.NewLine);
                }
            }
        }

        private class detectedprocesses
        {
            public string window { get; set; }
            public string pid { get; set; }
            public string process { get; set; }
            public string title { get; set; }
        }
    }
}