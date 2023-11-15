using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DeCraftLauncher.Translation
{
    public class Translatable
    {
        public object target;
        public string key;

        public Translatable(object target, string key)
        {
            this.target = target;
            this.key = key;
        }

        public void Translate(Dictionary<string,string> dict)
        {
            string text = dict[key].Replace("\\n","\n");
            if (text == "---")
            {
                return;
            }
            if (target is Label)
            {
                ((Label)target).Content = text;
            }
            if (target is TextBlock)
            {
                ((TextBlock)target).Text = text;
            }
            else if (target is Button)
            {
                ((Button)target).Content = text;
            }
        }
    }


    public class UITranslator
    {
        public Dictionary<string, string> currentKeyToString = new Dictionary<string, string>();

        public void LoadIni(string lang)
        {
            try
            {
                using (StreamReader tReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"DeCraftLauncher.Translation.{lang}.txt")))
                {
                    string nextLine = tReader.ReadLine();
                    while (nextLine != null)
                    {
                        if (nextLine.Contains("="))
                        {
                            string[] keyValPair = nextLine.Split('=');
                            currentKeyToString[keyValPair[0]] = keyValPair[1];
                        }

                        nextLine = tReader.ReadLine();
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException("Error accessing resource!");
            }
        }


        public void Translate(IEnumerable<Translatable> translatables)
        {
            foreach (Translatable a in translatables)
            {
                a.Translate(currentKeyToString);
            }
        }
    }
}
