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
using System.Windows.Shapes;
using static DeCraftLauncher.Utils.JavaClassReader;

namespace DeCraftLauncher.Utils
{
    public unsafe class JarUtils
    {
        public static List<string> RunProcessAndGetOutput(string program, string args, bool throwOnNZEC = false) {
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
            proc.WaitForExit();
            //proc.WaitForExit(75);
            if (proc.ExitCode != 0 && throwOnNZEC)
            {
                throw new ApplicationException("Non-zero exit code");
            }
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

        public static bool HasJREInstalled(string at)
        {
            return File.Exists(at + "java.exe");
        }        
        public static bool HasJDKInstalled(string at)
        {
            return at != "" ? File.Exists(at + "javac.exe") : GetJDKInstalled(at) != null;
        }

        public static string GetJavaVersionFromReleaseFile(string fileAt)
        {
            if (File.Exists(fileAt))
            {
                string[] lines = File.ReadAllLines(fileAt);
                foreach (string line in lines)
                {
                    string[] splt = line.Split('=');
                    try
                    {
                        if (splt[0] == "JAVA_VERSION")
                        {
                            return splt[1].Replace("\"", "");
                        }
                    }
                    catch (IndexOutOfRangeException) { }
                }
            }
            return null;
        }

        public struct JavaFinderResult
        {
            public string version;
            public string path;

            public JavaFinderResult(string version, string path)
            {
                this.version = version;
                this.path = path;
            }
        }

        public static List<JavaFinderResult> FindAllJavaInstallations()
        {
            HashSet<string> potentialPaths = new HashSet<string>();
            DriveInfo[] drives = DriveInfo.GetDrives();
            (from drive in drives
             select drive.Name.Replace('\\', '/')).ToList().ForEach((driveLabel) => {
                 //please add
                 string[] vendorPaths = new string[]
                 {
                     "Java",
                     "Zulu",
                     "Eclipse Adoptium",
                     "AdoptOpenJDK",
                     "Android/Android Studio",
                     "Microsoft"
                 };
                 foreach (string vendorPath in vendorPaths)
                 {
                     potentialPaths.Add(driveLabel + "Program Files/" + vendorPath + "/");
                     potentialPaths.Add(driveLabel + "Program Files (x86)/" + vendorPath + "/");
                 }
             });

            string envJAVA_HOME = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (envJAVA_HOME != null) {
                envJAVA_HOME = envJAVA_HOME.Replace('\\', '/');
                potentialPaths.Add(envJAVA_HOME + (envJAVA_HOME.EndsWith("/") ? "" : "/"));
            }

            List<JavaFinderResult> results = new List<JavaFinderResult>();
            foreach (string ppath in potentialPaths)
            {
                List<string> potentialPotentialPaths = new List<string>();
                potentialPotentialPaths.Add(ppath);
                if (Directory.Exists(ppath))
                {
                    (from subdir in Directory.GetDirectories(ppath)
                     select subdir + "/").ToList().ForEach((nsubdir) => { potentialPotentialPaths.Add(nsubdir); });
                }

                (from njavapath in potentialPotentialPaths
                 where File.Exists(njavapath + "bin/java.exe")
                 select new JavaFinderResult((File.Exists(njavapath + "release") ? GetJavaVersionFromReleaseFile(njavapath + "release") : "???"), (njavapath + "bin/")))
                 .ToList().ForEach((a) => results.Add(a));

            }
            return (from x in results 
                    group x by x.path into y
                    select y.First()).ToList();
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
            public string additionalInfo;

            public EntryPoint(string classpath, EntryPointType type, string additionalInfo = "")
            {
                this.classpath = classpath;
                this.type = type;
                this.additionalInfo = additionalInfo;
            }
        }

        public class EntryPointScanResults
        {
            public int minMajorVersion = -1;
            public int minMinorVersion = -1;

            public int maxMajorVersion = -1;
            public int maxMinorVersion = -1;

            public bool hasLWJGLBuiltIn = false;
            public string lwjglNativesDir = "";

            public List<EntryPoint> entryPoints = new List<EntryPoint>();
            public List<string> modsFound = new List<string>();
        }

        [Obsolete("related to the old entry point finder.")]
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

        [Obsolete("much, much slower method of doing things. this is only kept in case there's something wrong with the classreader.")]
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

        public unsafe static EntryPointScanResults FindAllEntryPoints(string jarfile, ReferenceType<float> progressReport = null)
        {
            EntryPointScanResults ret = new EntryPointScanResults();

            int validClassCount = 0;
            int doneClassCount = 0;

            using (ZipArchive archive = ZipFile.OpenRead(jarfile))
            {
                //we look through all classes that also aren't synthetic classes
                IEnumerable<ZipArchiveEntry> classesToScan = from x in archive.Entries 
                                                             where x.Name.EndsWith(".class") && !x.Name.Contains('$') 
                                                             select x;

                validClassCount = classesToScan.Count();

                ret.hasLWJGLBuiltIn = archive.Entries.Where((zipEntry) => zipEntry.FullName.StartsWith("org/lwjgl/")).Any();
                if (ret.hasLWJGLBuiltIn)
                {
                    var lwjglDlls = (from x in archive.Entries
                                     where x.Name == "lwjgl.dll" || x.Name == "lwjgl64.dll"
                                     select x.FullName);
                    if (lwjglDlls.Count() > 0)
                    {
                        string dllPath = lwjglDlls.First();
                        dllPath = dllPath.Substring(0, dllPath.LastIndexOf('/'));
                        ret.lwjglNativesDir = dllPath;
                    } else
                    {
                        ret.hasLWJGLBuiltIn = false;
                    }
                }

                ret.modsFound = (from x in archive.Entries
                                 where x.FullName.EndsWith(".class") && 
                                    (x.FullName.StartsWith("mod_")
                                    || x.FullName == "ModLoader.class")
                                 select x.FullName.Substring(0, x.FullName.Length - 6)).ToList();

                bool firstClassEntry = true;

                foreach (ZipArchiveEntry entry in classesToScan)
                {
                    try
                    {
                        Stream currentClassFile = entry.Open();
                        JavaClassInfo classInfo = ReadJavaClassFromStream(currentClassFile);
                        string className = ((ConstantPoolEntry.ClassReferenceEntry)classInfo.entries[classInfo.thisClassNameIndex]).GetName(classInfo.entries).Replace('/', '.');
                        string superClassName = ((ConstantPoolEntry.ClassReferenceEntry)classInfo.entries[classInfo.superClassNameIndex]).GetName(classInfo.entries).Replace('/', '.');
                        if (firstClassEntry 
                            || (classInfo.versionMajor > ret.maxMajorVersion)
                            || (classInfo.versionMajor == ret.maxMajorVersion && classInfo.versionMinor > ret.maxMinorVersion))
                        {
                            ret.maxMajorVersion = classInfo.versionMajor;
                            ret.maxMinorVersion = classInfo.versionMinor;
                        }
                        if (firstClassEntry
                            || (classInfo.versionMajor < ret.minMajorVersion)
                            || (classInfo.versionMajor == ret.minMajorVersion && classInfo.versionMinor < ret.minMinorVersion))
                        {
                            ret.minMajorVersion = classInfo.versionMajor;
                            ret.minMinorVersion = classInfo.versionMinor;
                        }
                        firstClassEntry = false;


                        if (superClassName == "java.applet.Applet")
                        {
                            ret.entryPoints.Add(new EntryPoint(className, EntryPointType.APPLET));
                        }
                        else
                        {
                            foreach (JavaMethodInfo method in classInfo.methods.Where((x) =>
                                                                x.IsPublic
                                                                && x.IsStatic
                                                                && x.Name(classInfo.entries) == "main"
                                                                && x.Descriptor(classInfo.entries) == "([Ljava/lang/String;)V")
                            )
                            {
                                EntryPoint newEntryPoint = new EntryPoint(className, EntryPointType.STATIC_VOID_MAIN);

                                //try finding the version name...
                                foreach (ConstantPoolEntry.StringEntry stringEntry in (from x in classInfo.entries 
                                                                                        where x is ConstantPoolEntry.StringEntry 
                                                                                        select (ConstantPoolEntry.StringEntry)x))
                                {
                                    if (stringEntry.value.StartsWith("Minecraft Minecraft "))
                                    {
                                        newEntryPoint.additionalInfo = stringEntry.value.Substring("Minecraft Minecraft ".Length);
                                        break;
                                    }
                                    else if (stringEntry.value.ToLower().StartsWith("starting minecraft server version "))
                                    {
                                        newEntryPoint.additionalInfo = stringEntry.value.Substring("starting minecraft server version ".Length);
                                        break;
                                    }
                                    else if (stringEntry.value.StartsWith($"M\xCC\xB6i\xCC\xB6n\xCC{'\xB6'}e\xCC{'\xB6'}c\xCC\xB6r\xCC{'\xB6'}a\xCC{'\xB6'}f\xCC\xB6t\xCC\xB6 "))
                                    {
                                        //ERR422 thinks it's really funny apparently...
                                        newEntryPoint.additionalInfo = stringEntry.value.Substring($"M\xCC\xB6i\xCC\xB6n\xCC{'\xB6'}e\xCC{'\xB6'}c\xCC\xB6r\xCC{'\xB6'}a\xCC{'\xB6'}f\xCC\xB6t\xCC\xB6 ".Length);
                                        break;
                                    }
                                    else if (stringEntry.value.StartsWith("Minecraft ") && !className.EndsWith("Server") && stringEntry.value != "Minecraft main thread")
                                    {
                                        newEntryPoint.additionalInfo = stringEntry.value.Substring("Minecraft ".Length);
                                    }
                                }

                                ret.entryPoints.Add(newEntryPoint);
                                break;
                            }
                        }
                        currentClassFile.Close();
                        doneClassCount++;
                        if (progressReport != null)
                        {
                            progressReport.Value = doneClassCount / (float)validClassCount;
                        }
                    } catch (Exception e)
                    {
                        Console.WriteLine("error reading class " + e);
                    }
                }
            }
            return ret;
        }
    }
}
