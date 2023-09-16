using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
using DeCraftLauncher.UIControls;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static DeCraftLauncher.Utils.JarUtils;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AcrylicWindow
    {
        public const string jarDir = "./jars";
        public const string configDir = "./config";
        public const string instanceDir = "./instance";
        public static RuntimeConfig mainRTConfig = new RuntimeConfig();

        public readonly string[] unimportantClasspaths = new string[] { 
            "org.jsoup.",
            "org.newdawn.",
            "org.lwjgl.",
            "org.mozilla.javascript.",
            "com.jcraft.jorbis.",
            "net.java.games.",
            "ibxm."
        };

        public static string currentDirectory = "";

        public JarConfig currentlySelectedJar = null;

        public List<WorkerThread> currentScanThreads = new List<WorkerThread>();

        public void UpdateLWJGLVersions()
        {
            string currentItem = combobox_lwjgl_version.Text;
            combobox_lwjgl_version.Items.Clear();
            if (currentlySelectedJar != null && currentlySelectedJar.jarHasLWJGLClasses)
            {
                TextBlock nTextBlock = new TextBlock();
                nTextBlock.Text = "+ built-in";
                nTextBlock.Foreground = Brushes.White;
                combobox_lwjgl_version.Items.Add(nTextBlock);
            }
            foreach (string lwjglSubdir in Directory.GetDirectories($"./lwjgl"))
            {
                string versionName = lwjglSubdir.Substring(lwjglSubdir.LastIndexOf('\\') + 1);
                if (versionName != "_temp_builtin")
                {
                    TextBlock nTextBlock = new TextBlock();
                    nTextBlock.Text = versionName;
                    nTextBlock.Foreground = Brushes.White;
                    combobox_lwjgl_version.Items.Add(nTextBlock);
                }
            }
            combobox_lwjgl_version.Text = currentItem;
        }

        public void UpdateLaunchOptionsSegment()
        {
            if (jarlist.SelectedItem == null)
            {
                currentlySelectedJar = null;
                segment_launch_options.Visibility = Visibility.Hidden;
            }
            else
            {
                string jar = (string)jarlist.SelectedItem;
                currentlySelectedJar = JarConfig.LoadFromXML(configDir + "/" + jar + ".xml", jar);
                tbox_playername.Text = currentlySelectedJar.playerName;
                jvmargs.Text = currentlySelectedJar.jvmArgs;
                window_width.Text = currentlySelectedJar.windowW+"";
                window_height.Text = currentlySelectedJar.windowH+"";
                tbox_instance_dir.Text = currentlySelectedJar.instanceDirName;
                tbox_proxyhost.Text = currentlySelectedJar.proxyHost;
                combobox_lwjgl_version.Text = currentlySelectedJar.LWJGLVersion;
                if (currentlySelectedJar.maxJavaVersion != "")
                {
                    label_reqJVMVersion.Content =
                        currentlySelectedJar.maxJavaVersion != currentlySelectedJar.minJavaVersion ?
                        $"req.JVM: {Util.JavaVersionFriendlyName(currentlySelectedJar.minJavaVersion)} - {Util.JavaVersionFriendlyName(currentlySelectedJar.maxJavaVersion)}"
                        : $"req.JVM: {Util.JavaVersionFriendlyName(currentlySelectedJar.maxJavaVersion)}";
                } else
                {
                    label_reqJVMVersion.Content = "";
                }
                entrypointlist.Items.Clear();
                IEnumerable<WorkerThread> wthreads = from x in currentScanThreads where x.jar == jar select x;
                if (wthreads.Any())
                {
                    entrypointlist.Items.Add(new LaunchEntryPointFinding(wthreads.First().report));
                }
                else
                {

                    IEnumerable<EntryPoint> unimportantLaunchEntryPoints = (from x in currentlySelectedJar.entryPoints
                                                                            where (from y in unimportantClasspaths
                                                                                   where x.classpath.StartsWith(y)
                                                                                   select y).Any()
                                                                            select x);
                    IEnumerable<EntryPoint> importantLaunchEntryPoints = (from x in currentlySelectedJar.entryPoints
                                                                          where !unimportantLaunchEntryPoints.Contains(x)
                                                                          select x);

                    foreach (EntryPoint a in importantLaunchEntryPoints)
                    {
                        entrypointlist.Items.Add(new LaunchEntryPoint(a, this, currentlySelectedJar));
                    }
                    if (unimportantLaunchEntryPoints.Any())
                    {
                        //add a line to separate them
                        entrypointlist.Items.Add(new Rectangle
                        {
                            Width = 400,
                            Height = 1,
                            Fill = Brushes.White,
                            Opacity = 0.3
                        });
                        foreach (EntryPoint a in unimportantLaunchEntryPoints)
                        {
                            entrypointlist.Items.Add(new UnimportantLaunchEntryPoint(a, this, currentlySelectedJar));
                        }
                    }

                    if (entrypointlist.Items.Count == 0 && !currentlySelectedJar.entryPointsScanned)
                    {
                        btn_scan_entrypoints_Click(null, null);
                        entrypointlist.Items.Add(new LaunchEntryPointFinding(wthreads.First().report));
                    }
                    segment_launch_options.Visibility = Visibility.Visible;
                }
                UpdateLWJGLVersions();
            }

        }

        public static void EnsureDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void ResetJarlist()
        {
            jarlist.Items.Clear();
            EnsureDir(jarDir);
            EnsureDir(configDir);
            EnsureDir(instanceDir);
            string[] jars = Directory.GetFiles(jarDir);
            foreach (string a in jars)
            {
                string jarName = a.Substring(jarDir.Length + 1);
                jarlist.Items.Add(jarName);
                if (!File.Exists(configDir + "/" + jarName + ".xml"))
                {
                    JarConfig newConf = new JarConfig(jarName);
                    newConf.SaveToXML(configDir + "/" + jarName + ".xml");
                }
            }
        }

        public MainWindow()
        {
            currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                InitializeComponent();
            } catch (XamlParseException e)
            {
                MessageBox.Show($"Error starting main window:\n {e}", "DECRAFT");
            }
            mainRTConfig = RuntimeConfig.LoadFromXML();
            Util.UpdateAcrylicWindowBackground(this);
            segment_launch_options.Visibility = Visibility.Hidden;
            //Console.WriteLine(JarUtils.GetJDKInstalled());
            UpdateLWJGLVersions();
            FileSystemWatcher lwjglVersionWatcher = new FileSystemWatcher("./lwjgl");
            lwjglVersionWatcher.EnableRaisingEvents = true;
            lwjglVersionWatcher.Created += delegate { Dispatcher.Invoke(UpdateLWJGLVersions); };
            lwjglVersionWatcher.Deleted += delegate { Dispatcher.Invoke(UpdateLWJGLVersions); };
            lwjglVersionWatcher.Renamed += delegate { Dispatcher.Invoke(UpdateLWJGLVersions); };
            ResetJarlist();
            //Test.TestXMLSaveLoad();
            //Test.TestClassParse();
            FileSystemWatcher watcher = new FileSystemWatcher("./jars", "*.jar");
            watcher.EnableRaisingEvents = true;
            watcher.Created += delegate { Dispatcher.Invoke(ResetJarlist); };
            watcher.Deleted += delegate { Dispatcher.Invoke(ResetJarlist); };
            watcher.Renamed += delegate { Dispatcher.Invoke(ResetJarlist); };

            TextBox[] saveEvents = new TextBox[] {
                jvmargs,
                window_width,
                window_height,
                tbox_playername,
                tbox_instance_dir,
                tbox_proxyhost
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new WindowRuntimeConfig(this).Show();
        }

        private void rtest_Click(object sender, RoutedEventArgs e)
        {
            List<JarUtils.EntryPoint> epoints = JarUtils.FindAllEntryPoints("./a1.0.16.jar").entryPoints;
            Console.WriteLine(epoints.Count + " entry points");
            foreach (JarUtils.EntryPoint entry in epoints)
            {
                Console.WriteLine(entry.classpath + ", " + entry.type.ToString());
            }
        }

        private void jarlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine($"Selection changed: {jarlist.SelectedItem}");
            if (currentlySelectedJar != null)
            {
                SaveCurrentJarConfig();
            }
            UpdateLaunchOptionsSegment();
        }        

        private void btn_scan_entrypoints_Click(object sender, RoutedEventArgs e)
        {
            //List<EntryPoint> entryPoint = JarUtils.FindAllEntryPoints(jarDir + "/" + currentlySelectedJar.jarFileName);
            Thread nthread = new Thread(ThreadFindEntryPointsAndSaveToXML);
            WorkerThread a = new WorkerThread(nthread, currentlySelectedJar.jarFileName, new ReferenceType<float>(0));
            currentScanThreads.Add(a);
            nthread.Start(a);
        }

        private void btn_advanced_settings_Click(object sender, RoutedEventArgs e)
        {
            new WindowJarAdvancedOptions(currentlySelectedJar).Show();
        }        

        private void btn_open_instance_dir_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentJarConfig();
            EnsureDir($"{instanceDir}/{currentlySelectedJar.instanceDirName}");
            JarUtils.RunProcess("explorer", System.IO.Path.GetFullPath($"{instanceDir}/{currentlySelectedJar.instanceDirName}"));
        }

        public class WorkerThread
        {
            public Thread a;
            public string jar;
            public volatile ReferenceType<float> report;

            public WorkerThread(Thread a, string jar, ReferenceType<float> report)
            {
                this.a = a;
                this.jar = jar;
                this.report = report;
            }
        }

        public void ThreadFindEntryPointsAndSaveToXML(object obj)
        {
            WorkerThread param = (WorkerThread)obj;
            try
            {
                EntryPointScanResults scanRes = JarUtils.FindAllEntryPoints(jarDir + "/" + param.jar, param.report);
                List<EntryPoint> entryPoint = scanRes.entryPoints;
                currentScanThreads.Remove(param);
                JarConfig conf = JarConfig.LoadFromXML(configDir + "/" + param.jar + ".xml", param.jar);
                conf.entryPoints = entryPoint;
                conf.entryPointsScanned = true;
                conf.maxJavaVersion = $"{scanRes.maxMajorVersion}.{scanRes.maxMinorVersion}";
                conf.minJavaVersion = $"{scanRes.minMajorVersion}.{scanRes.minMinorVersion}";
                conf.jarHasLWJGLClasses = scanRes.hasLWJGLBuiltIn;
                conf.jarBuiltInLWJGLDLLs = scanRes.lwjglNativesDir;
                conf.SaveToXML(configDir + "/" + param.jar + ".xml");
                if (currentlySelectedJar.jarFileName == param.jar)
                {
                    Dispatcher.Invoke(delegate
                    {
                        UpdateLaunchOptionsSegment();
                    });
                }
            } catch (Exception e)
            {
                currentScanThreads.Remove(param);
                JarConfig conf = JarConfig.LoadFromXML(configDir + "/" + param.jar + ".xml", param.jar);
                conf.entryPoints = new List<EntryPoint>();
                conf.entryPointsScanned = true;
                conf.SaveToXML(configDir + "/" + param.jar + ".xml");
                Dispatcher.Invoke(delegate
                {
                    MessageBox.Show($"Error analyzing {param.jar}: {e.Message}\n\nThe jar file must be a valid zip archive.", "DECRAFT");
                });
            }
            Console.WriteLine("ThreadFindEntryPointsAndSaveToXML done");
        }

        private void jarlist_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] dt = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string a in dt)
                {
                    if (a.EndsWith(".jar") && File.Exists(a))
                    {
                        string copyName = $"{jarDir}/{new FileInfo(a).Name}";
                        if (!File.Exists(copyName)
                            || (File.Exists(copyName) 
                                && MessageBox.Show($"{copyName} already exists. Overwrite?", "DECRAFT", MessageBoxButton.YesNo) == MessageBoxResult.Yes)) {
                            File.Copy(a, copyName, true);
                        }
                    }
                }
            }
        }

        public void SaveCurrentJarConfig()
        {
            currentlySelectedJar.windowW = uint.TryParse(window_width.Text, out currentlySelectedJar.windowW) ? currentlySelectedJar.windowW : 960;
            currentlySelectedJar.windowH = uint.TryParse(window_height.Text, out currentlySelectedJar.windowH) ? currentlySelectedJar.windowH : 540;

            currentlySelectedJar.jvmArgs = jvmargs.Text;
            currentlySelectedJar.LWJGLVersion = combobox_lwjgl_version.Text;
            currentlySelectedJar.playerName = tbox_playername.Text;
            currentlySelectedJar.instanceDirName = tbox_instance_dir.Text;
            currentlySelectedJar.proxyHost = tbox_proxyhost.Text;

            currentlySelectedJar.SaveToXML(configDir + "/" + currentlySelectedJar.jarFileName + ".xml");
        }

        public static void SaveRuntimeConfig()
        {
            mainRTConfig.SaveToXML();
        }
    }
}
