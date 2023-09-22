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
        public List<Category> jarCategories = new List<Category>();
        public List<JarEntry> jarEntries = new List<JarEntry>();

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

                XmlNode categoriesNode = rootNode.SelectSingleNode("Categories");
                if (categoriesNode != null)
                {
                    foreach (XmlNode catNode in categoriesNode.SelectNodes("Category"))
                    {
                        string catName = Util.GetInnerOrDefault(catNode, "Name", null);
                        string colorText = Util.GetInnerOrDefault(catNode, "Color", null);
                        //todo: verify if it's actually a hex value
                        try
                        {
                            UInt32 catColor = colorText != null ? UInt32.Parse(colorText, System.Globalization.NumberStyles.HexNumber) : 0;
                            if (catName != null && colorText != null)
                            {
                                Category cat = new Category(catName, colorText);
                                ret.jarCategories.Add(cat);
                            }
                        } catch (FormatException)
                        {
                        }
                    }
                }

                XmlNode jarsNode = rootNode.SelectSingleNode("Jars");
                if (jarsNode != null)
                {
                    foreach (XmlNode jarNode in jarsNode.SelectNodes("JarEntry"))
                    {
                        string jarFileName = Util.GetInnerOrDefault(jarNode, "JarFileName", null);
                        string jarFriendlyName = Util.GetInnerOrDefault(jarNode, "FriendlyName", "");
                        string category = Util.GetInnerOrDefault(jarNode, "Category", null);
                        IEnumerable<Category> matchingCategories = 
                            (from x in ret.jarCategories
                             where category != null && x.name == category
                             select x);

                        JarEntry jarEntry = new JarEntry(jarFileName);
                        jarEntry.friendlyName = jarFriendlyName;
                        jarEntry.category = matchingCategories.Any() ? matchingCategories.First() : null;

                        ret.jarEntries.Add(jarEntry);
                    }
                }
            }
            else
            {
                ret.UpdateAutoIsJava9Option();
            }
            return ret;
        }

        public void SaveToXML(MainWindow caller)
        {
            XmlDocument newXml = new XmlDocument();
            newXml.LoadXml("<?xml version=\"1.0\"?>\n<RuntimeConfig>\n</RuntimeConfig>");
            XmlNode rootElement = newXml.GetElementsByTagName("RuntimeConfig")[0];

            rootElement.AppendChild(Util.GenElementChild(newXml, "JavaPath", javaHome));
            rootElement.AppendChild(Util.GenElementChild(newXml, "IsJava9", isJava9.ToString()));

            XmlNode catEntries = Util.GenElementChild(newXml, "Categories");
            foreach (Category cat in jarCategories) //meow
            {
                XmlNode catEntryNode = Util.GenElementChild(newXml, "Category");
                catEntryNode.AppendChild(Util.GenElementChild(newXml, "Name", cat.name));
                catEntryNode.AppendChild(Util.GenElementChild(newXml, "Color", cat.color));
                catEntries.AppendChild(catEntryNode);
            }
            rootElement.AppendChild(catEntries);


            XmlNode jarEntries = Util.GenElementChild(newXml, "Jars");
            foreach (JarEntry jarEntry in caller.loadedJars)
            {
                XmlNode jarEntryNode = Util.GenElementChild(newXml, "JarEntry");
                jarEntryNode.AppendChild(Util.GenElementChild(newXml, "JarFileName", jarEntry.jarFileName));
                jarEntryNode.AppendChild(Util.GenElementChild(newXml, "FriendlyName", jarEntry.friendlyName));
                if (jarEntry.category != null)
                {
                    jarEntryNode.AppendChild(Util.GenElementChild(newXml, "Category", jarEntry.category.name));
                }
                jarEntries.AppendChild(jarEntryNode);
            }
            rootElement.AppendChild(jarEntries);

            newXml.Save($"{MainWindow.configDir}/_launcher_config.xml");
        }
    }
}
