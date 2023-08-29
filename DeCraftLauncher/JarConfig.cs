using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static DeCraftLauncher.JarUtils;

namespace DeCraftLauncher
{

    public class JarConfig
    {
        public string friendlyName = "";
        public string jarFileName;
        public List<EntryPoint> entryPoints = new List<EntryPoint>();
        public string LWJGLVersion = "2.9.3";
        public string playerName = "DECRAFT_Player";
        public bool entryPointsScanned = false;
        public string instanceDirName = "";
        public string jvmArgs = "-Djava.util.Arrays.useLegacyMergeSort=true";
        public uint windowW = 960;
        public uint windowH = 540;

        public JarConfig(string jarFileName)
        {
            this.jarFileName = jarFileName;
            this.instanceDirName = jarFileName;
        }

        public static string GetInnerOrDefault(XmlNode target, string property, string defaultValue = "", string enforceType = "string")
        {
            try
            {
                XmlNode node = target.SelectSingleNode(property);
                if (node == null)
                {
                    return defaultValue;
                }
                switch (enforceType)
                {
                    case "bool":
                        bool.Parse(node.InnerText);
                        break;
                    case "int":
                        int.Parse(node.InnerText);
                        break;
                    case "uint":
                        uint.Parse(node.InnerText);
                        break;
                }
                return node.InnerText;
            }
            catch (NullReferenceException)
            {
                return defaultValue;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }


        private XmlNode SetVal(XmlNode a, string v)
        {
            a.InnerText = v;
            return a;
        }

        public void SaveToXML(string path)
        {
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<?xml version=\"1.0\"?>\n<JarConfig>\n</JarConfig>");
            XmlNode rootElement = newXml.GetElementsByTagName("JarConfig")[0];
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "FriendlyName", ""), friendlyName));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "LWJGLVersion", ""), LWJGLVersion));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "PlayerName", ""), playerName));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "InstanceDirectory", ""), instanceDirName));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "JVMArgs", ""), jvmArgs));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "WindowW", ""), windowW+""));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "WindowH", ""), windowH+""));
            rootElement.AppendChild(SetVal(newXml.CreateNode("element", "EntryPointsScanned", ""), entryPointsScanned.ToString()));

            XmlNode entryPointsList = rootElement.AppendChild(newXml.CreateNode("element", "EntryPoints", ""));
            foreach (EntryPoint a in entryPoints)
            {
                XmlNode nEntryPoint = entryPointsList.AppendChild(newXml.CreateNode("element", "EntryPoint", ""));
                nEntryPoint.AppendChild(SetVal(newXml.CreateNode("element", "ClassPath", ""), a.classpath));
                nEntryPoint.AppendChild(SetVal(newXml.CreateNode("element", "LaunchType", ""), ((int)a.type).ToString()));
            }

            newXml.Save(path);
        }

        public static JarConfig LoadFromXML(string path, string jarName)
        {
            JarConfig newJarConf = new JarConfig(jarName);

            XmlDocument newXml = new XmlDocument();
            newXml.Load(path);
            XmlNode rootNode = newXml.SelectSingleNode("JarConfig");
            if (rootNode != null)
            {

                newJarConf.friendlyName = GetInnerOrDefault(rootNode, "FriendlyName");
                newJarConf.LWJGLVersion = GetInnerOrDefault(rootNode, "LWJGLVersion", "2.9.3");
                newJarConf.playerName = GetInnerOrDefault(rootNode, "PlayerName", "DECRAFT_player");
                newJarConf.jvmArgs = GetInnerOrDefault(rootNode, "JVMArgs", "-Djava.util.Arrays.useLegacyMergeSort=true");
                newJarConf.instanceDirName = GetInnerOrDefault(rootNode, "InstanceDirectory", jarName);
                newJarConf.windowW = uint.Parse(GetInnerOrDefault(rootNode, "WindowW", "960", "uint"));
                newJarConf.windowH = uint.Parse(GetInnerOrDefault(rootNode, "WindowH", "540", "uint"));

                newJarConf.entryPointsScanned = bool.Parse(GetInnerOrDefault(rootNode, "EntryPointsScanned", "false", "bool"));

                foreach (XmlNode a in newXml.GetElementsByTagName("EntryPoint"))
                {
                    EntryPoint b = new EntryPoint();
                    string classPath = GetInnerOrDefault(a, "ClassPath", null);
                    string type = GetInnerOrDefault(a, "LaunchType", null, "int");
                    if (classPath == null || type == null)
                    {
                        continue;
                    }
                    b.classpath = classPath;
                    try
                    {
                        b.type = (EntryPointType)int.Parse(type);
                    }
                    catch (FormatException)
                    {
                        continue;
                    }
                    newJarConf.entryPoints.Add(b);
                }
            }

            return newJarConf;
        }
    }
}
