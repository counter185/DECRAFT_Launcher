using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DeCraftLauncher.Utils.JavaClassReader;

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy REToolMethodEntry.xaml
    /// </summary>
    public partial class REToolMethodEntry : UserControl
    {
        JavaMethodInfo target;

        public static string DescriptorTypeToFriendlyName(string descriptorType)
        {
            switch (descriptorType)
            {
                case "V":
                    return "void";
                case "S":
                    return "short"; 
                case "I":
                    return "int"; 
                case "J":
                    return "long";                
                case "B":
                    return "byte";
                case "Z":
                    return "boolean";                
                case "F":
                    return "float";                
                case "D":
                    return "double";
                default:
                    if (descriptorType.StartsWith("["))
                    {
                        return $"{DescriptorTypeToFriendlyName(descriptorType.Substring(1))}[]";
                    }
                    else if (descriptorType.StartsWith("L") && descriptorType.EndsWith(";"))
                    {
                        return descriptorType.Substring(1, descriptorType.Length - 2).Replace("/", ".");
                    }
                    return descriptorType;
            }
        }

        public static string[] ParseParameters(string descriptorString)
        {
            string inParenth = descriptorString.Substring(1, descriptorString.IndexOf(')') - 1);
            List<string> separated = new List<string>();

            string readingNow = "";
            bool isReadingClassName = false;
            foreach (char a in inParenth)
            {
                readingNow += a;
                if (a == 'L' && !isReadingClassName)
                {
                    isReadingClassName = true;
                }
                else if (a == ';' || (a != '[' && !isReadingClassName))
                {
                    separated.Add(readingNow);
                    isReadingClassName = false;
                    readingNow = "";
                }
            }
            return separated.ToArray();
        }

        public REToolMethodEntry(JavaMethodInfo methodInfo, List<ConstantPoolEntry> entries)
        {
            target = methodInfo;
            InitializeComponent();
            string descriptor = target.Descriptor(entries);
            label_functionname.Content = target.Name(entries);
            label_returntype.Content = DescriptorTypeToFriendlyName(descriptor.Substring(descriptor.LastIndexOf(')')+1));

            if (target.Name(entries) == "<init>")
            {
                label_functionname.Content = "<Constructor>";
                label_returntype.Content = "";
            }

            label_paramnames.Content = $"({String.Join(", ", (from x in ParseParameters(descriptor) select DescriptorTypeToFriendlyName(x)))})";

            label_modifiers.Content =
                (target.IsPublic ? "public" : "")
                + (target.IsStatic ? " static" : "");
        }
    }
}
