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
            Dispatcher.Invoke(delegate
            {
                logtext.Text += $"\nProcess exited with code {target.ExitCode}";
            });

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
                logtext.Text += "Process exited with code " + t.ExitCode;
                logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
            };

            /*new Thread(ThreadLoggerStdOut).Start();
            new Thread(ThreadLoggerStdErr).Start();*/
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            logscroller.ScrollToVerticalOffset(logscroller.ExtentHeight);
        }
    }
}
