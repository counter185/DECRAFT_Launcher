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
            window.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(setTransparent ? (byte)0xA0 : (byte)0xF0,0,0,0));
            if (setTransparent)
            {
                window.Opacity = 0.85;
                window.TintOpacity = 0.1;
            }
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

        static readonly Dictionary<int, string> JavaMajorVersionsToNames = new Dictionary<int, string>()
        {
            {45, "JDK1.1"},
            {46, "JDK1.2"},
            {47, "JDK1.3"},
            {48, "JDK1.4"},
            {49, "Java5"},
            {50, "Java6"},
        };

        public static string JavaVersionFriendlyName(string majorDotMinorVer)
        {
            try
            {
                string ver = majorDotMinorVer.Split('.')[0];
                int v = int.Parse(ver);
                if (v >= 51)
                {
                    return $"{majorDotMinorVer} (Java{7+v-51}{(v > 65 ? "?" : "")})";
                }
                else if (v >= 45)
                {
                    return $"{majorDotMinorVer} ({JavaMajorVersionsToNames[v]})";
                }
                else
                {
                    return $"{majorDotMinorVer} (JDK <1.1?)";
                }
            }
            catch (Exception)
            {
                return majorDotMinorVer;
            }
        }
    }
}
