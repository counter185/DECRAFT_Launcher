using DeCraftLauncher.UIControls;
using DeCraftLauncher.Utils;
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

namespace DeCraftLauncher.Configs.UI
{
    /// <summary>
    /// Logika interakcji dla klasy JavaFinder.xaml
    /// </summary>
    public partial class WindowJavaFinder : AcrylicWindow
    {
        public WindowRuntimeConfig caller;

        public WindowJavaFinder(WindowRuntimeConfig caller)
        {
            this.caller = caller;

            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
            List<JarUtils.JavaFinderResult> javaInstalls = JarUtils.FindAllJavaInstallations();
            foreach (JarUtils.JavaFinderResult javaInstall in (from x in javaInstalls 
                                                               orderby Util.TryParseJavaVersionString(x.version) descending
                                                               select x))
            {
                listbox_javaversions.Items.Add(new JavaFinderEntry(this, javaInstall));
            }

            GlobalVars.L.Translate(
                    this,
                    label_header
                );
        }
    }
}
