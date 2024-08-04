using DME.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DECRAFTModdingEnvironment
{
    public class WorkspaceConfig
    {
        public string jdkPath = "";
        public string workspaceName = "New DME Workspace";
        public string versionName = "";
        public string launchEntryPoint = "net.minecraft.client.Minecraft";
        public string gameArgs = "";
        public string jvmArgs = "";
        public int preferredJDKVersion = 7;

        public Dictionary<string, string> originalMD5s = new Dictionary<string, string>();
        public string[] passthroughExtensions = { 
            ".txt", ".png", ".jpg", ".jpeg", ".gif", ".json"
        };

        //folder structure:
        //  DME.exe <- this exe
        //  _dme_config/_dme_workspace.xml <- workspace config (this class)
        //  _dme_config/bin/remapper.jar <- fabric remapper
        //  _dme_config/bin/fernflower.jar <- fernflower decompiler
        //  _dme_config/bin/procyon.jar <- procyon decompiler
        //  _dme_config/bin/lib/base.jar <- base game jar
        //  _dme_config/bin/lib/*.jar <- other game lib jars
        //  _dme_config/bin/lib/native/*.dll <- native libraries
        //  _dme_config/class_md5s.txt <- md5s of all original java source files
        //  _dme_config/obf_to_deobf.tinyv2 <- base jar mappings
        //  _dme_config/deobf_to_obf.tinyv2 <- reverse base jar mappings
        //  _dme_config/source_backup.zip <- zip with all original java source files
        //  build/build_modified.zip <- zip with only modified classes
        //  build/build_full.jar <- jar with all classes
        //  build/classes/ <- built modified classes
        //  src/ <- (modifiable) java source files


        public static string NodeOrDefault(XmlNode node, string name, string def = "") 
        {
            return node.SelectSingleNode(name)?.InnerText ?? def;
        }

        public static WorkspaceConfig LoadFromXML(string path)
        {
            WorkspaceConfig config = new WorkspaceConfig();

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNode root = xdoc.DocumentElement;
            config.jdkPath =            NodeOrDefault(root, "JDKPath");
            config.workspaceName =      NodeOrDefault(root, "WorkspaceName", "New DME Workspace");
            config.versionName =        NodeOrDefault(root, "VersionName");
            config.launchEntryPoint =   NodeOrDefault(root, "LaunchEntryPoint", "net.minecraft.client.Minecraft");
            config.gameArgs =           NodeOrDefault(root, "GameArgs");
            config.jvmArgs =            NodeOrDefault(root, "JVMArgs");

            foreach (string md5pair in File.ReadAllLines("./_dme_config/class_md5s.txt"))
            {
                config.originalMD5s.Add(md5pair.Split('+')[0], md5pair.Split('+')[1]);
            }

            return config;
        }

        public void SaveToXML() {             
            XmlDocument xdoc = new XmlDocument();
            XmlNode root = xdoc.CreateElement("WorkspaceConfig");
            xdoc.AppendChild(root);
        
            XmlNode jdkPathNode = xdoc.CreateElement("JDKPath");
            jdkPathNode.InnerText = jdkPath;
            root.AppendChild(jdkPathNode);
        
            XmlNode workspaceNameNode = xdoc.CreateElement("WorkspaceName");
            workspaceNameNode.InnerText = workspaceName;
            root.AppendChild(workspaceNameNode);
        
            XmlNode versionNameNode = xdoc.CreateElement("VersionName");
            versionNameNode.InnerText = versionName;
            root.AppendChild(versionNameNode);

            XmlNode launchEntryPointNode = xdoc.CreateElement("LaunchEntryPoint");
            launchEntryPointNode.InnerText = launchEntryPoint;
            root.AppendChild(launchEntryPointNode);

            XmlNode gameArgsNode = xdoc.CreateElement("GameArgs");
            gameArgsNode.InnerText = gameArgs;
            root.AppendChild(gameArgsNode);

            XmlNode jvmArgsNode = xdoc.CreateElement("JVMArgs");
            jvmArgsNode.InnerText = jvmArgs;
            root.AppendChild(jvmArgsNode);
        
            xdoc.Save("./_dme_config/_dme_workspace.xml");
        }

        public void RunDecomp()
        {
            Directory.CreateDirectory("./src");
            //todo: remap jar

            if (File.Exists("./_dme_config/obf_to_deobf.tinyv2"))
            {
                string remapperLaunchString = $"-jar {"./_dme_config/bin/remapper.jar"} ./_dme_config/bin/lib/base.jar ./_dme_config/bin/lib/base_deobf.jar ./_dme_config/obf_to_deobf.tinyv2 official named";
                Process remapperProc = JavaExec.RunProcess(jdkPath == "" ? "java.exe" : (jdkPath + "/java.exe"), remapperLaunchString, null);
                WindowProcessLog remapperLog = new WindowProcessLog(remapperProc, true, true);
                remapperLog.ShowDialog();
                File.Delete("./_dme_config/bin/lib/base.jar");
                //File.Move("./_dme_config/bin/lib/base.jar", "./_dme_config/bin/lib/base_obf.jar");
                File.Move("./_dme_config/bin/lib/base_deobf.jar", "./_dme_config/bin/lib/base.jar");
            }

            string procyonLaunchString = $"-jar {"./_dme_config/bin/procyon.jar"} ./_dme_config/bin/lib/base.jar -o ./src";
            Process procyonProc = JavaExec.RunProcess(jdkPath == "" ? "java.exe" : (jdkPath + "/java.exe"), procyonLaunchString, null);
            WindowProcessLog log = new WindowProcessLog(procyonProc, true, true);
            log.ShowDialog();
            int procyonExitCode = procyonProc.ExitCode;
            if (procyonExitCode != 0)
            {
                throw new Exception("Procyon decompilation failed");
            }
            //open base.jar as zip and extract all other resource files to /src
            originalMD5s = new Dictionary<string, string>();
            ZipArchive baseJar = ZipFile.OpenRead("./_dme_config/bin/lib/base.jar");

            foreach (ZipArchiveEntry entry in baseJar.Entries.Where((x)=> !x.Name.EndsWith(".class")))
            {
                if (entry.Length == 0)
                {
                    continue;
                }
                Console.WriteLine($"Unpack: {entry.FullName}");
                Directory.CreateDirectory(Path.GetDirectoryName($"./src/{entry.FullName}"));
                if (File.Exists($"./src/{entry.FullName}"))
                {
                    File.Delete($"./src/{entry.FullName}");
                }
                entry.ExtractToFile($"./src/{entry.FullName}");
                FileStream file = File.OpenRead($"./src/{entry.FullName}");
                file.Close();
            }
            baseJar.Dispose();

            MD5 md5 = MD5.Create();
            foreach (string file in Directory.EnumerateFiles("./src", "*", SearchOption.AllDirectories))
            {
                Console.WriteLine($"Hashing: {file}");
                FileStream fileStream = File.OpenRead(file);
                byte[] md5Bytes = md5.ComputeHash(fileStream);
                fileStream.Close();
                originalMD5s.Add(file, BitConverter.ToString(md5Bytes).Replace("-", ""));
            }
            md5.Dispose();

            File.WriteAllText("./_dme_config/class_md5s.txt", String.Join("\n", originalMD5s.Select((x) => $"{x.Key}+{x.Value}")));
        }

        public bool RunRecomp(bool force = false)
        {

            IEnumerable<string> recompFiles = originalMD5s.Where((x) =>
            {
                if (!File.Exists(x.Key))
                {
                    return false;
                }
                FileStream file = File.OpenRead(x.Key);
                byte[] md5Bytes = MD5.Create().ComputeHash(file);
                file.Close();
                return BitConverter.ToString(md5Bytes).Replace("-", "") != x.Value;
            }).Select((x) => x.Key);
            
            Directory.CreateDirectory("./build");
            Directory.CreateDirectory("./build/classes");

            if (force
                || recompFiles.Any()
                || Directory.EnumerateFiles("./src", "*", SearchOption.AllDirectories).Count() != originalMD5s.Count)
            {
                var classRecompFiles = recompFiles.Where(x => x.EndsWith(".java"));
                if (classRecompFiles.Any()) { 
                    File.WriteAllLines("./_dme_config/build_classes.txt", classRecompFiles);

                    var classPaths = Directory.EnumerateFiles("./_dme_config/bin/lib", "*.jar").Select((x) => x.Replace("\\", "/"));
                    string javacLaunchString = $"-cp {String.Join(";", classPaths)} -d ./build/classes @./_dme_config/build_classes.txt";
                    Process javacProc = JavaExec.RunProcess(jdkPath == "" ? "javac.exe" : (jdkPath + "/javac.exe"), javacLaunchString);
                    WindowProcessLog log = new WindowProcessLog(javacProc, false, true);
                    log.ShowDialog();

                    int javacExitCode = javacProc.ExitCode;
                    if (javacExitCode != 0)
                    {
                        MessageBox.Show("The build failed. Fix the errors in your code and try again.", "Compilation failed");
                        return false;
                        //throw new Exception("Javac compilation failed");
                    }
                }

                //copy all non-java files from src to build/classes
                foreach (string file in Directory.EnumerateFiles("./src", "*", SearchOption.AllDirectories).Where((x) => !x.EndsWith(".java")))
                {
                    string relativePath = file.Substring(5);
                    Directory.CreateDirectory($"./build/classes/{Path.GetDirectoryName(relativePath)}");
                    if (File.Exists($"./build/classes/{relativePath}"))
                    {
                        File.Delete($"./build/classes/{relativePath}");
                    }
                    File.Copy(file, $"./build/classes/{relativePath}");
                }
            }
            return true;
        }

        public bool HalfBuild()
        {
            if (RunRecomp())
            {
                if (File.Exists("./build/build_modified.zip"))
                {
                    File.Delete("./build/build_modified.zip");
                }
                ZipFile.CreateFromDirectory("./build/classes", "./build/build_modified.zip");
                return true;
            }
            return false;
        }

        public bool FullBuild()
        {
            if (HalfBuild())
            {
                if (File.Exists("./build/build_full.jar"))
                {
                    File.Delete("./build/build_full.jar");
                }
                File.Copy("./_dme_config/bin/lib/base.jar", "./build/build_full.jar");

                //merge build_modified.zip into build_full.jar
                using (ZipArchive fullJar = ZipFile.Open("./build/build_full.jar", ZipArchiveMode.Update))
                {
                    //delete META-INF and all of its contents
                    foreach (ZipArchiveEntry entry in fullJar.Entries.Where((x) => x.FullName.StartsWith("META-INF")))
                    {
                        entry.Delete();
                    }

                    using (ZipArchive modifiedZip = ZipFile.OpenRead("./build/build_modified.zip"))
                    {
                        foreach (ZipArchiveEntry entry in modifiedZip.Entries)
                        {
                            string arcEntryName = entry.FullName.Replace('\\', '/');
                            ZipArchiveEntry existingEntry = fullJar.GetEntry(arcEntryName);
                            if (existingEntry != null)
                            {
                                Console.WriteLine($"Replacing {entry.FullName}");
                                existingEntry.Delete();
                            }
                            //Console.WriteLine($"Adding entry {entry.FullName}");
                            ZipArchiveEntry newEntry = fullJar.CreateEntry(arcEntryName);
                            using (Stream newEntryStream = newEntry.Open())
                            {
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.CopyTo(newEntryStream);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void RunBuild()
        {
            var classPaths = Directory.EnumerateFiles("./_dme_config/bin/lib", "*.jar").Where(x => !x.EndsWith("base.jar")).Select((x) => x.Replace("\\", "/"))
                .Concat(new string[]{ "./build/build_full.jar"});
            string javaArgs = $"-cp {String.Join(";", classPaths)} -Djava.library.path={"./_dme_config/bin/lib/native"} {jvmArgs} {launchEntryPoint} {gameArgs}";     //TODO!!!!
            Directory.CreateDirectory("./gamedir");
            Process javaProc = JavaExec.RunProcess(jdkPath == "" ? "java.exe" : (jdkPath + "/java.exe"), javaArgs, "./gamedir");
            WindowProcessLog log = new WindowProcessLog(javaProc, false);
            log.Show();
        }

        public void FullBuildAndRun()
        {
            if (FullBuild())
            {
                RunBuild();
            }
        }
    }
}
