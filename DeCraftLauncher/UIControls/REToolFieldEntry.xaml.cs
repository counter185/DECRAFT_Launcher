using DeCraftLauncher.Utils;
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
    /// Logika interakcji dla klasy REToolFieldEntry.xaml
    /// </summary>
    public partial class REToolFieldEntry : UserControl
    {
        JavaFieldInfo target;

        public REToolFieldEntry(JavaFieldInfo methodInfo, List<ConstantPoolEntry> entries)
        {
            target = methodInfo;
            InitializeComponent();
            string descriptor = target.Descriptor(entries);
            label_fieldname.Content = Util.CleanStringForXAML(target.Name(entries));
            label_fieldtype.Content = Util.CleanStringForXAML(REToolMethodEntry.DescriptorTypeToFriendlyName(descriptor.Substring(descriptor.LastIndexOf(')')+1)));

            label_modifiers.Content = String.Join(" ", (from x in JavaMethodInfo.accessFlagNames
                                                        where (target.accessFlags & x.Key) == x.Key
                                                        select x.Value));
        }
    }
}
