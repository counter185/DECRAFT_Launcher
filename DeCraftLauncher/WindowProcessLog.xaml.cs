using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DeCraftLauncher
{
    public partial class WindowProcessLog : AcrylicWindow
    {
        const int MAX_LINES = 100;

        int trimmedLines = 0;
        public IList lines = ArrayList.Synchronized(new List<string>() { "" });
        public bool hasNewStdoutData = false;
        public DispatcherTimer logPrintTimer = new DispatcherTimer();
        public MainWindow parent;

        public Process target;
        private volatile bool autoExitTimerStarted = false;
        private volatile bool abortAutoExit = false;

        public WindowProcessLog(Process t, MainWindow parent, bool allowStdin = false)
        {
            this.target = t;
            this.parent = parent;
            InitializeComponent();
            this.Title = $"DECRAFT: Process Log [{t.ProcessName} : {t.Id}]";
            panel_stdin.Visibility = allowStdin ? Visibility.Visible : Visibility.Collapsed;
            Util.UpdateAcrylicWindowBackground(this);
            t.OutputDataReceived += (a, b) =>
            {
                lines.Add(b.Data);
                while (lines.Count > MAX_LINES)
                {
                    lines.RemoveAt(0);
                    trimmedLines++;
                }
                hasNewStdoutData = true;
            };
            t.ErrorDataReceived += (a, b) =>
            {
                lines.Add(b.Data);
                while (lines.Count > MAX_LINES)
                {
                    lines.RemoveAt(0);
                    trimmedLines++;
                }
                hasNewStdoutData = true;
            };
            tbox_contentstdin.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    SendSTDIN();
                }
            };

            logPrintTimer.Interval = TimeSpan.FromMilliseconds(32);    //30 fps
            logPrintTimer.Tick += delegate
            {
                if (hasNewStdoutData)
                {
                    hasNewStdoutData = false;
                    string logTextUpdate = trimmedLines != 0 ? $"(trimmed {trimmedLines} lines)\n" : "";
                    try
                    {
                        foreach (string logLine in lines)
                        {
                            logTextUpdate += logLine + "\n";
                        }
                        logtext.Text = logTextUpdate;
                        logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                    } catch (InvalidOperationException)
                    {
                        //very crackhead fix for a concurrent list modification error...
                        hasNewStdoutData = true;
                    }
                }
            };
            logPrintTimer.Start();

            t.BeginErrorReadLine();
            t.BeginOutputReadLine();
            t.EnableRaisingEvents = true;
            t.Exited += delegate
            {
                Thread.Sleep(500);
                while (hasNewStdoutData)
                {
                    Thread.Sleep(100);
                }
                logPrintTimer.Stop();
                Dispatcher.Invoke(delegate
                {
                    parent.UpdateRunningInstancesList();
                    logtext.Text += "Process exited with code " + t.ExitCode;
                    if (logtext.Text.Contains("\n\tat "))
                    {
                        logtext.Text += "\n[F1]: translate the stack trace using a TinyV2 mappings file";
                    }
                    logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                    proc_kill.Visibility = Visibility.Hidden;

                    if (logtext.Text.Contains("java.lang.VerifyError"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe launch failed due to a bytecode verification error.";
                        logtext.Text += "\nAdd this to your JVM arguments to try launching anyway:";
                        logtext.Text += "\n\n-noverify";
                    }
                    else if (logtext.Text.Contains("java.lang.IllegalArgumentException: Comparison method violates"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe game crashed due to the sorting algorithm being given invalid data.";
                        logtext.Text += "\nAdd this to your JVM arguments to use an older algorithm that ignores invalid data:";
                        logtext.Text += "\n\n-Djava.util.Arrays.useLegacyMergeSort=true";
                    }
                    else if (logtext.Text.Contains("java.lang.reflect.InaccessibleObjectException: Unable to make field private")
                        || logtext.Text.Contains("java.lang.NoSuchFieldException: packages")
                        || logtext.Text.Contains("java.lang.NoSuchFieldException: modifiers"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe launch may have failed due to a mod loader expecting a field from an older version of Java.";
                        logtext.Text += "\nOpen Runtime settings and set the path to the \"bin\" folder of an older version of Java (8 recommended).";
                        if (logtext.Text.Contains("InaccessibleObjectException"))
                        {
                            logtext.Text += "\n\nAlternatively, if you know what you're doing, you can try adding \"--add-opens <module>/<export>=ALL-UNNAMED\" with the right fields to your JVM arguments.";
                        }
                    }                    
                    else if (logtext.Text.Contains("java.lang.UnsupportedClassVersionError"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nYour current Java version is too old to run this.";
                        logtext.Text += "\nOpen Runtime settings and set the path to the \"bin\" folder of a newer version of Java.";
                        logtext.Text += "\n\n(Note: if you're running Java 5 and trying to launch a jar built with JDK 5, use an older version of LWJGL.)";
                    }                    
                    else if (logtext.Text.Contains("Unrecognized option: --add-exports"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nYour current Java version may be too old to support the \"--add-exports\" flag, which is required for the \"Emulate HTTP Server\" option.";
                        logtext.Text += "\nOpen Runtime settings and uncheck the \"Use required Java 9+ options\" option.";
                    }
                    else if (logtext.Text.Contains("NoClassDefFoundError: joptsimple/OptionSpec"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nDECRAFT does not support versions newer than release 1.5.2.";
                    }
                    else if (logtext.Text.Contains("NoSuchMethodError: getPointer"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe launch failed due to a version mismatch between LWJGL's Java library and its DLLs.";
                        logtext.Text += "\nThis jar may have been packaged with a different LWJGL version. ";
                        logtext.Text += "\n";
                        logtext.Text += "\nCheck if this jar contains a built-in LWJGL version in the \"LWJGL Version\" dropdown.";
                    }
                    else if (logtext.Text.Contains("Can't load IA 32-bit .dll on a ARM 64-bit platform"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe launch failed, because you are using a Java version designed for ARM64, which cannot load x86 DLLs.";
                        logtext.Text += "\nIf possible, open Runtime settings set the Java path to a non-ARM version of Java.";
                        logtext.Text += "\nAlternatively, add a version of LWJGL with ARM DLLs.";
                    }
                    else if (logtext.Text.Contains("java.lang.NoClassDefFoundError: net/minecraft/client/Minecraft$SyntheticClass_1"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nYou are seeing this error, because a modding tool improperly packaged your build.";
                        logtext.Text += "\nTo launch this jar, open \"Advanced settings\" and enable \"Workaround missing synthetic classes\".";
                        logtext.Text += "\nNote: a JDK is required to use this feature.";
                    }
                    if (MainWindow.mainRTConfig.autoExitProcessLog)
                    {
                        logtext.Text += "\n\nExiting in 5 seconds... [F2] to keep this window open";
                    }
                });
                autoExitTimerStarted = true;
                if (MainWindow.mainRTConfig.autoExitProcessLog)
                {
                    Thread.Sleep(5000);
                    if (!abortAutoExit)
                    {
                        Dispatcher.Invoke(delegate
                        {
                            this.Close();
                        });
                    }
                }
            };
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
        }

        private void proc_kill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                target.Kill();
            } catch (System.InvalidOperationException)
            {
                //??????
            }
            proc_kill.Visibility = Visibility.Hidden;
        }

        public string ProcessLogTranslateString(string a, TinyV2Mapper tinyV2Mapper)
        {
            if (a.StartsWith("\tat "))
            {
                string classPathString = a.Substring(4).Split('(')[0];
                int l = classPathString.LastIndexOf('.');
                string classPath = classPathString.Substring(0, l);
                string methodName = classPathString.Substring(l+1);

                var possibleClassPaths = (from x in tinyV2Mapper.remappedClasses
                                          where x.@from == classPath.Replace('.', '/')
                                          select x);
                if (!possibleClassPaths.Any())
                {
                    possibleClassPaths = (from x in tinyV2Mapper.remappedClasses
                                          where x.to == classPath.Replace('.', '/')
                                          select x);
                }
                if (possibleClassPaths.Any())
                {
                    TinyV2Mapper.ClassMapping fClassPath = possibleClassPaths.First();
                    classPath = fClassPath.to.Replace('/', '.');
                    try
                    {
                        var possibleMethodNames = (from x in fClassPath.remappedMethods
                                                   where x.@from == methodName
                                                   select x.to);
                        if (possibleMethodNames.Count() == 1)
                        {
                            methodName = possibleMethodNames.First();
                        }
                        else if (possibleMethodNames.Count() > 1)
                        {
                            methodName = $"<multiple choices: {String.Join(",", possibleMethodNames)}>";
                        }
                    } catch (InvalidOperationException)
                    {
                    }
                }

                return $"\tat {classPath}.{methodName}{a.Substring(a.LastIndexOf('('))}";

            } else
            {
                return a;
            }
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                OpenFileDialog tinyV2MapDialog = new OpenFileDialog();
                tinyV2MapDialog.Filter = "TinyV2 Files|*.tiny";
                if (tinyV2MapDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TinyV2Mapper tinyV2Mapper = TinyV2Mapper.FromMappingsFile(tinyV2MapDialog.FileName);
                    Console.WriteLine("Read " + tinyV2Mapper.remappedClasses.Count + " remapped classes");
                    string currentLogText = logtext.Text;
                    logtext.Text = String.Join("\n", (from x in currentLogText.Split('\n')
                                                      select ProcessLogTranslateString(x, tinyV2Mapper)));
                }
            }
            else if (e.Key == Key.F2)
            {
                if (autoExitTimerStarted)
                {
                    abortAutoExit = true;
                    Dispatcher.Invoke(delegate
                    {
                        logtext.Text += "\nAuto exit aborted.";
                    });
                }
            }
            else if (e.Key == Key.F3)
            {
                panel_stdin.Visibility = Visibility.Visible;
            }
            base.OnKeyDown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            /*if (!target.HasExited)
            {
                if (System.Windows.MessageBox.Show("This process is still running.\nClosing this window will keep it in the background. Close anyway?", "DECRAFT", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    
                }
            }*/
            if (!target.HasExited) {
                e.Cancel = true;
                this.Hide();
            }
            base.OnClosing(e);
        }

        public void SendSTDIN()
        {
            if (!target.HasExited)
            {
                target.StandardInput.WriteLine(tbox_contentstdin.Text);
                tbox_contentstdin.Text = "";
            }
        }

        private void btn_sendstdin_Click(object sender, RoutedEventArgs e)
        {
            SendSTDIN();
        }
    }
}
