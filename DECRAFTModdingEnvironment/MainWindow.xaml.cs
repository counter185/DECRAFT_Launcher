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
using DeCraftLauncher;
using SourceChord.FluentWPF;

namespace DECRAFTModdingEnvironment
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AcrylicWindow
    {
        public WorkspaceConfig workspaceConfig;
        public MainWindow()
        {
            InitializeComponent();
            DeCraftLauncher.Utils.Util.UpdateAcrylicWindowBackground(this);
            try
            {
                workspaceConfig = WorkspaceConfig.LoadFromXML("_dme_config.xml");
            } catch (Exception e)
            {
                //uhhh
            }
        }
    }
}
