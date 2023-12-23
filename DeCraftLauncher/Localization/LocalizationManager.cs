using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        public LocalizationManager() : this("default.txt")
        {
            
        }

        public LocalizationManager(string file)
        {
            //keyToTranslatedStringMap.Clear();

            //this is the linq statement of all time
            (from x in File.ReadLines(file)
            where x.Contains('=')
            select new Func<string, KeyValuePair<string,string>>(y =>
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

        public void Translate(IEnumerable<UIElement> controls)
        {
            controls.ToList().ForEach(x =>
            {
                if (x is TextBlock)
                {
                    ((TextBlock)x).Text = keyToTranslatedStringMap[GlobalVars.GetLocKey(x)];
                }
                else if (x is ContentControl)
                {
                    ((ContentControl)x).Content = keyToTranslatedStringMap[GlobalVars.GetLocKey(x)];
                }
            });
        }

    }
}
