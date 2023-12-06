using DeCraftLauncher.UIControls;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DeCraftLauncher.Configs.UI
{
    /// <summary>
    /// Interaction logic for WindowSetJarLibs.xaml
    /// </summary>
    public partial class WindowSetJarLibs : AcrylicWindow
    {
        JarConfig target;

        public WindowSetJarLibs(JarConfig target)
        {
            this.target = target;
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);

            label_header.Content = $"Set additional libraries for {Utils.Util.CleanStringForXAML(target.jarFileName)}";

            MainWindow.EnsureDir(MainWindow.jarLibsDir);
            
            foreach (string a in Directory.GetFiles(MainWindow.jarLibsDir))
            {
                string filename = new FileInfo(a).Name;
                JarLibListEntry jarLibListEntry = new JarLibListEntry(filename);
                jarLibListEntry.checkbox_jarlibenabled.IsChecked = target.addJarLibs.Contains(filename);
                listbox_jarlibs.Items.Add(jarLibListEntry);

            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            target.addJarLibs.Clear();
            foreach (JarLibListEntry a in listbox_jarlibs.Items.OfType<JarLibListEntry>())
            {
                if (a.checkbox_jarlibenabled.IsChecked == true)
                {
                    target.addJarLibs.Add(a.data);
                }
            }
            target.SaveToXMLDefault();
        }
    }
}
