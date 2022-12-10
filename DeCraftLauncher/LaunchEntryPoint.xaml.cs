using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy LaunchEntryPoint.xaml
    /// </summary>
    public partial class LaunchEntryPoint : UserControl
    {
        JarUtils.EntryPoint entryPoint;
        JarConfig jarConfig;
        MainWindow caller;

        public String GetDescription()
        {
            switch (entryPoint.classpath)
            {
                case "net.minecraft.client.Minecraft":
                    return "Launches the game directly.";
                case "net.minecraft.client.MinecraftApplet":
                    return "Launches the game as a Java applet.";
                case "net.minecraft.isom.IsomPreviewApplet":
                    return "Opens an applet that lets you view your worlds in an isometric view.";
                default:
                    return "<unknown>";
            }
        }

        public LaunchEntryPoint(JarUtils.EntryPoint target, MainWindow caller, JarConfig jarConfig)
        {
            InitializeComponent();
            this.caller = caller;
            entryPoint = target;
            classname.Content = entryPoint.classpath;
            desc.Content = GetDescription();
            mode.Content =
                entryPoint.type == JarUtils.EntryPointType.STATIC_VOID_MAIN ? "(main function)"
                : entryPoint.type == JarUtils.EntryPointType.RUNNABLE ? "(Runnable)"
                : entryPoint.type == JarUtils.EntryPointType.APPLET ? "(Applet)"
                : "<unknown>";
            this.jarConfig = jarConfig;
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            if (entryPoint.type == JarUtils.EntryPointType.STATIC_VOID_MAIN)
            {
                MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jarConfig.instanceDirName);
                MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jarConfig.instanceDirName + "/.minecraft");
                string args = "";
                args += "-cp ";
                args += Path.GetFullPath(MainWindow.jarDir + "/" + jarConfig.jarFileName);
                args += $";{Directory.GetCurrentDirectory()}/lwjgl/2.9.3/* ";
                args += $"-Djava.library.path={Directory.GetCurrentDirectory()}/lwjgl/2.9.3/native ";
                args += entryPoint.classpath;
                args += " " + jarConfig.playerName + " 0";
                Console.WriteLine("Running command: java " + args);

                Process nproc = JarUtils.RunProcess("cmd", $"/c cd {MainWindow.instanceDir + "/" + jarConfig.instanceDirName + "/.minecraft"} && " + MainWindow.javaHome + "java " +  args, Path.GetFullPath(MainWindow.instanceDir + "/" + jarConfig.instanceDirName));
                new ProcessLog(nproc).Show();
            } 
            else if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                AppletWrapper.LaunchAppletWrapper(entryPoint.classpath, jarConfig);
            } 
            else
            {
                throw new NotImplementedException("bruh");
            }
        }
    }
}
