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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy Config.xaml
    /// </summary>
    public partial class Config : AcrylicWindow
    {
        MainWindow parent;

        public Config(MainWindow parent)
        {
            InitializeComponent();
            Utils.UpdateAcrylicWindowBackground(this);
            this.parent = parent;
            jre_path.Text = MainWindow.javaHome;
        }

        public void FixJavaHomeString()
        {
            while (jre_path.Text.StartsWith(" "))
            {
                jre_path.Text = jre_path.Text.Substring(1);
            }
            if (jre_path.Text != "" && !jre_path.Text.EndsWith("\\"))
            {
                jre_path.Text += "\\";
            }
        }

        public void UpdateJREVersionString()
        {
            FixJavaHomeString();

            string verre = JarUtils.GetJREInstalled(jre_path.Text);
            string verdk = JarUtils.GetJDKInstalled(jre_path.Text);

            jreconfig_version.Content = $"JRE: {(verre != null ? verre : "<none>")}\nJDK: {(verdk != null ? verdk : "<none>")}";

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void jre_path_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateJREVersionString();
            }
        }

        private void AcrylicWindow_Closed(object sender, EventArgs e)
        {
            FixJavaHomeString();
            MainWindow.javaHome = jre_path.Text;
        }
    }
}
