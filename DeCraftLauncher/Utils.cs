using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

        public static short StreamReadShort(Stream input)
        {
            byte[] buffer = new byte[2];
            input.Read(buffer, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToInt16(buffer, 0);
        }        
        public static int StreamReadInt(Stream input)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToInt32(buffer, 0);
        }
        public static long StreamReadLong(Stream input)
        {
            byte[] buffer = new byte[8];
            input.Read(buffer, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
