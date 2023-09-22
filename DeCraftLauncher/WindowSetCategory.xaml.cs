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
            this.caller = caller;
            this.target = target;
        }
    }
}
