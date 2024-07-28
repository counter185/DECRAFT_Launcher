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
using System.Xml;

namespace DECRAFTModdingEnvironment
{
    public class WorkspaceConfig
    {
        public string jdkPath;
        public string workspaceName = "New DME Workspace";
        public string versionName;
        public int preferredJDKVersion = 7;

        public Dictionary<string, string> originalMD5s;
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


        public static WorkspaceConfig LoadFromXML(string path)
        {
            WorkspaceConfig config = new WorkspaceConfig();

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNode root = xdoc.DocumentElement;
            config.jdkPath = root.SelectSingleNode("JDKPath").InnerText;
            config.workspaceName = root.SelectSingleNode("WorkspaceName").InnerText;
            config.versionName = root.SelectSingleNode("VersionName").InnerText;

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
        
            xdoc.Save("./_dme_config/_dme_workspace.xml");
        }

        public void RunDecomp()
        {
            Directory.CreateDirectory("./src");
            //todo: remap jar
            string procyonLaunchString = $"-jar {"./_dme_config/bin/procyon.jar"} ./_dme_config/bin/lib/base.jar -o ./src";
            Process procyonProc = JavaExec.RunProcess(jdkPath == "" ? "java.exe" : (jdkPath + "/java.exe"), procyonLaunchString, callback: (logLine) => { });
            int procyonExitCode = procyonProc.ExitCode;
            if (procyonExitCode != 0)
            {
                throw new Exception("Procyon decompilation failed");
            }
            //open base.jar as zip and extract all other resource files to /src
            Dictionary<string, string> originalMD5s = new Dictionary<string, string>();
            ZipArchive baseJar = ZipFile.OpenRead("./_dme_config/bin/lib/base.jar");

            foreach (ZipArchiveEntry entry in baseJar.Entries.Where((x)=>passthroughExtensions.Any((y) => x.FullName.EndsWith(y))))
            {
                Console.WriteLine($"Unpack: {entry.FullName}");
                entry.ExtractToFile($"./src/{entry.FullName}");
                FileStream file = File.OpenRead($"./src/{entry.FullName}");
                file.Close();
            }
            baseJar.Dispose();

            MD5 md5 = MD5.Create();
            foreach (string file in Directory.EnumerateFiles("./src", "*.java", SearchOption.AllDirectories))
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

        public void RunRecomp(bool force = false)
        {
            if (force 
                || Directory.EnumerateFiles("./src", "*.java", SearchOption.AllDirectories).Count() != originalMD5s.Count)
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
                }).Select((x)=>x.Key);

                File.WriteAllLines("./_dme_config/build_classes.txt", recompFiles);

                Directory.CreateDirectory("./build");
                Directory.CreateDirectory("./build/classes");
                string javacLaunchString = $"-cp ./_dme_config/bin/lib/*.jar -d ./build/classes @_dme_config/build_classes.txt";
                Process javacProc = JavaExec.RunProcess(jdkPath == "" ? "javac.exe" : (jdkPath + "/javac.exe"), javacLaunchString, callback: (logLine) => { });
                int javacExitCode = javacProc.ExitCode;
                if (javacExitCode != 0)
                {
                    throw new Exception("Javac compilation failed");
                }
            }
        }
    }
}
