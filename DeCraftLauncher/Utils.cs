using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DeCraftLauncher
{
    public class Utils
    {

        public static void UpdateAcrylicWindowBackground(AcrylicWindow window)
        {
            bool setTransparent = System.Environment.OSVersion.Version.Major == 10;
            window.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(setTransparent ? (byte)0x80 : (byte)0xF0,0,0,0));
        }
    }
}
