using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher.Utils
{
    public class JavaExec
    {
        public List<string> jvmArgs = new List<string>();
        public List<string> classPath = new List<string>();
        public string className;
        public List<string> programArgs = new List<string>();

        public string workingDirectory = null;
        public string appdataDir = "";

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

        public Process Start(string javaPath = null)
        {
            javaPath = javaPath ?? (MainWindow.mainRTConfig.javaHome + "java");

            string argsString = GetFullArgsString();

            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }
            Process ret = JarUtils.RunProcess(javaPath, argsString, appdataDir);
            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(MainWindow.currentDirectory);
            }

            return ret;
        }

        public Process StartWithCustomArgsString(string argsString, string javaPath = null)
        {
            javaPath = javaPath ?? (MainWindow.mainRTConfig.javaHome + "java");

            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(workingDirectory);
            }
            Process ret = JarUtils.RunProcess(javaPath, argsString, appdataDir);
            if (workingDirectory != null)
            {
                Directory.SetCurrentDirectory(MainWindow.currentDirectory);
            }

            return ret;
        }
    }
}
