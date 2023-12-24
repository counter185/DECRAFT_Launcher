using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy JarListEntry.xaml
    /// </summary>
    public partial class JarListEntry : UserControl
    {
        MainWindow caller;
        public JarEntry jar;

        public static readonly DependencyProperty InnerTextProperty =
            DependencyProperty.Register("InnerText", typeof(string), typeof(JarListEntry));

        public JarListEntry(MainWindow caller, JarEntry jar, bool downloadIncomplete = false)
        {
            this.caller = caller;
            InitializeComponent();
            this.jar = jar;

            bool hasFriendlyName = !String.IsNullOrEmpty(jar.friendlyName);
            string mtext = jar.jarFileName + (downloadIncomplete ? GlobalVars.L.Translate("ui.jarlist.download_in_progress") : "");
            if (hasFriendlyName)
            {
                mtext = jar.friendlyName;
                this.Height = 38;
                subText.Visibility = Visibility.Visible;
                subText.Text = jar.jarFileName;
            }
            SetValue(InnerTextProperty, mtext);
            this.mainText.Text = mtext;
            if (jar.category != null)
            {
                byte[] argbCatColor = Util.hexStringToAARRGGBBBytes(jar.category.color);
                mainText.Foreground = new SolidColorBrush(Color.FromArgb(argbCatColor[0], argbCatColor[1], argbCatColor[2], argbCatColor[3]));
            }
        }

        public void RenameJar()
        {
            //todo: replace this visualbasic lmao
            string target = PopupTextBox.ShowNewPopup(GlobalVars.L.Translate("ui.jarlist.rename", Util.CleanStringForXAML(jar.jarFileName)), "DECRAFT", jar.jarFileName.Substring(0, jar.jarFileName.Length-4));
            string newJarName = $"{MainWindow.jarDir}/{target}.jar";
            string newJarConfName = $"{MainWindow.configDir}/{target}.jar.xml";
            if (target != "" && !File.Exists(newJarName))
            {
                File.Move($"{MainWindow.jarDir}/{jar.jarFileName}", newJarName);
                if (File.Exists(newJarConfName))
                {
                    File.Delete(newJarConfName);
                }
                File.Move($"{MainWindow.configDir}/{jar.jarFileName}.xml", newJarConfName);
            }
        }

        public void SetFriendlyName()
        {
            string target = PopupTextBox.ShowNewPopup(GlobalVars.L.Translate("ui.jarlist.set_friendly_name", Util.CleanStringForXAML(jar.jarFileName)), "DECRAFT", jar.friendlyName);
            Console.WriteLine(target);
            jar.friendlyName = target;
            caller.SaveRuntimeConfig();
            caller.ResetJarlist();
        }

        public void DeleteJar()
        {
            if (!(from y in caller.currentScanThreads
                  where y.jar == this.jar.jarFileName
                  select y).Any())
            {
                if (PopupYesNo.ShowNewPopup(GlobalVars.L.Translate("ui.jarlist.delete", Util.CleanStringForXAML(jar.jarFileName)), "DECRAFT") == MessageBoxResult.Yes)
                {
                    string path = $"{MainWindow.jarDir}/{jar.jarFileName}";
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            } else
            {
                PopupOK.ShowNewPopup(GlobalVars.L.Translate("ui.jarlist.hint_jar_is_being_scanned"), "DECRAFT");
            }
        }

        private void ContextSetFriendlyName_Click(object sender, RoutedEventArgs e)
        {
            SetFriendlyName();
            ctxMenu.IsOpen = false;
        }        
        
        private void ContextRename_Click(object sender, RoutedEventArgs e)
        {
            RenameJar();
            ctxMenu.IsOpen = false;
        }

        private void ContextDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteJar();
            ctxMenu.IsOpen = false;
        }

        private void ContextShowInExplorer_Click(object sender, RoutedEventArgs e)
        {

            Process.Start("explorer", $"/select,\"{Path.GetFullPath($"{MainWindow.jarDir}/{jar.jarFileName}")}\"");
            ctxMenu.IsOpen = false;
        }
        
        private void ContextSetCategory_Click(object sender, RoutedEventArgs e)
        {
            new WindowSetCategory(caller, jar).Show();
            ctxMenu.IsOpen = false;
        }

        private void ContextDeployToMobile_Click(object sender, RoutedEventArgs e)
        {
            new WindowDeployMTP(jar).Show();
            ctxMenu.IsOpen = false;
        }

        private void ContextOpenRETool_Click(object sender, RoutedEventArgs e)
        {
            new WindowRETool($"{MainWindow.jarDir}/{jar.jarFileName}").Show();
            ctxMenu.IsOpen = false;
        }
    }
}
