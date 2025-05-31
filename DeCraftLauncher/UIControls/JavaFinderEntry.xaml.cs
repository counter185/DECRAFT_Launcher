using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
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

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy JavaFinderEntry.xaml
    /// </summary>
    public partial class JavaFinderEntry : UserControl
    {
        string versionString = "???";
        string path = "";
        string implementor = "";
        string arch = "??";

        WindowJavaFinder target;

        public JavaFinderEntry(WindowJavaFinder target, JarUtils.JavaFinderResult javaInstall)
        {
            this.target = target;
            InitializeComponent();
            this.versionString = javaInstall.version;
            this.path = javaInstall.path;
            this.implementor = javaInstall.implementor;
            this.arch = javaInstall.arch;

            label_version.Content = Util.CleanStringForXAML(versionString);
            label_path.Content = Util.CleanStringForXAML(path);
            label_implementor.Content = Util.CleanStringForXAML(implementor);
            label_arch.Content = Util.CleanStringForXAML(arch);
            label_arch.Foreground = GetBrushForArch(arch);

            label_type.Content = javaInstall.hasJDK ? "JDK" : "JRE";
            label_type.Foreground = javaInstall.hasJDK ? Brushes.Turquoise : Brushes.Yellow;
        }

        Brush GetBrushForArch(string archV)
        {
            switch (archV.ToLower())
            {
                case "amd64":
                    return Brushes.Green;
                case "x86_64":
                    return Brushes.LawnGreen;
                case "i386":
                case "i586":
                    return Brushes.IndianRed;
                case "x86":
                    return Brushes.Red;
                case "arm64":
                case "aarch64":
                    return Brushes.Yellow;
            }
            return Brushes.DeepSkyBlue;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            target.caller.SetAndTestJavaPath(this.path);
            target.Close();
        }
    }
}
