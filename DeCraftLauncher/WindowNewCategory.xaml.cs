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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy WindowNewCategory.xaml
    /// </summary>
    public partial class WindowNewCategory : AcrylicWindow
    {
        public WindowNewCategory()
        {
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
        }
    }
}
