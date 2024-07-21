using DeCraftLauncher.Properties;
using DeCraftLauncher.Utils.NBTEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xaml;
using System.Xml;

namespace DeCraftLauncher.Localization
{
    public class LocalizationManager
    {
        public class TLDict : Dictionary<string,string>
        {
            HashSet<string> keysRequested = new HashSet<string>();

            public new string this[string key]
            {
                get
                {
                    keysRequested.Add(key);
                    return key == null ? "-- KEY IS NULL" : this.ContainsKey(key) ? base[key] : $"-- NO KEY: {Utils.Util.CleanStringForXAML(key)}";
                }
                set => base[key] = value;
            }
        }

        public Dictionary<string,string> Tl
        {
            get => keyToTranslatedStringMap;
            set { }
        }

        private TLDict keyToTranslatedStringMap = new TLDict();

        public LocalizationManager()
        {
            FromIEnumerableString(Resources.LocDefault.Replace("\r", "").Split('\n'));
            FromFile("default.txt");
        }

        public void FromFile(string file)
        {
            if (File.Exists(file))
            {
                FromIEnumerableString(File.ReadLines(file));
            }
        }

        public void FromIEnumerableString(IEnumerable<string> strings)
        {
            (from x in strings
             where x.Contains('=')
             select new Func<string, KeyValuePair<string, string>>(y =>
             {
                 var splt = y.Split('=');
                 return new KeyValuePair<string, string>(splt[0], splt[1].Replace("&#x0a;", "\n")
                                                                         .Replace("&quot;", "\""));
             }).Invoke(x)).ToList().ForEach(z => keyToTranslatedStringMap[z.Key] = z.Value);
        }

        public static void SetLocDataContext(Window window)
        {
            window.DataContext = GlobalVars.locManager;
            
        }

        public void Translate(params UIElement[] controls)
        {
            controls.ToList().ForEach(x =>
            {
                if (x is Window)
                {
                    ((Window)x).Title = keyToTranslatedStringMap[GlobalVars.GetLocKey(x)];
                }
                else if (x is TextBlock)
                {
                    ((TextBlock)x).Text = keyToTranslatedStringMap[GlobalVars.GetLocKey(x)];
                }
                else if (x is ContentControl)
                {
                    ((ContentControl)x).Content = keyToTranslatedStringMap[GlobalVars.GetLocKey(x)];
                }
            });
        }

        public string Translate(string key, params string[] args)
        {
            string outS = keyToTranslatedStringMap[key];
            for (int x = 0; x < args.Length; x++)
            {
                outS = outS.Replace($"&arg{x};", args[x]);
            }
            return outS;
        }

        public void GenerateLocalizationsFromXAML(params string[] xamlPaths)
        {
            List<KeyValuePair<string, string>> allNodes = new List<KeyValuePair<string, string>>();

            foreach (string xaml in xamlPaths)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xaml);
                //simple linq one liner 😋
                IEnumerable<KeyValuePair<string,string>> validNodes 
                    = (from xn in xdoc.GetElementsByTagName("Label").OfType<XmlElement>()
                                      .Concat(xdoc.GetElementsByTagName("Button").OfType<XmlElement>())
                                      .Concat(xdoc.GetElementsByTagName("fw:AcrylicWindow").OfType<XmlElement>())
                                      .Concat(xdoc.GetElementsByTagName("TextBlock").OfType<XmlElement>()).ToList()
                                      .Concat(xdoc.GetElementsByTagName("CheckBox").OfType<XmlElement>()).ToList()
                       where (from xn_attr in xn.Attributes.OfType<XmlAttribute>()
                              where xn_attr.Name.EndsWith("LocKey")
                              select xn_attr).Any()
                       select new KeyValuePair<string, string>((from xn_attr in xn.Attributes.OfType<XmlAttribute>()
                                                                where xn_attr.Name.EndsWith("LocKey")
                                                                select xn_attr).First().Value, 
                                                               (from xn_attr in xn.Attributes.OfType<XmlAttribute>()
                                                                where xn_attr.Name == "Text"
                                                                      || xn_attr.Name == "Content"
                                                                      || xn_attr.Name == "Title"
                                                                select xn_attr).First().Value)).GroupBy(x=>x.Key).Select(x=>x.First());
                allNodes.AddRange(validNodes);
            }

            string appendGenFile = "../../Localization/default-genappend.txt";
            string saveGenFile = "../../Localization/default.txt";
            IEnumerable<string> saveStrings = (from x in allNodes
                                               orderby x.Key
                                               select $"{x.Key}={x.Value.Replace("\n", "&#x0a;").Replace("\"", "&quot;")}")
                                               .Concat(File.Exists(appendGenFile) ? File.ReadAllLines(appendGenFile) : new string[] { });

            File.WriteAllLines(saveGenFile, saveStrings);
        }

    }
}
