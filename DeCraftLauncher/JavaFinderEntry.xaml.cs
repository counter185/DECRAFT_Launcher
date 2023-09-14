using DeCraftLauncher.Configs;
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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy JavaFinderEntry.xaml
    /// </summary>
    public partial class JavaFinderEntry : UserControl
    {
        string versionString = "???";
        string path = "";

        JavaFinder target;

        public JavaFinderEntry(JavaFinder target, JarUtils.JavaFinderResult javaInstall)
        {
            this.target = target;
            InitializeComponent();
            this.versionString = javaInstall.version;
            this.path = javaInstall.path;

            label_version.Content = versionString.Replace("_", "__");
            label_path.Content = path.Replace("_", "__");
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            target.caller.SetAndTestJavaPath(this.path);
            target.Close();
        }
    }
}
