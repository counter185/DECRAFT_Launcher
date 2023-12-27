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
    /// Logika interakcji dla klasy REToolScanRefLevelClass.xaml
    /// </summary>
    public partial class REToolScanRefLevelClass : UserControl
    {
        public REToolScanRefLevelClass(string key, List<WindowREToolOutgoingRefsScanResult.RefScanEntry> refScanEntries)
        {
            InitializeComponent();
            label_classname.Content = key;
            foreach (var a in refScanEntries.OrderBy(x=>x.Name).GroupBy(x => x.Name+x.Descriptor))
            {
                panel_methods.Children.Add(new REToolScanRefLevelMethod(a.First().Name, a.First().Descriptor, a.ToList()));
            }
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            panel_methods.Visibility = panel_methods.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            btn_open.Content = panel_methods.Visibility == Visibility.Collapsed ? "Open" : "Close";
        }
    }
}
