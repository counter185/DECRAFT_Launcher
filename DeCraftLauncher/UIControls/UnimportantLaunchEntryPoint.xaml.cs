using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
using DeCraftLauncher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy LaunchEntryPoint.xaml
    /// </summary>
    public partial class UnimportantLaunchEntryPoint : UserControl
    {
        JarUtils.EntryPoint entryPoint;
        JarConfig jarConfig;
        MainWindow caller;

        public UnimportantLaunchEntryPoint(JarUtils.EntryPoint target, MainWindow caller, JarConfig jarConfig)
        {
            InitializeComponent();
            this.caller = caller;
            entryPoint = target;
            classname.Content = entryPoint.classpath;
            this.jarConfig = jarConfig;
        }

        private void launchButton_Click(object sender, RoutedEventArgs e)
        {
            caller.SaveCurrentJarConfig();
            if (jarConfig.EvalPrereqs())
            {
                jarConfig.DoLaunch(entryPoint, caller);
            }

        }

        private void launchAdvancedButton_Click(object sender, RoutedEventArgs e)
        {
            caller.SaveCurrentJarConfig();
            if (entryPoint.type == JarUtils.EntryPointType.APPLET)
            {
                new WindowAppletParametersOptions(entryPoint.classpath, caller, jarConfig).Show();
            }
        }
    }
}
