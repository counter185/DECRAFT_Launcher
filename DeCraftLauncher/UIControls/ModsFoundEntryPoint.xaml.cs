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

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy ModsFoundEntryPoint.xaml
    /// </summary>
    public partial class ModsFoundEntryPoint : UserControl
    {
        public ModsFoundEntryPoint(List<string> entries)
        {
            InitializeComponent();
            label_modsfound.Text = "Mods found:\n" + String.Join("\n", from x in entries select $"- {x}");
        }
    }
}
