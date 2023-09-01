using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy ProcessLog.xaml
    /// </summary>
    public partial class ProcessLog : AcrylicWindow
    {

        public Process target;

        public void ThreadLoggerStdOut()
        {
            Stopwatch timer = new Stopwatch();
            string stdout_buffer = "";
            while (!target.HasExited)
            {
                if (!target.StandardOutput.EndOfStream)
                {
                    stdout_buffer += target.StandardOutput.ReadLine() + "\n";
                    if (!timer.IsRunning)
                    {
                        timer.Start();
                    }
                }
                if (timer.IsRunning && timer.ElapsedMilliseconds > 64)
                {
                    timer.Stop();
                }
                if (stdout_buffer != "" && !timer.IsRunning)
                {
                    Dispatcher.Invoke(delegate
                    {
                        logtext.Text += stdout_buffer;
                        logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                        stdout_buffer = "";
                    });

                }
                Thread.Sleep(1);
            }

            Console.WriteLine("ThreadLogger exit");
        }
        public void ThreadLoggerStdErr()
        {
            int dispatcherTimer = 0;
            string stderr_buffer = "";
            while (!target.HasExited)
            {
                if (!target.StandardError.EndOfStream)
                {
                    stderr_buffer += target.StandardError.ReadLine() + "\n";
                }
                if (stderr_buffer != "" && dispatcherTimer <= 0)
                {
                    Dispatcher.Invoke(delegate
                    {
                        logtext.Text += stderr_buffer;
                        logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                        stderr_buffer = "";
                    });
                    dispatcherTimer = 100;
                }
                Thread.Sleep(1);
                if (dispatcherTimer > 0)
                {
                    dispatcherTimer--;
                }
            }

            Console.WriteLine("ThreadLoggerStdErr exit");
        }
        public ProcessLog(Process t)
        {
            target = t;
            InitializeComponent();
            Utils.UpdateAcrylicWindowBackground(this);
            t.OutputDataReceived += (a, b) =>
            {
                //Console.WriteLine("recvd: " + b.Data);
                Dispatcher.Invoke(delegate
                {
                    logtext.Text += b.Data + "\n";
                    logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                });
                
            };
            t.ErrorDataReceived += (a, b) =>
            {
                Dispatcher.Invoke(delegate
                {
                    logtext.Text += b.Data + "\n";
                    logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                });

            };
            t.BeginErrorReadLine();
            t.BeginOutputReadLine();
            t.EnableRaisingEvents = true;
            t.Exited += delegate
            {
                Dispatcher.Invoke(delegate
                {
                    logtext.Text += "Process exited with code " + t.ExitCode;
                    logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
                    proc_kill.Visibility = Visibility.Hidden;

                    if (t.ExitCode == -1 && logtext.Text.Contains("java.lang.VerifyError"))
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
                        || logtext.Text.Contains("java.lang.NoSuchFieldException: modifiers"))
                    {
                        logtext.Text += "\n----------------------------------------------";
                        logtext.Text += "\n";
                        logtext.Text += "\nThe launch may have failed due to a mod loader expecting a field from an older version of Java.";
                        logtext.Text += "\nOpen Runtime settings and set the path to the \"bin\" folder of an older version of Java.";
                        if (logtext.Text.Contains("InaccessibleObjectException"))
                        {
                            logtext.Text += "\n\nAlternatively, if you know what you're doing, you can try adding \"--add-opens <module>/<export>=ALL-UNNAMED\" with the right fields to your JVM arguments.";
                        }
                    }
                });
            };
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
        }

        private void proc_kill_Click(object sender, RoutedEventArgs e)
        {
            target.Kill();
            proc_kill.Visibility = Visibility.Hidden;
        }
    }
}
