using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeCraftLauncher
{
    public unsafe class JarUtils
    {
        public static List<string> RunProcessAndGetOutput(string program, string args) {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            proc.Start();
            proc.WaitForExit(75);
            List<string> stdout = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                stdout.Add(proc.StandardOutput.ReadLine());
            }
            while (!proc.StandardError.EndOfStream)
            {
                stdout.Add(proc.StandardError.ReadLine());
            }
            return stdout;
        }

        public static Process RunProcess(string program, string args, string appdataDir = "")
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            if (appdataDir != "") {
                startInfo.EnvironmentVariables["appdata"] = appdataDir;
                //startInfo.EnvironmentVariables["cd"] = appdataDir + "\\.minecraft";
            }

            Process proc = new Process
            {
                StartInfo = startInfo
            };
            proc.Start();
            return proc;
        }

        public static string GetJREInstalled(string at)
        {
            try
            {
                return RunProcessAndGetOutput(at + "java", "-version")[0];
            } catch (Exception)
            {
                return null;
            }
        }

        public static string GetJDKInstalled(string at)
        {
            try
            {
                return RunProcessAndGetOutput(at + "javac", "-version")[0];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public enum EntryPointType
        {
            RUNNABLE = 0,
            APPLET = 1, //definitely todo for a long time
            STATIC_VOID_MAIN = 2,

        }

        public struct EntryPoint
        {
            public string classpath;
            public EntryPointType type;

            public EntryPoint(string classpath, EntryPointType type)
            {
                this.classpath = classpath;
                this.type = type;
            }
        }

        public struct EPFinderThread_args
        {
            public string jarfile; public List<ZipArchiveEntry> arcEntries; public List<EntryPoint> returnPoint; public Mutex mtx;
            public int num;
            public int* progress_rep;

            public EPFinderThread_args(string jarfile, List<ZipArchiveEntry> arcEntries, List<EntryPoint> returnPoint, Mutex mtx, int num, int* progress_rep)
            {
                this.jarfile = jarfile;
                this.arcEntries = arcEntries;
                this.returnPoint = returnPoint;
                this.mtx = mtx;
                this.num = num;
                this.progress_rep = progress_rep;
            }
        }

        private static void Thread_EntryPointFinder(object arg)
        {
            EPFinderThread_args args = (EPFinderThread_args)arg;
            string javap_prefix = $"-cp {args.jarfile} ";
            foreach (ZipArchiveEntry entry in args.arcEntries)
            {
                string ename = entry.FullName.Substring(0, entry.FullName.Length - 6).Replace('/', '.');
                //Console.WriteLine("[" + args.num + "] " + ename);
                List<string> javap_output = RunProcessAndGetOutput("javap", javap_prefix + ename);
                //args.mtx.WaitOne();
                foreach (string a in javap_output)
                {
                    
                    /*if (a.Contains("implements java.lang.Runnable"))
                    {
                        //Console.WriteLine("Entry point found");
                        args.returnPoint.Add(new EntryPoint(ename, EntryPointType.RUNNABLE));
                        
                    }
                    else */if (a.Contains("public static void main(java.lang.String[])"))
                    {
                        //Console.WriteLine("Entry point found");
                        args.returnPoint.Add(new EntryPoint(ename, EntryPointType.STATIC_VOID_MAIN));
                    }
                    else if (a.Contains("extends java.applet.Applet"))
                    {
                        //Console.WriteLine("Entry point found");
                        args.returnPoint.Add(new EntryPoint(ename, EntryPointType.APPLET));
                    }
                    
                }
                (*args.progress_rep)++;
                //args.mtx.ReleaseMutex();
            }
            //Console.WriteLine("Thread "+args.num+" finished");
        }

        public unsafe static List<EntryPoint> FindAllEntryPoints(string jarfile, ReferenceType<float> progressReport = null)
        {
            int maxThreads = 4; //this shit deadlock idk why
            
            Thread[] runningThreads = new Thread[maxThreads];
            List<ZipArchiveEntry>[] workload = new List<ZipArchiveEntry>[maxThreads];
            int writingTo = 0;
            for (int x = 0; x != workload.Length; x++)
            {
                workload[x] = new List<ZipArchiveEntry>();
            }

            List<EntryPoint> ret = new List<EntryPoint>();

            int validClassCount = 0;
            int doneClassCount = 0;

            using (ZipArchive archive = ZipFile.OpenRead(jarfile))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.Contains('/') && entry.Name.EndsWith(".class") && !entry.Name.Contains('$'))
                    {
                        workload[writingTo].Add(entry);
                        writingTo++;
                        writingTo %= maxThreads;
                        validClassCount++;
                    }
                }
            }

            Mutex retListMutex = new Mutex();
            for (int x = 0; x != maxThreads; x++)
            {
                EPFinderThread_args a = new EPFinderThread_args(jarfile, workload[x], ret, retListMutex, x, &doneClassCount);
                runningThreads[x] = new Thread(Thread_EntryPointFinder);

                runningThreads[x].Start(a);
                //Thread_EntryPointFinder(a);
            }
            for (int x = 0; x != maxThreads; x++)
            {
                while (runningThreads[x].IsAlive)
                {
                    if (progressReport != null)
                    {
                        if (doneClassCount != 0)
                        {
                            progressReport.Set(((float)doneClassCount) / validClassCount);
                            //Console.WriteLine($"Progress: {progressReport.Get()}");
                        } else
                        {
                            progressReport.Set(0);
                        }
                    }
                    Thread.Sleep(100);
                }
            }
            Console.WriteLine($"All scanner threads finished, {doneClassCount}/{validClassCount}");
            return ret;
        }
    }
}
