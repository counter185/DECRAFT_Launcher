using DeCraftLauncher.Utils;
using MediaDevices;
using Microsoft.VisualBasic;
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

        public JarListEntry(MainWindow caller, JarEntry jar)
        {
            this.caller = caller;
            InitializeComponent();
            this.jar = jar;

            bool hasFriendlyName = !String.IsNullOrEmpty(jar.friendlyName);
            string mtext = jar.jarFileName;
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
            string target = Interaction.InputBox($"Rename {jar.jarFileName}:", "DECRAFT", jar.jarFileName.Substring(0, jar.jarFileName.Length-4));
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
            string target = Interaction.InputBox($"Set friendly name of {jar.jarFileName}:", "DECRAFT", jar.friendlyName);
            Console.WriteLine(target);
            jar.friendlyName = target;
            caller.SaveRuntimeConfig();
            caller.ResetJarlist();
        }

        public void DeleteJar()
        {
            if (MessageBox.Show($"Delete {jar.jarFileName}?", "DECRAFT", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string path = $"{MainWindow.jarDir}/{jar.jarFileName}";
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
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
    }
}
