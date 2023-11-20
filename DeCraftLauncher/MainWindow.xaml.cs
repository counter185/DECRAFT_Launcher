using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
using DeCraftLauncher.UIControls;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using static DeCraftLauncher.Utils.JarUtils;
using System.Windows.Input;
using DeCraftLauncher.NBTReader;
using System.Xml;

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
            "javazoom.jl",
            "ibxm."
        };

        public static string currentDirectory = "";

        public JarConfig currentlySelectedJar = null;

        public List<WorkerThread> currentScanThreads = new List<WorkerThread>();
        public List<JarEntry> loadedJars = new List<JarEntry>();

        public List<InstanceListElement.RunningInstanceData> runningInstances = new List<InstanceListElement.RunningInstanceData>();

        public void AddRunningInstance(InstanceListElement.RunningInstanceData runningInstance)
        {
            runningInstances.Add(runningInstance);
            UpdateRunningInstancesList();
        }

        public void UpdateRunningInstancesList()
        {
            label_instancesrunning.Content = $"{runningInstances.Count} running instance{(runningInstances.Count != 1 ? "s" : "")}";
            panel_runninginstances.Children.Clear();
            runningInstances.RemoveAll((x) => { return x.processLog.target.HasExited; });
            foreach (InstanceListElement.RunningInstanceData process in runningInstances)
            {
                panel_runninginstances.Children.Add(new InstanceListElement(process));
            }
            panel_instances.Visibility = runningInstances.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

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
            }
            else
            {
                string jar = ((JarListEntry)jarlist.SelectedItem).jar.jarFileName;
                EnsureDefaultJarConfig(jar);
                try
                {
                    currentlySelectedJar = JarConfig.LoadFromXML(configDir + "/" + jar + ".xml", jar);
                } catch (XmlException)
                {
                    File.Delete(configDir + "/" + jar + ".xml");
                    EnsureDefaultJarConfig(jar);
                    currentlySelectedJar = JarConfig.LoadFromXML(configDir + "/" + jar + ".xml", jar);
                }
                jvmargs.Text = currentlySelectedJar.jvmArgs;
                tbox_instance_dir.Text = tbox_server_instance_dir.Text = currentlySelectedJar.instanceDirName;
                tbox_proxyhost.Text = tbox_server_proxyhost.Text = currentlySelectedJar.proxyHost;

                panel_launch_client_options.Visibility = !currentlySelectedJar.isServer ? Visibility.Visible : Visibility.Collapsed;
                panel_launch_server_options.Visibility = currentlySelectedJar.isServer ? Visibility.Visible : Visibility.Collapsed;

                tbox_playername.Text = currentlySelectedJar.playerName;
                window_width.Text = currentlySelectedJar.windowW+"";
                window_height.Text = currentlySelectedJar.windowH+"";
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
                                                                          orderby x.GetImportance() descending
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

                    if (currentlySelectedJar.foundMods.Any())
                    {
                        entrypointlist.Items.Add(new ModsFoundEntryPoint(currentlySelectedJar.foundMods));
                    }

                    if (entrypointlist.Items.Count == 0 && !currentlySelectedJar.entryPointsScanned)
                    {
                        StartEntryPointScan();
                        entrypointlist.Items.Add(new LaunchEntryPointFinding(wthreads.First().report));
                    }
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
            loadedJars.Clear();
            EnsureDir(jarDir);
            EnsureDir(configDir);
            EnsureDir(instanceDir);
            string[] jars = Directory.GetFiles(jarDir);

            List<JarEntry> categorizedEntries = new List<JarEntry>();

            bool hadNonMatchingEntries = false;
            foreach (string a in jars)
            {
                string jarName = a.Substring(jarDir.Length + 1);

                IEnumerable<JarEntry> matchingEntries = (from x in mainRTConfig.jarEntries
                                                         where x.jarFileName == jarName
                                                         select x);
                if (matchingEntries.Any() && matchingEntries.First().category != null)
                {
                    categorizedEntries.Add(matchingEntries.First());
                }
                else
                {
                    loadedJars.Add(matchingEntries.Any() ? matchingEntries.First() : new JarEntry(jarName));
                    if (!matchingEntries.Any())
                    {
                        hadNonMatchingEntries = true;
                    }
                }

                EnsureDefaultJarConfig(jarName);
            }

            // we want categorized entries to appear first
            categorizedEntries.Sort((a, b) => { return a.category.name.CompareTo(b.category.name); });
            loadedJars = categorizedEntries.Concat(loadedJars).ToList();

            loadedJars.ForEach((x) => { jarlist.Items.Add(new JarListEntry(this, x)); });

            if (hadNonMatchingEntries)
            {
                SaveRuntimeConfig();
            }
        }

        public MainWindow()
        {
            currentDirectory = Directory.GetCurrentDirectory();
            try
            {
                InitializeComponent();
            } catch (Exception e)
            {
                MessageBox.Show($"Error starting main window:\n {e}", "DECRAFT");
            }
            mainRTConfig = RuntimeConfig.LoadFromXML();
            Util.UpdateAcrylicWindowBackground(this);
            ShowPanelWelcome();
            //Console.WriteLine(JarUtils.GetJDKInstalled());
            UpdateLWJGLVersions();
            UpdateRunningInstancesList();
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

            label_versionString.Content = GlobalVars.versionCode;
            label_reqJVMVersion.Content = "";

            TextBox[] saveEvents = new TextBox[] {
                jvmargs,
                window_width,
                window_height,
                tbox_playername,
                tbox_instance_dir,
                tbox_proxyhost
            };

            if (Util.RunningOnWine())
            {
                MessageBox.Show("You may be running DECRAFT on the Wine compatibility layer.\nIf the launcher crashes after this popup, open \"winecfg\" and set your Windows version to Windows 7.\nDECRAFT can only use Windows versions of Java, so be sure to install one into your Wine prefix.\n\nGood luck, and expect bugs.", "DECRAFT");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new WindowRuntimeConfig(this).Show();
        }

        private void jarlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Console.WriteLine($"Selection changed: JarListEntry:{((JarListEntry)jarlist.SelectedItem).jarName}");
            if (currentlySelectedJar != null)
            {
                SaveCurrentJarConfig();
                
            } 
            if (jarlist.SelectedItem == null)
            {
                ShowPanelWelcome();
            } else
            {
                UpdateLaunchOptionsSegment();
                ShowPanelLaunchSettings();
            }
            
        }        

        private void StartEntryPointScan()
        {
            //List<EntryPoint> entryPoint = JarUtils.FindAllEntryPoints(jarDir + "/" + currentlySelectedJar.jarFileName);
            Thread nthread = new Thread(ThreadFindEntryPointsAndSaveToXML);
            WorkerThread a = new WorkerThread(nthread, currentlySelectedJar.jarFileName, new ReferenceType<float>(0));
            currentScanThreads.Add(a);
            nthread.Start(a);
        }

        private void btn_scan_entrypoints_Click(object sender, RoutedEventArgs e)
        {
            StartEntryPointScan();
            UpdateLaunchOptionsSegment();
        }

        private void btn_advanced_settings_Click(object sender, RoutedEventArgs e)
        {
            new WindowJarAdvancedOptions(currentlySelectedJar, this).Show();
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
                conf.foundMods = scanRes.modsFound;
                conf.workaroundRetroMCP = scanRes.hasMissingSynthetics;
                string[] serverClassPaths = new string[]
                {
                    "com.mojang.minecraft.server.MinecraftServer",
                    "net.minecraft.server.MinecraftServer",
                };
                conf.isServer = scanRes.entryPoints.Count == 1 && serverClassPaths.Contains(scanRes.entryPoints[0].classpath);
                conf.cwdIsDotMinecraft = !conf.isServer;
                conf.SaveToXMLDefault();
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
                conf.SaveToXMLDefault();
                Dispatcher.Invoke(delegate
                {
                    MessageBox.Show($"Error analyzing {param.jar}: {e.Message}\n\nThe jar file must be a valid zip archive.", "DECRAFT");
                });
                if (currentlySelectedJar.jarFileName == param.jar)
                {
                    Dispatcher.Invoke(delegate
                    {
                        UpdateLaunchOptionsSegment();
                    });
                }
            }
            Console.WriteLine("ThreadFindEntryPointsAndSaveToXML done");
        }

        private void jarlist_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] dt = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string a in dt)
                {
                    if (File.Exists(a)) // goofy edge case
                    {
                        if (a.EndsWith(".jar"))
                        {
                            string copyName = $"{jarDir}/{new FileInfo(a).Name}";
                            if (!File.Exists(copyName)
                                || (File.Exists(copyName)
                                    && MessageBox.Show($"{copyName} already exists. Overwrite?", "DECRAFT", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
                            {
                                File.Copy(a, copyName, true);
                            }
                        }
                        else if (a.EndsWith(".json"))
                        {
                            try
                            {
                                //todo: clean this up
                                JObject rootObj = JObject.Parse(File.ReadAllText(a));
                                string versionID = rootObj.SelectToken("id").Value<string>();
                                JObject dlElement = rootObj.SelectToken("downloads").Value<JObject>().SelectToken("client").Value<JObject>();
                                if (MessageBox.Show($"Download {versionID}?\n\n Size: {dlElement.SelectToken("size").Value<UInt64>()}\n URL: {dlElement.SelectToken("url").Value<string>()}", "DECRAFT", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                {
                                    using (var client = new WebClient())
                                    {
                                        client.DownloadFileCompleted += (sender2, evt) =>
                                        {
                                            if (evt.Error != null)
                                            {
                                                string errorString = $"Download error:\n{evt.Error.Message}";
                                                if (evt.Error is System.Net.WebException && evt.Error.Message.Contains("SSL/TLS"))
                                                {
                                                    errorString += "\n\nYour system's SSL certificates may have expired.";
                                                }
                                                MessageBox.Show(errorString, "DECRAFT");
                                            } else
                                            {
                                                MessageBox.Show("Download complete", "DECRAFT");
                                            }
                                        };
                                        //todo: progress bar for this
                                        client.DownloadFileAsync(new Uri(dlElement.SelectToken("url").Value<string>()), $"{jarDir}/{versionID}.jar");
                                    }
                                }
                            } catch (Exception ex)
                            {
                                MessageBox.Show($"Error reading {a}.\nThe JSON file may be invalid or not in a standard launcher format.\n\n{ex.Message}", "DECRAFT");
                            }
                        }
                        else if (a.EndsWith(".dat") || a.EndsWith(".nbt"))
                        {
                            try
                            {
                                NBTData nbtData = NBTData.FromFile(a);
                                NBTData.PrintNBT(nbtData.rootNode);
                            } catch (Exception ex)
                            {
                                MessageBox.Show($"Error reading NBT data:\n {ex.Message}", "DECRAFT");
                            }
                        }
                    }
                }
            }
        }

        void EnsureDefaultJarConfig(string jarName) {
            if (!File.Exists(configDir + "/" + jarName + ".xml"))
            {
                JarConfig newConf = new JarConfig(jarName);
                newConf.SaveToXMLDefault();
            }
        }

        public void SaveCurrentJarConfig()
        {
            currentlySelectedJar.windowW = uint.TryParse(window_width.Text, out currentlySelectedJar.windowW) ? currentlySelectedJar.windowW : 960;
            currentlySelectedJar.windowH = uint.TryParse(window_height.Text, out currentlySelectedJar.windowH) ? currentlySelectedJar.windowH : 540;

            currentlySelectedJar.jvmArgs = jvmargs.Text;

            currentlySelectedJar.proxyHost = currentlySelectedJar.isServer ? tbox_server_proxyhost.Text : tbox_proxyhost.Text;
            currentlySelectedJar.instanceDirName = currentlySelectedJar.isServer ? tbox_server_instance_dir.Text : tbox_instance_dir.Text;

            currentlySelectedJar.LWJGLVersion = combobox_lwjgl_version.Text;
            currentlySelectedJar.playerName = tbox_playername.Text;

            currentlySelectedJar.SaveToXMLDefault();
        }

        public void SaveRuntimeConfig()
        {
            mainRTConfig.SaveToXML(this);
        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        int pauseBreakEECount = 0;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            //comic sans easter egg
            if (e.Key == Key.Pause)
            {
                if (pauseBreakEECount++ == 5)
                {
                    if (!Util.RunningOnWine())      //the font isn't installed there by default so it will crash
                    {                               //sorry linux people no comic sans
                        FontFamily targetFontFamily = new FontFamily("Comic Sans MS");
                        foreach (TextBlock a in FindLogicalChildren<TextBlock>(this))
                        {
                            a.FontFamily = targetFontFamily;
                        }
                        foreach (Label a in FindLogicalChildren<Label>(this))
                        {
                            a.FontFamily = targetFontFamily;
                        }
                        foreach (TextBox a in FindLogicalChildren<TextBox>(this))
                        {
                            a.FontFamily = targetFontFamily;
                        }
                        foreach (Button a in FindLogicalChildren<Button>(this))
                        {
                            a.FontFamily = targetFontFamily;
                        }
                        pauseBreakEECount = 0;
                    }
                }
            }
            base.OnKeyDown(e);
        }

        public void ShowPanelLaunchSettings()
        {
            segment_welcome.Visibility = Visibility.Hidden;
            segment_launch_options.Visibility = Visibility.Visible;
        }        
        public void ShowPanelWelcome()
        {
            segment_welcome.Visibility = Visibility.Visible;
            segment_launch_options.Visibility = Visibility.Hidden;
        }

        private void jarlist_KeyDown(object sender, KeyEventArgs e)
        {
            if (jarlist.SelectedItem != null && jarlist.SelectedItem is JarListEntry && e.Key == Key.Delete)
            {
                ((JarListEntry)jarlist.SelectedItem).DeleteJar();
            }
        }

        private void btn_editproperties_Click(object sender, RoutedEventArgs e)
        {
            new WindowServerPropertiesEditor($"{instanceDir}/{currentlySelectedJar.instanceDirName}/server.properties").Show();
        }
    }
}
