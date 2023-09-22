using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using DeCraftLauncher.Utils;
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
    /// Logika interakcji dla klasy WindowSetCategory.xaml
    /// </summary>
    public partial class WindowSetCategory : AcrylicWindow
    {
        MainWindow caller;
        JarEntry target;

        public WindowSetCategory(MainWindow caller, JarEntry target)
        {
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
            this.caller = caller;
            this.target = target;
            UpdateCategories();
        }

        void UpdateCategories()
        {
            listbox_categories.Items.Clear();
            foreach (Category a in MainWindow.mainRTConfig.jarCategories) {
                TextBlock nTextBlock = new TextBlock();
                nTextBlock.Text = a.name;
                nTextBlock.Foreground = Util.hexStringToAARRGGBBBrush(a.color, Brushes.White);
                Category targetCat = a;
                nTextBlock.MouseDown += delegate
                {
                    target.category = targetCat;
                    caller.SaveRuntimeConfig();
                    caller.ResetJarlist();
                    this.Close();
                };
                listbox_categories.Items.Add(nTextBlock);
            }
        }

        private void btn_categorynew_Click(object sender, RoutedEventArgs e)
        {
            WindowNewCategory nWd = new WindowNewCategory(caller);
            nWd.Closed += delegate
            {
                UpdateCategories();
            };
            nWd.Show();
        }
    }
}
