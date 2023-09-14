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

namespace DeCraftLauncher.Configs
{
    /// <summary>
    /// Logika interakcji dla klasy JavaFinder.xaml
    /// </summary>
    public partial class JavaFinder : AcrylicWindow
    {
        public Config caller;

        public JavaFinder(Config caller)
        {
            this.caller = caller;

            InitializeComponent();
            List<JarUtils.JavaFinderResult> javaInstalls = JarUtils.FindAllJavaInstallations();
            foreach (JarUtils.JavaFinderResult javaInstall in javaInstalls)
            {
                listbox_javaversions.Items.Add(new JavaFinderEntry(this, javaInstall));
            }

            
        }
    }
}
