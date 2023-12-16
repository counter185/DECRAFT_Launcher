using Newtonsoft.Json.Linq;
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
    /// Logika interakcji dla klasy REToolConstPoolEntry.xaml
    /// </summary>
    public partial class REToolConstPoolEntry : UserControl
    {
        ConstantPoolEntry target;
        List<ConstantPoolEntry> allEntries;
        public List<int> subEntries;

        public REToolConstPoolEntry(int index, List<ConstantPoolEntry> allEntries, List<int> prevSubEntries = null)
        {
            this.allEntries = allEntries;
            this.subEntries = prevSubEntries ?? new List<int>();
            this.target = allEntries[index];
            InitializeComponent();

            label_entryname.Content = $"[{index}]";
            label_entrytype.Content =
                target is ConstantPoolEntry.ClassReferenceEntry ? "(class reference)"
                : target is ConstantPoolEntry.StringEntry ? "(string)"
                : target is ConstantPoolEntry.FloatEntry ? "(float)"
                : target is ConstantPoolEntry.DoubleEntry ? "(double)"
                : target is ConstantPoolEntry.LongEntry ? "(long)"
                : target is ConstantPoolEntry.FieldReferenceEntry ? "(field reference)"
                : target is ConstantPoolEntry.DynamicEntry ? "(dynamic)"
                : target is ConstantPoolEntry.IntegerEntry ? "(integer)"
                : target is ConstantPoolEntry.InterfaceMethodReferenceEntry ? "(interface method reference)"
                : target is ConstantPoolEntry.InvokeDynamicEntry ? "(invoke dynamic)"
                : target is ConstantPoolEntry.MethodHandleEntry ? "(method handle)"
                : target is ConstantPoolEntry.MethodReferenceEntry ? "(method reference)"
                : target is ConstantPoolEntry.MethodTypeEntry ? "(method type)"
                : target is ConstantPoolEntry.ModuleEntry ? "(module)"
                : target is ConstantPoolEntry.NameAndTypeDescriptorEntry ? "(name and type descriptor)"
                : target is ConstantPoolEntry.PackageEntry ? "(package)"
                : target is ConstantPoolEntry.StringReferenceEntry ? "(string reference)"
                : $"(??? tag {target.tag})";

            label_lessimportantvalue.Content = "";
            label_value.Content = "";
            if (target is ConstantPoolEntry.StringEntry)
            {
                label_value.Content = ((ConstantPoolEntry.StringEntry)target).value;
            }
            else if (target is ConstantPoolEntry.IntegerEntry)
            {
                int val = ((ConstantPoolEntry.IntegerEntry)target).value;
                label_value.Content = val.ToString();
                label_lessimportantvalue.Content = $"-> 0x{Convert.ToString(val, 16)}";
            }
            else if (target is ConstantPoolEntry.FloatEntry)
            {
                label_value.Content = ((ConstantPoolEntry.FloatEntry)target).value.ToString();
            }
            else if (target is ConstantPoolEntry.DoubleEntry)
            {
                label_value.Content = ((ConstantPoolEntry.DoubleEntry)target).value.ToString();
            }
            else if (target is ConstantPoolEntry.ClassReferenceEntry)
            {
                int idx = ((ConstantPoolEntry.ClassReferenceEntry)target).indexOfClassNameString;
                AddSubElement(idx);
                label_lessimportantvalue.Content = $"-> {((ConstantPoolEntry.StringEntry)allEntries[idx]).value.Replace('/', '.')}";
            }
            else if (target is ConstantPoolEntry.NameAndTypeDescriptorEntry)
            {
                int idx1 = ((ConstantPoolEntry.NameAndTypeDescriptorEntry)target).indexOfNameString;
                int idx2 = ((ConstantPoolEntry.NameAndTypeDescriptorEntry)target).indexOfTypeDescriptor;
                AddSubElement(idx1);
                AddSubElement(idx2);
                string descriptor = ((ConstantPoolEntry.StringEntry)allEntries[idx2]).value;
                if (descriptor.StartsWith("("))
                {
                    //$"({String.Join(", ", (from x in ParseParameters(descriptor) select DescriptorTypeToFriendlyName(x)))})";
                    label_lessimportantvalue.Content = $"-> {REToolMethodEntry.DescriptorTypeToFriendlyName(descriptor.Substring(descriptor.LastIndexOf(')') + 1))} {((ConstantPoolEntry.StringEntry)allEntries[idx1]).value}({String.Join(", ", (from x in REToolMethodEntry.ParseParameters(descriptor) select REToolMethodEntry.DescriptorTypeToFriendlyName(x)))});";
                }
                else
                {
                    label_lessimportantvalue.Content = $"-> {REToolMethodEntry.DescriptorTypeToFriendlyName(descriptor)} {((ConstantPoolEntry.StringEntry)allEntries[idx1]).value};";
                }
            }
            else if (target is ConstantPoolEntry.MethodReferenceEntry)
            {
                AddSubElement(((ConstantPoolEntry.MethodReferenceEntry)target).indexOfClassReference);
                AddSubElement(((ConstantPoolEntry.MethodReferenceEntry)target).indexOfNameAndTypeDescriptor);
            } 
            else if (target is ConstantPoolEntry.FieldReferenceEntry)
            {
                AddSubElement(((ConstantPoolEntry.FieldReferenceEntry)target).indexOfClassReference);
                AddSubElement(((ConstantPoolEntry.FieldReferenceEntry)target).indexOfNameAndTypeDescriptor);
            } 
            else if (target is ConstantPoolEntry.StringReferenceEntry)
            {
                AddSubElement(((ConstantPoolEntry.StringReferenceEntry)target).indexOfTargetString);
            }
            else if (target is ConstantPoolEntry.InterfaceMethodReferenceEntry)
            {
                AddSubElement(((ConstantPoolEntry.InterfaceMethodReferenceEntry)target).indexOfClassReference);
                AddSubElement(((ConstantPoolEntry.InterfaceMethodReferenceEntry)target).indexOfNameAndTypeDescriptor);
            } 
            else
            {
                label_value.Content = "(not implemented)";
            }
        }

        private void AddSubElement(int index) {
            if (!subEntries.Contains(index)) {
                panel_subentries.Children.Add(new REToolConstPoolEntry(index, allEntries,subEntries));
                subEntries.Add(index);
            }
        }
    }
}
