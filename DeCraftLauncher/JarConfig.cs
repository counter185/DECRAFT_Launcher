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
        public string proxyHost = "";
        public uint windowW = 960;
        public uint windowH = 540;
        public string maxJavaVersion = "";
        public string minJavaVersion = "";

        //advanced options
        public string sessionID = "0";
        public string gameArgs = "";
        public bool appletEmulateHTTP = true;
        public string documentBaseUrl = "http://www.minecraft.net/play.jsp";
        public bool appletRedirectSkins = true;
        public string appletSkinRedirectPath = "C:\\skincache";

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

        const string NODETYPE_ELEMENT = "element";

        private XmlNode GenElementChild(XmlDocument doc, string name, string value = null)
        {
            var result = doc.CreateNode(NODETYPE_ELEMENT, name, string.Empty);
            if(!string.IsNullOrEmpty(value))
                result.InnerText = value;
            return result;
        }

        public void SaveToXML(string path)
        {
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<?xml version=\"1.0\"?>\n<JarConfig>\n</JarConfig>");
            XmlNode rootElement = newXml.GetElementsByTagName("JarConfig")[0];
            rootElement.AppendChild(GenElementChild(newXml, "FriendlyName", friendlyName));
            rootElement.AppendChild(GenElementChild(newXml, "LWJGLVersion", LWJGLVersion));
            rootElement.AppendChild(GenElementChild(newXml, "PlayerName", playerName));
            rootElement.AppendChild(GenElementChild(newXml, "InstanceDirectory", instanceDirName));
            rootElement.AppendChild(GenElementChild(newXml, "JVMArgs", jvmArgs));
            rootElement.AppendChild(GenElementChild(newXml, "WindowW", windowW.ToString()));
            rootElement.AppendChild(GenElementChild(newXml, "WindowH", windowH.ToString()));
            rootElement.AppendChild(GenElementChild(newXml, "EntryPointsScanned", entryPointsScanned.ToString()));
            rootElement.AppendChild(GenElementChild(newXml, "ProxyHost", proxyHost));
            rootElement.AppendChild(GenElementChild(newXml, "MaxJavaVersion", maxJavaVersion));
            rootElement.AppendChild(GenElementChild(newXml, "MinJavaVersion", minJavaVersion));

            rootElement.AppendChild(GenElementChild(newXml, "PassSessionID", sessionID));
            rootElement.AppendChild(GenElementChild(newXml, "GameArgs", gameArgs));
            rootElement.AppendChild(GenElementChild(newXml, "AppletEmulateHTTP", appletEmulateHTTP.ToString()));
            rootElement.AppendChild(GenElementChild(newXml, "AppletDocumentURL", documentBaseUrl));
            rootElement.AppendChild(GenElementChild(newXml, "AppletRedirectToLocalSkins", appletRedirectSkins.ToString()));
            rootElement.AppendChild(GenElementChild(newXml, "AppletSkinRedirectLocalPath", appletSkinRedirectPath));

            XmlNode entryPointsList = rootElement.AppendChild(GenElementChild(newXml, "EntryPoints"));
            foreach (EntryPoint a in entryPoints)
            {
                XmlNode nEntryPoint = entryPointsList.AppendChild(newXml.CreateNode("element", "EntryPoint", ""));
                nEntryPoint.AppendChild(SetVal(newXml.CreateNode("element", "ClassPath", ""), a.classpath));
                nEntryPoint.AppendChild(SetVal(newXml.CreateNode("element", "LaunchType", ""), ((int)a.type).ToString()));
                nEntryPoint.AppendChild(SetVal(newXml.CreateNode("element", "ClassInfo", ""), a.additionalInfo));
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
                newJarConf.proxyHost = GetInnerOrDefault(rootNode, "ProxyHost");
                newJarConf.maxJavaVersion = GetInnerOrDefault(rootNode, "MaxJavaVersion");
                newJarConf.minJavaVersion = GetInnerOrDefault(rootNode, "MinJavaVersion");

                newJarConf.sessionID = GetInnerOrDefault(rootNode, "SessionID", "0");
                newJarConf.gameArgs = GetInnerOrDefault(rootNode, "GameArgs");
                newJarConf.appletEmulateHTTP = bool.Parse(GetInnerOrDefault(rootNode, "AppletEmulateHTTP", "true", "bool"));
                newJarConf.documentBaseUrl = GetInnerOrDefault(rootNode, "AppletDocumentURL", "http://www.minecraft.net/play.jsp");
                newJarConf.appletRedirectSkins = bool.Parse(GetInnerOrDefault(rootNode, "AppletRedirectToLocalSkins", "true", "bool"));
                newJarConf.appletSkinRedirectPath = GetInnerOrDefault(rootNode, "AppletSkinRedirectLocalPath", "C:\\skincache");

                newJarConf.entryPointsScanned = bool.Parse(GetInnerOrDefault(rootNode, "EntryPointsScanned", "false", "bool"));

                foreach (XmlNode a in newXml.GetElementsByTagName("EntryPoint"))
                {
                    EntryPoint b = new EntryPoint();
                    string classPath = GetInnerOrDefault(a, "ClassPath", null);
                    string type = GetInnerOrDefault(a, "LaunchType", null, "int");
                    string additionalInfo = GetInnerOrDefault(a, "ClassInfo", "");
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
                    b.additionalInfo = additionalInfo;
                    newJarConf.entryPoints.Add(b);
                }
            }

            return newJarConf;
        }
    }
}
