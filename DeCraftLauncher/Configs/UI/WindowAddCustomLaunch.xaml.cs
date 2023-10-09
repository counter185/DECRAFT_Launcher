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
    /// Logika interakcji dla klasy WindowAddCustomLaunch.xaml
    /// </summary>
    public partial class WindowAddCustomLaunch : AcrylicWindow
    {

        JarConfig target;
        MainWindow parent;

        public WindowAddCustomLaunch(JarConfig target, MainWindow parent)
        {
            this.target = target;
            this.parent = parent;
            InitializeComponent();

            Dictionary<object, string> localization = new Dictionary<object, string>() {
                {label_windowaddcustomlaunch, "l.ui.window_add_custom_launch"},
                {label_fullcommand, "l.ui.window_custom_launch_fullcommand"},
                {btn_add, "btn.ui.window_custom_launch_add"},
            };
            MainWindow.locale.Localize(localization);

            Util.UpdateAcrylicWindowBackground(this);
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            target.entryPoints.Add(new JarUtils.EntryPoint(tbox_fullcommand.Text, JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND));
            target.SaveToXMLDefault();
            parent.UpdateLaunchOptionsSegment();
            this.Close();
        }
    }
}
