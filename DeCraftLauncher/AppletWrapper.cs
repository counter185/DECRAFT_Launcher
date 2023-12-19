using DeCraftLauncher.Configs;
using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static DeCraftLauncher.Utils.JarUtils;

namespace DeCraftLauncher
{
    public class AppletWrapper
    {
        public static void LaunchAppletWrapper(string className, MainWindow caller, JarConfig jar, Dictionary<string, string> appletParameters)
        {
            //first, compile the applet wrapper

            bool isDefaultPackage = !className.Contains(".");

            //todo: clean this up in the same way as i did with javaexec
            MainWindow.EnsureDir("./java_temp");
            File.WriteAllText("./java_temp/AppletWrapper.java", JavaCode.GenerateAppletWrapperCode(className, jar, appletParameters, isDefaultPackage));
            if (jar.appletEmulateHTTP)
            {
                File.WriteAllText("./java_temp/InjectedStreamHandlerFactory.java", JavaCode.GenerateHTTPStreamInjectorCode(jar, isDefaultPackage));
            }
            List<string> compilerOut;
            try
            {
                compilerOut = RunProcessAndGetOutput(MainWindow.mainRTConfig.javaHome + "javac", $"-cp \"{MainWindow.jarDir}/{jar.jarFileName}\" " +
                    $"./java_temp/AppletWrapper.java " +
                    (jar.appletEmulateHTTP ? $"./java_temp/InjectedStreamHandlerFactory.java " : "") +
                    $"-d ./java_temp " +
                    (jar.appletEmulateHTTP && MainWindow.mainRTConfig.isJava9 ? "--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED " : ""), true);
            } catch (ApplicationException)
            {
                PopupOK.ShowNewPopup("Failed to compile the Applet Wrapper.\n\nNote: the Applet Wrapper only supports JDK 6+", "DECRAFT");
                return;
            }
            Console.WriteLine("Compilation log:");
            foreach (string a in compilerOut)
            {
                Console.WriteLine(a);
            }


            //now we launch the compiled class
            JavaExec appletExec = new JavaExec(!isDefaultPackage ? "decraft_internal.AppletWrapper" : "AppletWrapper");

            //class paths
            //todo: make this cleaner (preferrably without getting rid of relative paths)
            string relativePath = (jar.cwdIsDotMinecraft ? "../" : "") + "../../";
            appletExec.classPath.Add($"{relativePath}java_temp/");
            if (jar.LWJGLVersion != "+ built-in")
            {
                appletExec.classPath.Add($"{relativePath}lwjgl/{jar.LWJGLVersion}/*");
            }
            appletExec.classPath.Add($"{relativePath}{MainWindow.jarDir}/{jar.jarFileName}");


            //jvm args
            appletExec.jvmArgs.Add($"-Djava.library.path={relativePath}lwjgl/{(jar.LWJGLVersion == "+ built-in" ? "_temp_builtin" : jar.LWJGLVersion)}/native");
            appletExec.jvmArgs.Add(jar.jvmArgs);
            if (jar.proxyHost != "")
            {
                appletExec.jvmArgs.Add($"-Dhttp.proxyHost={jar.proxyHost.Replace(" ", "%20")}");
            }
            if (MainWindow.mainRTConfig.setHeapDump)
            {
                appletExec.jvmArgs.Add("-XX:HeapDumpPath=dont-mind-me-javaw.exe-minecraft.exe.bin");
            }
            if (jar.appletEmulateHTTP && MainWindow.mainRTConfig.isJava9)
            {
                appletExec.jvmArgs.Add("--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED");
            }

            //game args
            if (jar.gameArgs != "")
            {
                appletExec.programArgs.Add(jar.gameArgs);
            }

            Console.WriteLine($"[LaunchAppletWrapper] Running command: java {appletExec.GetFullArgsString()}");

            string emulatedAppDataDir = Path.GetFullPath($"{MainWindow.currentDirectory}/{MainWindow.instanceDir}/{jar.instanceDirName}");
            appletExec.appdataDir = emulatedAppDataDir;
            appletExec.workingDirectory = $"{emulatedAppDataDir}{(jar.cwdIsDotMinecraft ? "/.minecraft" : "")}";
            try
            {
                appletExec.StartOpenWindowAndAddToInstances(caller, jar, false);

                //nproc = JarUtils.RunProcess(MainWindow.mainRTConfig.javaHome + "java", args, emulatedAppDataDir);
            } catch (Win32Exception w32e)
            {
                PopupOK.ShowNewPopup($"Error launching java process: {w32e.Message}\n\nVerify that Java is installed in \"Runtime settings\".");
            }
        }

        public static void TryLaunchAppletWrapper(string classpath, MainWindow caller, JarConfig jarConfig, Dictionary<string, string> appletParameters = null)
        {

            try
            {
                AppletWrapper.LaunchAppletWrapper(classpath, caller, jarConfig, appletParameters ?? new Dictionary<string, string>());
            }
            catch (Win32Exception)
            {
                PopupOK.ShowNewPopup("Applet wrapper requires JDK installed.");
            }
            
        }
    }
}
