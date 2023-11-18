using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
using DeCraftLauncher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy LaunchEntryPoint.xaml
    /// </summary>
    public partial class UnimportantLaunchEntryPoint : UserControl
    {
        JarUtils.EntryPoint entryPoint;
        JarConfig jarConfig;
        MainWindow caller;

        public UnimportantLaunchEntryPoint(JarUtils.EntryPoint target, MainWindow caller, JarConfig jarConfig)
        {
            InitializeComponent();
            this.caller = caller;
            entryPoint = target;
            classname.Content = entryPoint.classpath;
            this.jarConfig = jarConfig;
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            //todo: make this code into a function shared between LaunchEntryPoint and UnimportantLaunchEntryPoint
            caller.SaveCurrentJarConfig();
            if (jarConfig.LWJGLVersion == "+ built-in")
            {
                MainWindow.EnsureDir($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin");
                MainWindow.EnsureDir($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native");
                ZipArchive zip = ZipFile.OpenRead(Path.GetFullPath(MainWindow.jarDir + "/" + jarConfig.jarFileName));
                var dllFilesToExtract = (from x in zip.Entries where x.FullName.StartsWith($"{jarConfig.jarBuiltInLWJGLDLLs}") && x.Name.EndsWith(".dll") select x);
                DirectoryInfo nativesdir = new DirectoryInfo($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native");
                foreach (FileInfo f in nativesdir.EnumerateFiles())
                {
                    f.Delete();
                }

                foreach (ZipArchiveEntry dllFile in dllFilesToExtract)
                {
                    dllFile.ExtractToFile($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native/{dllFile.Name}");
                }
                Console.WriteLine("Extracted temp LWJGL natives");
            }

            if (entryPoint.type == JarUtils.EntryPointType.STATIC_VOID_MAIN)
            {
                MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jarConfig.instanceDirName);
                MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jarConfig.instanceDirName + "/.minecraft");

                JavaExec mainFunctionExec = new JavaExec(entryPoint.classpath);

                mainFunctionExec.classPath.Add(Path.GetFullPath(MainWindow.jarDir + "/" + jarConfig.jarFileName));
                if (jarConfig.LWJGLVersion != "+ built-in")
                {
                    mainFunctionExec.classPath.Add($"{MainWindow.currentDirectory}/lwjgl/{jarConfig.LWJGLVersion}/*");
                }

                if (jarConfig.proxyHost != "")
                {
                    mainFunctionExec.jvmArgs.Add($"-Dhttp.proxyHost={jarConfig.proxyHost.Replace(" ", "%20")}");
                }
                mainFunctionExec.jvmArgs.Add($"-Djava.library.path=\"{MainWindow.currentDirectory}/lwjgl/{(jarConfig.LWJGLVersion == "+ built-in" ? "_temp_builtin" : jarConfig.LWJGLVersion)}/native\"");
                mainFunctionExec.jvmArgs.Add(jarConfig.jvmArgs);

                mainFunctionExec.programArgs.Add($"\"{jarConfig.playerName}\"");
                mainFunctionExec.programArgs.Add(jarConfig.sessionID);
                mainFunctionExec.programArgs.Add(jarConfig.gameArgs);
                Console.WriteLine("Running command: java " + mainFunctionExec.GetFullArgsString());

                string emulatedAppDataDir = Path.GetFullPath($"{MainWindow.currentDirectory}/{MainWindow.instanceDir}/{jarConfig.instanceDirName}");
                mainFunctionExec.appdataDir = emulatedAppDataDir;
                mainFunctionExec.workingDirectory = $"{emulatedAppDataDir}{(jarConfig.cwdIsDotMinecraft ? "/.minecraft" : "")}";
                try
                {
                    Process newProcess = mainFunctionExec.Start();
                    WindowProcessLog processLog = new WindowProcessLog(newProcess, caller, jarConfig.isServer);
                    processLog.Show();
                    caller.AddRunningInstance(new UIControls.InstanceListElement.RunningInstanceData(jarConfig.friendlyName == "" ? jarConfig.jarFileName : jarConfig.jarFileName, processLog));
                    Thread.Sleep(1000);
                    Util.SetWindowDarkMode(newProcess.MainWindowHandle);
                }
                catch (Win32Exception w32e)
                {
                    MessageBox.Show($"Error launching java process: {w32e.Message}\n\nVerify that Java is installed in \"Runtime settings\".");
                }
            } 
            else if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                AppletWrapper.TryLaunchAppletWrapper(entryPoint.classpath, caller, jarConfig);
            } 
            else
            {
                throw new NotImplementedException("What");
            }
        }

        private void launchAdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            caller.SaveCurrentJarConfig();
            if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                new WindowAppletParametersOptions(entryPoint.classpath, caller, jarConfig).Show();
            }
        }
    }
}
