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
            Util.UpdateAcrylicWindowBackground(this);

            GlobalVars.L.Translate(
                    label_header,
                    label_commandheader,
                    btn_add,
                    this
                );
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
