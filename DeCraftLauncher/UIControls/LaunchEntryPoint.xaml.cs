using DeCraftLauncher.Configs;
using DeCraftLauncher.Configs.UI;
using DeCraftLauncher.UIControls.Popup;
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
    public partial class LaunchEntryPoint : UserControl
    {
        JarUtils.EntryPoint entryPoint;
        JarConfig jarConfig;
        MainWindow caller;

        public string GetDescription()
        {
            if (entryPoint.type == JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND)
            {
                return Util.CleanStringForXAML(entryPoint.classpath);
            }
            return entryPoint.GetDescription();
        }

        public LaunchEntryPoint(JarUtils.EntryPoint target, MainWindow caller, JarConfig jarConfig)
        {
            InitializeComponent();
            this.caller = caller;
            this.jarConfig = jarConfig;
            this.entryPoint = target;

            string cleanClassPath = Util.CleanStringForXAML(entryPoint.classpath);
            //cleanClassPath = cleanClassPath.Length > 50 ? cleanClassPath.Substring(0, 50) + "..." : cleanClassPath;
            classname.Foreground = target.GetUIColor() ?? classname.Foreground;
            classname.Content = (target.GetImportance() > 1 ? "★ " : "") + (entryPoint.type == JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND ? "Custom" : cleanClassPath);
            classname.ToolTip = Util.CleanStringForXAML(entryPoint.classpath);

            desc.Content = GetDescription();
            mode.Content =
                entryPoint.type == JarUtils.EntryPointType.STATIC_VOID_MAIN ? "(main function)"
                : entryPoint.type == JarUtils.EntryPointType.RUNNABLE ? "(Runnable)"
                : entryPoint.type == JarUtils.EntryPointType.APPLET ? "(Applet)"
                : entryPoint.type == JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND ? "(custom launch command)"
                : "<unknown>";
            moreInfo.Content = Util.CleanStringForXAML(target.additionalInfo);
            switch (entryPoint.type)
            {
                case JarUtils.EntryPointType.STATIC_VOID_MAIN:
                    launchAdvancedButton.Content = "Launch+";
                    launchAdvancedButton.ToolTip = "Launch using the Main function Wrapper. Allows you to use server redirection & other wrapper-specific settings.\nRequires a JDK.";
                    break;
                case JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND:
                    launchAdvancedButton.Content = "Remove...";
                    break;
                case JarUtils.EntryPointType.APPLET:
                    launchAdvancedButton.Content = "Parameters...";
                    break;
                default:
                    launchAdvancedButton.Visibility = Visibility.Hidden;
                    break;
            }
            
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
            else if (entryPoint.type == JarUtils.EntryPointType.STATIC_VOID_MAIN)
            {
                if (jarConfig.EvalPrereqs())
                {
                    MainFunctionWrapper.LaunchMainFunctionWrapper(entryPoint.classpath, caller, jarConfig);
                }
            }
            else if (entryPoint.type == JarUtils.EntryPointType.CUSTOM_LAUNCH_COMMAND)
            {
                if (PopupYesNo.ShowNewPopup($"Remove the custom launch command?\n\njava {this.entryPoint.classpath}", "DECRAFT") == MessageBoxResult.Yes)
                {
                    jarConfig.entryPoints.Remove(this.entryPoint);
                    jarConfig.SaveToXMLDefault();
                    caller.UpdateLaunchOptionsSegment();
                }
            }
        }
    }
}
