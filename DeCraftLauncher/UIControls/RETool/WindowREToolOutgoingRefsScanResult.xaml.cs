using SourceChord.FluentWPF;
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
using System.Windows.Shapes;

namespace DeCraftLauncher.UIControls.RETool
{
    /// <summary>
    /// Logika interakcji dla klasy WindowREToolOutgoingRefsScanResult.xaml
    /// </summary>
    public partial class WindowREToolOutgoingRefsScanResult : AcrylicWindow
    {

        public class RefScanEntry
        {
            public string ClassName;
            public string Name;
            public string Descriptor;
            public string CallingClassName;
        }

        public WindowREToolOutgoingRefsScanResult(IEnumerable<RefScanEntry> scanResult)
        {
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);

            /*foreach (string x in scanResult) { 
                panel_refs.Children.Add(new Label { 
                    Content = x,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0,-2,0,-2)
                }); 
            }*/

            foreach (var refScanEntry in scanResult.GroupBy(x=>x.ClassName))
            {
                panel_refs.Children.Add(new REToolScanRefLevelClass(refScanEntry.Key, refScanEntry.ToList()));
            }
        }
    }
}
