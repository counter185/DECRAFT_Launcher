using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DeCraftLauncher.JarUtils;

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
        public static string javaHome = "";

        public JarConfig currentlySelectedJar = null;

        public List<WorkerThread> currentScanThreads = new List<WorkerThread>();

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
                entrypointlist.Items.Clear();
                IEnumerable<WorkerThread> wthreads = from x in currentScanThreads where x.jar == jar select x;
                if (wthreads.Any())
                {
                    entrypointlist.Items.Add(new LaunchEntryPointFinding(wthreads.First().report));
                }
                else
                {
                    foreach (EntryPoint a in currentlySelectedJar.entryPoints)
                    {
                        entrypointlist.Items.Add(new LaunchEntryPoint(a, this, currentlySelectedJar));
                    }
                    if (entrypointlist.Items.Count == 0 && !currentlySelectedJar.entryPointsScanned)
                    {
                        btn_scan_entrypoints_Click(null, null);
                        entrypointlist.Items.Add(new LaunchEntryPointFinding(wthreads.First().report));
                    }
                    segment_launch_options.Visibility = Visibility.Visible;
                }
            }

        }

        public void PushJarConfig()
        {
            currentlySelectedJar.windowW = uint.Parse(window_width.Text);
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
            InitializeComponent();
            segment_launch_options.Visibility = Visibility.Hidden;
            //Console.WriteLine(JarUtils.GetJDKInstalled());
            ResetJarlist();
            //Test.TestXMLSaveLoad();
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
                tbox_lwjgl_version,
                tbox_instance_dir
            };

            foreach (TextBox a in saveEvents)
            {
                a.IsKeyboardFocusedChanged += (s, e) =>
                {
                    if ((bool)e.NewValue == false)
                    {
                        PushJarConfig();
                    }
                };
            }
            jvmargs.IsKeyboardFocusedChanged += delegate { };

            //entrypointlist.Items.Add(new LaunchEntryPoint(new JarUtils.EntryPoint("net.minecraft.client.Minecraft", JarUtils.EntryPointType.STATIC_VOID_MAIN), this));
            //entrypointlist.Items.Add(new LaunchEntryPoint(new JarUtils.EntryPoint("net.minecraft.isom.IsomPreviewApplet", JarUtils.EntryPointType.APPLET), this));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Config(this).Show();
        }

        private void rtest_Click(object sender, RoutedEventArgs e)
        {
            List<JarUtils.EntryPoint> epoints = JarUtils.FindAllEntryPoints("./a1.0.16.jar");
            Console.WriteLine(epoints.Count + " entry points");
            foreach (JarUtils.EntryPoint entry in epoints)
            {
                Console.WriteLine(entry.classpath + ", " + entry.type.ToString());
            }
        }

        private void jarlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine($"Selection changed: {jarlist.SelectedItem}");
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
            List<EntryPoint> entryPoint = JarUtils.FindAllEntryPoints(jarDir + "/" + param.jar, param.report);
            currentScanThreads.Remove(param);
            JarConfig conf = JarConfig.LoadFromXML(configDir + "/" + param.jar + ".xml", param.jar);
            conf.entryPoints = entryPoint;
            conf.entryPointsScanned = true;
            conf.SaveToXML(configDir + "/" + param.jar + ".xml");
            if (currentlySelectedJar.jarFileName == param.jar)
            {
                Dispatcher.Invoke(delegate
                {
                    UpdateLaunchOptionsSegment();
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
                        File.Copy(a, jarDir + "/" + new FileInfo(a).Name);
                    }
                }
            }
        }
    }
}
