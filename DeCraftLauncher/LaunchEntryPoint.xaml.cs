﻿using DeCraftLauncher.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
                case "com.mojang.rubydung.RubyDung":
                case "com.mojang.minecraft.RubyDung":
                case "com.mojang.minecraft.Minecraft":
                case "net.minecraft.client.Minecraft":
                    return "Launch the game directly.";
                case "com.mojang.minecraft.MinecraftApplet":
                case "net.minecraft.client.MinecraftApplet":
                    return "Launch the game as a Java applet.";
                case "com.mojang.minecraft.server.MinecraftServer":
                case "net.minecraft.server.MinecraftServer":
                    return "Launch a server.";
                case "net.minecraft.isom.IsomPreviewApplet":
                    return "Open an applet that lets you view your worlds in an isometric view.";
                case "Start":
                    return "Launch using a default wrapper generated by RetroMCP.";
                default:
                    if (entryPoint.classpath.StartsWith("com.jdotsoft.jarloader"))
                    {
                        return "Launch using a loader that will load its own dependencies.";
                    }
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
            moreInfo.Content = target.additionalInfo.Replace("_", "__");
            launchAdvancedButton.Visibility = entryPoint.type == JarUtils.EntryPointType.APPLET ? Visibility.Visible : Visibility.Hidden;
            this.jarConfig = jarConfig;
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
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
                string args = "";
                args += "-cp ";
                args += "\"";
                args += Path.GetFullPath(MainWindow.jarDir + "/" + jarConfig.jarFileName);
                if (jarConfig.LWJGLVersion != "+ built-in")
                {
                    args += $";{MainWindow.currentDirectory}/lwjgl/{jarConfig.LWJGLVersion}/*";
                }
                args += "\" ";
                if (jarConfig.proxyHost != "")
                {
                    args += $"-Dhttp.proxyHost={jarConfig.proxyHost.Replace(" ", "%20")} ";
                }
                args += $"-Djava.library.path=\"{MainWindow.currentDirectory}/lwjgl/{(jarConfig.LWJGLVersion == "+ built-in" ? "_temp_builtin" : jarConfig.LWJGLVersion)}/native\" ";
                //args += $"-Duser.dir=\"{Path.GetFullPath($"{MainWindow.instanceDir}/{jarConfig.instanceDirName}/.minecraft")}\" ";
                args += jarConfig.jvmArgs + " ";
                args += entryPoint.classpath + " ";
                args += $"\"{jarConfig.playerName}\" {jarConfig.sessionID} ";
                args += jarConfig.gameArgs;
                Console.WriteLine("Running command: java " + args);

                Process nproc = null;

                //this is unclean but it's the only way
                string emulatedAppDataDir = Path.GetFullPath($"{MainWindow.currentDirectory}/{MainWindow.instanceDir}/{jarConfig.instanceDirName}");
                Directory.SetCurrentDirectory($"{emulatedAppDataDir}{(jarConfig.cwdIsDotMinecraft ? "/.minecraft" : "")}");
                try
                {
                    nproc = JarUtils.RunProcess($"{MainWindow.mainRTConfig.javaHome}java", args, emulatedAppDataDir);
                }
                catch (Win32Exception w32e)
                {
                    MessageBox.Show($"Error launching java process: {w32e.Message}\n\nVerify that Java is installed in \"Runtime settings\".");
                }
                Directory.SetCurrentDirectory(MainWindow.currentDirectory);

                if (nproc != null)
                {
                    new ProcessLog(nproc).Show();
                }
            } 
            else if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                AppletWrapper.TryLaunchAppletWrapper(entryPoint.classpath, jarConfig);
            } 
            else
            {
                throw new NotImplementedException("bruh");
            }
        }

        private void launchAdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            caller.SaveCurrentJarConfig();
            if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                new AppletParametersOptions(entryPoint.classpath, jarConfig).Show();
            }
        }
    }
}
