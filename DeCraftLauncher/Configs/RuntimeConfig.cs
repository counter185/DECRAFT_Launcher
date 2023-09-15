using DeCraftLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace DeCraftLauncher.Configs
{
    public class RuntimeConfig
    {
        public string javaHome = "";
        public bool isJava9 = true;

        public void UpdateAutoIsJava9Option()
        {
            string verdk = JarUtils.GetJDKInstalled(javaHome);
            if (verdk != null)
            {
                int JDKVer = Util.TryParseJavaCVersionString(verdk);
                Console.WriteLine($"Detected JDK Version: {JDKVer}");
                if (JDKVer != -1)
                {
                    isJava9 = JDKVer >= 9;
                }
            }
        }

        public static RuntimeConfig LoadFromXML()
        {
            RuntimeConfig ret = new RuntimeConfig();
            string rtConfXMLFilePath = $"{MainWindow.configDir}/_launcher_config.xml";
            if (File.Exists(rtConfXMLFilePath))
            {
                XmlDocument newXml = new XmlDocument();
                newXml.Load(rtConfXMLFilePath);
                XmlNode rootNode = newXml.SelectSingleNode("RuntimeConfig");
                if (rootNode != null)
                {
                    ret.javaHome = Util.GetInnerOrDefault(rootNode, "JavaPath");
                    ret.isJava9 = bool.Parse(Util.GetInnerOrDefault(rootNode, "IsJava9", "true", "bool"));
                }
            }
            else
            {
                ret.UpdateAutoIsJava9Option();
            }
            return ret;
        }

        public void SaveToXML()
        {
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<?xml version=\"1.0\"?>\n<RuntimeConfig>\n</RuntimeConfig>");
            XmlNode rootElement = newXml.GetElementsByTagName("RuntimeConfig")[0];

            rootElement.AppendChild(Util.GenElementChild(newXml, "JavaPath", javaHome));
            rootElement.AppendChild(Util.GenElementChild(newXml, "IsJava9", isJava9.ToString()));

            newXml.Save($"{MainWindow.configDir}/_launcher_config.xml");
        }
    }
}
