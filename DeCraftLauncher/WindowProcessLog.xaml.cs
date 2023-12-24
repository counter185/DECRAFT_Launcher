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
            this.Title = GlobalVars.L.Translate("window.processlog.codegen.window_title", t.ProcessName, t.Id+"");
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
                    string logTextUpdate = trimmedLines != 0 ? GlobalVars.L.Translate("window.processlog.codegen.hint_log_trimmed", trimmedLines+"") : "";
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
                    logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.exit_code", t.ExitCode+"");
                    if (logtext.Text.Contains("\n\tat "))
                    {
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.hint_translate_stacktrace");
                    }
                    logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                    btn_killprocess.Visibility = Visibility.Hidden;

                    if (logtext.Text.Contains("java.lang.VerifyError"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_verifyerror_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_verifyerror_l2");
                        logtext.Text += "\n\n-noverify";
                    }
                    else if (logtext.Text.Contains("java.lang.IllegalArgumentException: Comparison method violates"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_timsorterror_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_timsorterror_l2");
                        logtext.Text += "\n\n-Djava.util.Arrays.useLegacyMergeSort=true";
                    }
                    else if (logtext.Text.Contains("java.lang.reflect.InaccessibleObjectException: Unable to make field private")
                        || logtext.Text.Contains("java.lang.NoSuchFieldException: packages")
                        || logtext.Text.Contains("java.lang.NoSuchFieldException: modifiers"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_modloadernotj8error_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_modloadernotj8error_l2");
                        if (logtext.Text.Contains("InaccessibleObjectException"))
                        {
                            logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_modloadernotj8error_l3");
                        }
                    }                    
                    else if (logtext.Text.Contains("java.lang.UnsupportedClassVersionError"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_too_old_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_too_old_l2");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_too_old_l3");
                    }                    
                    else if (logtext.Text.Contains("Unrecognized option: --add-exports"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_noaddexports_flag_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_noaddexports_flag_l2");
                    }
                    else if (logtext.Text.Contains("NoClassDefFoundError: joptsimple/OptionSpec"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.hint_onesix_not_supported");
                    }
                    else if (logtext.Text.Contains("NoSuchMethodError: getPointer"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.lwjglerror_class_mismatch_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.lwjglerror_class_mismatch_l2");
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.lwjglerror_class_mismatch_l3");
                    }
                    else if (logtext.Text.Contains("Can't load IA 32-bit .dll on a ARM 64-bit platform"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.dllerror_x86_on_arm_java_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.dllerror_x86_on_arm_java_l2");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.dllerror_x86_on_arm_java_l3");
                    }
                    else if (logtext.Text.Contains("java.lang.NoClassDefFoundError: net/minecraft/client/Minecraft$SyntheticClass_1"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_mcp_nosynthclass_l1");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_mcp_nosynthclass_l2");
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.javaerror_mcp_nosynthclass_l3");
                    }
                    if (MainWindow.mainRTConfig.autoExitProcessLog)
                    {
                        logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.hint_autoexit");
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

            GlobalVars.L.Translate(
                    btn_sendstdin,
                    btn_killprocess
                );
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
            btn_killprocess.Visibility = Visibility.Hidden;
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
                            methodName = GlobalVars.L.Translate("window.processlog.codegen.hint_remapper_multiplechoice", String.Join(",", possibleMethodNames));
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
                    logtext.Text += GlobalVars.L.Translate("window.processlog.codegen.autoexit_abort");
                }
            }
            else if (e.Key == Key.F3)
            {
                panel_stdin.Visibility = Visibility.Visible;
            }
            else if (e.Key == Key.F4)
            {
                logtext.Text = "";
            }
            base.OnKeyDown(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
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
