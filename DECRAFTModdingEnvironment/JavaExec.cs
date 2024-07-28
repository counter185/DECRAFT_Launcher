using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DECRAFTModdingEnvironment;

namespace DME.Utils
{
    public class JavaExec
    {
        public List<string> jvmArgs = new List<string>();
        public List<string> classPath = new List<string>();
        public string className;
        public List<string> programArgs = new List<string>();

        public string workingDirectory = null;
        public string appdataDir = "";

        public string execName = "java";

        public JavaExec(string className) 
        { 
            this.className = className;
        }

        public string GetFullArgsString()
        {
            string classPaths = classPath.Count != 0 ? $"-cp \"{String.Join(";", classPath)}\"" : "";

            // do not use Append here or it will be incompatible with .net 4.5.2
            return String.Join(" ", new List<string>()
                .Concat(new string[] { classPaths })
                .Concat(jvmArgs)
                .Concat(new string[] { className })
                .Concat(programArgs)
            );
        }

        public Process Start(string javaPath = null, Action<List<string>> callback = null)
        {
            return StartWithCustomArgsString(GetFullArgsString(), javaPath, callback);
        }

        public Process StartWithCustomArgsString(string argsString, string javaPath = null, Action<List<string>> callback = null)
        {
            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }

            try
            {
                return RunProcess(javaPath, argsString, appdataDir, callback);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                if (workingDirectory != null)
                {
                    Directory.SetCurrentDirectory(MainWindow.currentDirectory);
                }
            }
        }

        public static Process RunProcess(string program, string args, string appdataDir = "", Action<List<string>> callback = null)
        {
            Console.WriteLine($"[RunProcess] {program} {args}");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = program,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
            if (appdataDir != "")
            {
                startInfo.EnvironmentVariables["appdata"] = appdataDir;
                //startInfo.EnvironmentVariables["cd"] = appdataDir + "\\.minecraft";
            }

            Process proc = new Process
            {
                StartInfo = startInfo,
            };
            if (callback != null)
            {
                proc.Start();
                proc.WaitForExit();
                List<string> stdout = new List<string>();
                while (!proc.StandardOutput.EndOfStream)
                {
                    stdout.Add(proc.StandardOutput.ReadLine());
                }
                while (!proc.StandardError.EndOfStream)
                {
                    stdout.Add(proc.StandardError.ReadLine());
                }
                callback(stdout);
            }
            else
            {
                proc.Start();
            }
            return proc;
        }
    }
}
