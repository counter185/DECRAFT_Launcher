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

namespace DeCraftLauncher.UIControls.RETool
{
    /// <summary>
    /// Logika interakcji dla klasy REToolScanRefLevelMethod.xaml
    /// </summary>
    public partial class REToolScanRefLevelMethod : UserControl
    {
        public REToolScanRefLevelMethod(string name, string descriptor, List<WindowREToolOutgoingRefsScanResult.RefScanEntry> refScanEntries)
        {
            InitializeComponent();
            label_methodname.Content = Util.CleanStringForXAML(name);
            var parameters = from x in REToolMethodEntry.ParseParameters(descriptor)
                             select Util.CleanStringForXAML(REToolMethodEntry.DescriptorTypeToFriendlyName(x));
            label_parameters.Content = $"({String.Join(", ", parameters)})";
            label_returntype.Content = Util.CleanStringForXAML(REToolMethodEntry.DescriptorTypeToFriendlyName(descriptor.Substring(descriptor.LastIndexOf(')') + 1)));
            

            foreach (var a in refScanEntries)
            {
                panel_references.Children.Add(new REToolScanRefLevelRef(a.CallingClassName));
            }
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            panel_references.Visibility = panel_references.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            btn_open.Content = panel_references.Visibility == Visibility.Collapsed ? "Open" : "Close";
        }
    }
}
