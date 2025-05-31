using DeCraftLauncher.Configs;
using DeCraftLauncher.UIControls.Popup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using static DeCraftLauncher.Utils.JarUtils;
using static DeCraftLauncher.Utils.JavaClassReader;
using Brushes = System.Windows.Media.Brushes;
using Path = System.IO.Path;

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
            List<string> stdout = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                stdout.Add(proc.StandardOutput.ReadLine());
            }
            while (!proc.StandardError.EndOfStream)
            {
                stdout.Add(proc.StandardError.ReadLine());
            }
            if (proc.ExitCode != 0 && throwOnNZEC)
            {
#if DEBUG
                PopupOK.ShowNewPopup(String.Join("\n", stdout), "DECRAFT Debug");
#endif
                throw new ApplicationException("Non-zero exit code");
            }
            return stdout;
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
            if (appdataDir != "") {
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

        public struct JavaFinderResult
        {
            public string version;
            public string path;
            public string implementor;
            public string arch;
            public bool hasJDK;

            public JavaFinderResult(string path)
            {
                this.version = "???";
                this.path = path;
                this.implementor = "---";
                this.arch = "(unk.)";
                this.hasJDK = false;
            }

            public JavaFinderResult UpdateFromReleaseFile(string fileAt)
            {
                if (File.Exists(fileAt))
                {
                    string[] lines = File.ReadAllLines(fileAt);
                    foreach (string line in lines)
                    {
                        string[] splt = line.Split('=');
                        try
                        {
                            switch (splt[0])
                            {
                                case "JAVA_VERSION":
                                    this.version = splt[1].Replace("\"", "");
                                    break;
                                case "IMPLEMENTOR":
                                    this.implementor = splt[1].Replace("\"", "");
                                    break;
                                case "OS_ARCH":
                                    this.arch = splt[1].Replace("\"", "");
                                    break;
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                    }
                }
                return this;
            }
            
            public JavaFinderResult UpdateFromJavaExecutableIfNoArch(string fileAt)
            {
                return this.arch == "(unk.)" ? UpdateFromJavaExecutable(fileAt) : this;
            }

            public JavaFinderResult UpdateFromJavaExecutable(string fileAt)
            {
                if (File.Exists(fileAt))
                {
                    try
                    {
                        using (FileStream pExe = File.OpenRead(fileAt))
                        {
                            pExe.Seek(0x3C, SeekOrigin.Begin);
                            int PEHeaderStart = Utils.Util.StreamReadInt(pExe, false);
                            pExe.Seek(PEHeaderStart + 4, SeekOrigin.Begin);
                            ushort platformID = (ushort)Utils.Util.StreamReadShort(pExe, false);
                            this.arch =
                                platformID == 0x8664 ? "AMD64"
                                : platformID == 0x14c ? "I386"
                                : platformID == 0x200 ? "IA-64"
                                : platformID == 0xaa64 ? "ARM64"
                                : this.arch;
                        }
                    } catch (Exception)
                    {
                        //a
                    }
                }
                return this;
            }

            public JavaFinderResult SetHasJDK(bool value)
            {
                hasJDK = value;
                return this;
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
                     "BellSoft",    //Liberica
                     "Semeru",      //IBM
                     "Microsoft"
                 };
                 foreach (string vendorPath in vendorPaths)
                 {
                     potentialPaths.Add(driveLabel + "Program Files/" + vendorPath + "/");
                     potentialPaths.Add(driveLabel + "Program Files (x86)/" + vendorPath + "/");
                     potentialPaths.Add(driveLabel + "Program Files (Arm)/" + vendorPath + "/");
                 }
             });

            string envJAVA_HOME = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (envJAVA_HOME != null) {
                envJAVA_HOME = envJAVA_HOME.Replace('\\', '/');
                potentialPaths.Add(envJAVA_HOME + (envJAVA_HOME.EndsWith("/") ? "" : "/"));
            }

            string envLOCALAPPDATA = Environment.GetEnvironmentVariable("localappdata");
            if (envLOCALAPPDATA != null)
            {
                envLOCALAPPDATA = envLOCALAPPDATA.Replace('\\', '/');
                //lmao if this works
                potentialPaths.Add($"{envLOCALAPPDATA}/Packages/Microsoft.4297127D64EC6_8wekyb3d8bbwe/LocalCache/Local/runtime/jre-legacy/windows-x64/");
            }
            
            string envUSERPROFILE = Environment.GetEnvironmentVariable("userprofile");
            if (envUSERPROFILE != null)
            {
                //double lmao if this works
                string rhSearchPath = $"{envUSERPROFILE}/.vscode/extensions";
                if (Directory.Exists(rhSearchPath))
                {
                    (from x in Directory.GetDirectories(rhSearchPath, "redhat.java*")
                     select x + "/jre/").ToList().ForEach((x) => { potentialPaths.Add(x.Replace('\\', '/')); });
                }

                string ideaDefaultJDKPath = $"{envUSERPROFILE}/.jdks/";
                if (Directory.Exists(ideaDefaultJDKPath))
                {
                    (from x in Directory.GetDirectories(ideaDefaultJDKPath)
                     select x + "/").ToList().ForEach((x) => { potentialPaths.Add(x.Replace('\\', '/')); });
                }
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
                 select new JavaFinderResult(njavapath + "bin/")
                                .SetHasJDK(File.Exists(njavapath + "bin/javac.exe"))
                                .UpdateFromReleaseFile(njavapath + "release")
                                .UpdateFromJavaExecutableIfNoArch(njavapath + "bin/java.exe"))
                 .ToList().ForEach((a) => results.Add(a));

            }
            return (from x in results
                    group x by x.path into y
                    select y.First()).ToList();
        }

        public enum EntryPointType
        {
            RUNNABLE = 0,   //obsolete lmao
            APPLET = 1,
            STATIC_VOID_MAIN = 2,
            CUSTOM_LAUNCH_COMMAND = 3
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

            public string GetDescription()
            {
                switch (classpath)
                {
                    case "com.mojang.rubydung.RubyDung":
                    case "com.mojang.minecraft.RubyDung":
                    case "com.mojang.minecraft.Minecraft":
                    case "net.minecraft.client.Minecraft":
                    case "finalforeach.cosmicreach.lwjgl3.Lwjgl3Launcher":
                        return GlobalVars.L.Translate("ui.entrypoint.description.direct_launch");
                    case "com.mojang.minecraft.MinecraftApplet":
                    case "net.minecraft.client.MinecraftApplet":
                        return GlobalVars.L.Translate("ui.entrypoint.description.applet_launch");
                    case "com.mojang.minecraft.server.MinecraftServer":
                    case "net.minecraft.server.MinecraftServer":
                        return GlobalVars.L.Translate("ui.entrypoint.description.server_launch");
                    case "net.minecraft.isom.IsomPreviewApplet":
                        return GlobalVars.L.Translate("ui.entrypoint.description.isom_applet_launch");
                    case "Start":
                        return GlobalVars.L.Translate("ui.entrypoint.description.mcp_launch");
                    case "net.minecraft.client.main.Main":
                    case "net.minecraft.data.Main":
                        return GlobalVars.L.Translate("ui.entrypoint.description.unsupported_onesix_launch");
                    case "decraft_internal.AppletWrapper":
                    case "decraft_internal.MainFunctionWrapper":
                    case "decraft_internal.LWJGLTestGPU":
                        return GlobalVars.L.Translate("ui.entrypoint.description.ee_decraftinternal_launch");
                    case "com.megacrit.cardcrawl.desktop.DesktopLauncher":
                        return GlobalVars.L.Translate("ui.entrypoint.description.ee_slaythespire_launch");
                    default:
                        return classpath.StartsWith("com.jdotsoft.jarloader") ? GlobalVars.L.Translate("ui.entrypoint.description.jdotsoft_loader_launch")
                               : GlobalVars.L.Translate("ui.entrypoint.description.unknown");
                }
            }

            public System.Windows.Media.Brush GetUIColor()
            {
                switch (classpath)
                {
                    case "com.mojang.rubydung.RubyDung":
                    case "com.mojang.minecraft.RubyDung":
                    case "com.mojang.minecraft.Minecraft":
                    case "net.minecraft.client.Minecraft":
                        return Brushes.LightGreen;
                    case "com.mojang.minecraft.MinecraftApplet":
                    case "net.minecraft.client.MinecraftApplet":
                        return Brushes.LimeGreen;
                    case "com.mojang.minecraft.server.MinecraftServer":
                    case "net.minecraft.server.MinecraftServer":
                        return Brushes.LightSkyBlue;
                    case "finalforeach.cosmicreach.lwjgl3.Lwjgl3Launcher":
                    case "com.megacrit.cardcrawl.desktop.DesktopLauncher":
                        return Brushes.Goldenrod;

                }
                return null;
            }

            public int GetImportance()
            {
                switch (classpath)
                {
                    case "com.mojang.rubydung.RubyDung":
                    case "com.mojang.minecraft.RubyDung":
                    case "com.mojang.minecraft.Minecraft":
                    case "net.minecraft.client.Minecraft":
                    case "finalforeach.cosmicreach.lwjgl3.Lwjgl3Launcher":
                        return 4;
                    case "com.mojang.minecraft.MinecraftApplet":
                    case "net.minecraft.client.MinecraftApplet":
                        return 3;
                    case "com.mojang.minecraft.server.MinecraftServer":
                    case "net.minecraft.server.MinecraftServer":
                        return 2;
                    case "net.minecraft.isom.IsomPreviewApplet":
                    case "decraft_internal.AppletWrapper":
                    case "decraft_internal.MainFunctionWrapper":
                    case "decraft_internal.LWJGLTestGPU":
                    case "Start":
                        return 1;
                    default:
                        return classpath.StartsWith("com.jdotsoft.jarloader") ? 5
                               : -1;
                }
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

            public bool hasMissingSynthetics = false;

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
                        dllPath = dllPath.Substring(0, Math.Max(0, dllPath.LastIndexOf('/')));
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

                        if (!ret.hasMissingSynthetics)
                        {
                            foreach (ConstantPoolEntry.ClassReferenceEntry x in (from y in classInfo.entries
                                                                                 where y is ConstantPoolEntry.ClassReferenceEntry
                                                                                 select (ConstantPoolEntry.ClassReferenceEntry)y))
                            {
                                string referencedClassName = x.GetName(classInfo.entries);
                                if (referencedClassName.StartsWith("net/minecraft/client/Minecraft$SyntheticClass")
                                    && !(from y in archive.Entries
                                         where y.FullName.StartsWith(referencedClassName)
                                         select y).Any())
                                {
                                    ret.hasMissingSynthetics = true;
                                    break;
                                }
                            }
                        }

                        if ((superClassName == "java.applet.Applet" 
                            || superClassName == "javax.swing.JApplet")
                            && (from y in classInfo.methods
                                where y.Name(classInfo.entries) == "<init>"
                                    && y.Descriptor(classInfo.entries) == "()V"
                                select y).Any())
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
                                    else if (stringEntry.value.StartsWith("DECRAFT Applet Wrapper: "))
                                    {
                                        newEntryPoint.additionalInfo = "-> " + stringEntry.value.Substring("DECRAFT Applet Wrapper: ".Length);
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

        public static void ExtractBuiltInLWJGLToTempFolder(JarConfig jarConfig)
        {
            MainWindow.EnsureDir($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin");
            MainWindow.EnsureDir($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native");
            ZipArchive zip = ZipFile.OpenRead(Path.GetFullPath(MainWindow.jarDir + "/" + jarConfig.jarFileName));
            var dllFilesToExtract = (from x in zip.Entries where x.FullName.StartsWith($"{jarConfig.jarBuiltInLWJGLDLLs}") && x.Name.EndsWith(".dll") select x);
            DirectoryInfo nativesdir = new DirectoryInfo($"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native");
            foreach (FileInfo f in nativesdir.EnumerateFiles())
            {
                f.Delete();
            }

            foreach (ZipArchiveEntry dllFile in dllFilesToExtract)
            {
                string targetPath = $"{MainWindow.currentDirectory}/lwjgl/_temp_builtin/native/{dllFile.Name}";
                if (!File.Exists(targetPath))   //duplicate dll workaround
                {
                    dllFile.ExtractToFile(targetPath);
                }
            }
            Console.WriteLine("Extracted temp LWJGL natives");
        }
    }
}
