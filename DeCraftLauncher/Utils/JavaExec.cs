using DeCraftLauncher.Configs;
using DeCraftLauncher.UIControls.Popup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DeCraftLauncher.Utils
{
    public class JavaExec
    {
        public List<string> jvmArgs = new List<string>();
        public List<string> classPath = new List<string>();
        public string className;
        public List<string> programArgs = new List<string>();

        public string workingDirectory = null;
        public string appdataDir = "";

        public string execName = "java";

        public JavaExec(string className) 
        { 
            this.className = className;
        }

        public string GetFullArgsString()
        {
            string classPaths = classPath.Count != 0 ? $"-cp \"{String.Join(";", classPath)}\"" : "";

            // do not use Append here or it will be incompatible with .net 4.5.2
            return String.Join(" ", new List<string>()
                .Concat(new string[] { classPaths })
                .Concat(jvmArgs)
                .Concat(new string[] { className })
                .Concat(programArgs)
            );
        }

        public Process Start(string javaPath = null, Action<List<string>> callback = null)
        {
            return StartWithCustomArgsString(GetFullArgsString(), javaPath, callback);
        }

        public Process StartWithCustomArgsString(string argsString, string javaPath = null, Action<List<string>> callback = null)
        {
            javaPath = javaPath ?? (MainWindow.mainRTConfig.javaHome + execName);

            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }

            try
            {
                return JarUtils.RunProcess(javaPath, argsString, appdataDir, callback);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (workingDirectory != null)
                {
                    Directory.SetCurrentDirectory(MainWindow.currentDirectory);
                }
            }
        }

        public void StartOpenWindowAndAddToInstances(MainWindow caller, JarConfig jarConfig, bool setWindowSize = true)
        {
            Process newProcess = Start();
            WindowProcessLog processLog = new WindowProcessLog(newProcess, caller, jarConfig.isServer);
            processLog.Show();
            caller.AddRunningInstance(new UIControls.InstanceListElement.RunningInstanceData(jarConfig.friendlyName != "" ? jarConfig.friendlyName : jarConfig.jarFileName, processLog, jarConfig.playerName));
            new Thread((process) =>
            {
                int timeoutMS = 50000;
                int timeChecks = 500;
                try
                {
                    for (int x = 0; x < timeoutMS; x += timeChecks)
                    {
                        if (((Process)process).MainWindowHandle != IntPtr.Zero)
                        {
                            Util.SetWindowDarkMode(newProcess.MainWindowHandle);
                            if (setWindowSize)
                            {
                                Util.SetWindowSize(newProcess.MainWindowHandle, jarConfig);
                            }
                            break;
                        }
                        Thread.Sleep(timeChecks);
                    }
                } catch (Exception e)
                {
#if DEBUG
                    caller.Dispatcher.Invoke(delegate
                    {
                        PopupOK.ShowNewPopup($"windowupdate thread error: {e.Message}");
                    });
#endif
                }
                Console.WriteLine("Dark mode set thread exited");
            }).Start(newProcess);
        }
    }
}
