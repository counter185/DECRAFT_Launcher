using DeCraftLauncher.Utils;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
            int trueVal = 3;
            int trueVal2 = 1;
            IntPtr wHandle = new WindowInteropHelper(this).EnsureHandle();
            Util.DwmSetWindowAttribute(wHandle, 20, ref trueVal2, sizeof(uint));
            Util.DwmSetWindowAttribute(wHandle, 38, ref trueVal, sizeof(uint));

        }
    }
}
