﻿using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using PCInfo = Microsoft.VisualBasic.Devices.ComputerInfo;
using WinColor = System.Windows.Media.Color;

namespace DME.Utils
{
    public static class Util
    {

        const string OS_WIN10 = "windows 10";

        const byte TRANSPARENCY_ON = 0xA0;
        const byte TRANSPARENCY_OFF = 0xF0;

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode)]
        public static extern uint DwmSetWindowAttribute(IntPtr hwnd,
                                                uint attribute,
                                                ref uint pvAttribute,
                                                uint cbAttribute);
        [Flags]
        public enum DWM_BB
        {
            DWM_BB_ENABLE = 1,
            DWM_BB_BLURREGION = 2,
            DWM_BB_TRANSITIONONMAXIMIZED = 4
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;

            [MarshalAs(UnmanagedType.Bool)]
            public bool fEnable;

            [MarshalAs(UnmanagedType.SysUInt)]
            public IntPtr hRgnBlur;

            [MarshalAs(UnmanagedType.Bool)]
            public bool fTransitionOnMaximized;
        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOOWNERZORDER = 0x0200;

        public static void SetWindowDarkMode(IntPtr windowHandle)
        {
            uint val = 1;
            try
            {
                if (windowHandle != IntPtr.Zero)
                {
                    DwmSetWindowAttribute(windowHandle, 20, ref val, sizeof(uint));
                }
            } catch (COMException e)
            {
                Console.WriteLine($"Error setting dark mode: {e.Message}");
            }
        }

        public static void UpdateAcrylicWindowBackground(AcrylicWindow window)
        {
            string osName = new PCInfo().OSFullName;
            window.AcrylicWindowStyle = AcrylicWindowStyle.Normal;
            SourceChord.FluentWPF.ResourceDictionaryEx.GlobalTheme = SourceChord.FluentWPF.ElementTheme.Dark;
            if (Environment.OSVersion.Version.Major < 6 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 0)
                || osName.ToLower().Contains("windows 11"))
            {
                //COME ON FLUENTWPF
                //okay so for context for this
                //when you do fw:AcrylicWindow.Disable
                //it calls the private DisableBlur() function
                //which for some reason removes the action bar????????
                //https://github.com/sourcechord/FluentWPF/blob/9acc2519f36c60e830c4603d01363c5bfa8909b5/FluentWPF/AcrylicWindow.cs#L290
                var closeBinding = new CommandBinding(SystemCommands.CloseWindowCommand, (_, __) => { SystemCommands.CloseWindow(window); });
                var minimizeBinding = new CommandBinding(SystemCommands.MinimizeWindowCommand, (_, __) => { SystemCommands.MinimizeWindow(window); });
                var maximizeBinding = new CommandBinding(SystemCommands.MaximizeWindowCommand, (_, __) => { SystemCommands.MaximizeWindow(window); });
                var restoreBinding = new CommandBinding(SystemCommands.RestoreWindowCommand, (_, __) => { SystemCommands.RestoreWindow(window); });
                window.CommandBindings.Add(closeBinding);
                window.CommandBindings.Add(minimizeBinding);
                window.CommandBindings.Add(maximizeBinding);
                window.CommandBindings.Add(restoreBinding);
                if (osName.ToLower().Contains("windows 11"))
                {
                    //window.SetValue(AcrylicWindow.EnabledProperty, false);
                    window.Background = new SolidColorBrush(WinColor.FromArgb(TRANSPARENCY_ON, 0, 0, 0));
                    window.Opacity = 0.88;
                    window.TintOpacity = 0.1;
                    uint trueVal = 3;
                    uint trueVal2 = 1;
                    IntPtr wHandle = new WindowInteropHelper(window).EnsureHandle();
                    try
                    {
                        uint a1 = DwmSetWindowAttribute(wHandle, 20, ref trueVal2, sizeof(uint));
                        uint a2 = DwmSetWindowAttribute(wHandle, 38, ref trueVal, sizeof(uint));
                        if (DwmIsCompositionEnabled())
                        {
                            Util.DWM_BLURBEHIND nblurProperties = new Util.DWM_BLURBEHIND
                            {
                                dwFlags = Util.DWM_BB.DWM_BB_ENABLE,
                                fEnable = true,
                                hRgnBlur = IntPtr.Zero,
                                fTransitionOnMaximized = false,
                            };
                            DwmEnableBlurBehindWindow(wHandle, ref nblurProperties);
                        }
                    } catch (Exception)
                    {
                        //dwmapi error don't care
                    }
                }
            }
            else
            {
                //THERE REALLY IS NO CLEANER WAY OF DOING THIS.
                // Is there?
                window.SetValue(AcrylicWindow.EnabledProperty, true);
                
                bool setTransparent = osName.ToLower().Contains(OS_WIN10) 
                    || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 1);      //win7

                window.Background = new SolidColorBrush(WinColor.FromArgb(setTransparent ? TRANSPARENCY_ON : TRANSPARENCY_OFF, 0, 0, 0));
                if (setTransparent)
                {
                    window.Opacity = 0.88;
                    window.TintOpacity = 0.1;
                }

                IntPtr wHandle = new WindowInteropHelper(window).EnsureHandle();
                SetWindowDarkMode(wHandle);

                try
                {
                    if (DwmIsCompositionEnabled() && !osName.ToLower().Contains(OS_WIN10))
                    {
                        Util.DWM_BLURBEHIND nblurProperties = new Util.DWM_BLURBEHIND
                        {
                            dwFlags = Util.DWM_BB.DWM_BB_ENABLE,
                            fEnable = true,
                            hRgnBlur = IntPtr.Zero,
                            fTransitionOnMaximized = false,
                        };
                        DwmEnableBlurBehindWindow(wHandle, ref nblurProperties);
                    }
                } catch (Exception)
                {
                    //don't care dwmapi error
                }
            }
        }

        public static bool RunningOnWine()
        {
            return Process.GetProcessesByName("csrss").Length == 0;
        }

        const string NODETYPE_ELEMENT = "element";

        public static XmlNode GenElementChild(XmlDocument doc, string name, string value = "")
        {
            var result = doc.CreateNode(NODETYPE_ELEMENT, name, string.Empty);
            if (!string.IsNullOrEmpty(value))
                result.InnerText = value;
            return result;
        }

        public static string GetInnerOrDefault(XmlNode target, string property, string defaultValue = "", string enforceType = "string")
        {
            try
            {
                XmlNode node = target.SelectSingleNode(property);
                if (node == null)
                {
                    return defaultValue;
                }
                switch (enforceType)
                {
                    case "bool":
                        bool.Parse(node.InnerText);
                        break;
                    case "int":
                        int.Parse(node.InnerText);
                        break;
                    case "uint":
                        uint.Parse(node.InnerText);
                        break;
                }
                return node.InnerText;
            }
            catch (NullReferenceException)
            {
                return defaultValue;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        public static short StreamReadShort(Stream input, bool bigEndian = true)
        {
            byte[] buffer = new byte[2];
            input.Read(buffer, 0, 2);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToInt16(buffer, 0);
        }           
        public static void StreamWriteShort(Stream output, short a, bool bigEndian = true)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            output.Write(buffer, 0, 2);
        }        
        public static int StreamReadInt(Stream input, bool bigEndian = true)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToInt32(buffer, 0);
        }
        public static void StreamWriteInt(Stream output, int a, bool bigEndian = true)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            output.Write(buffer, 0, 4);
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
        public static void StreamWriteLong(Stream output, long a, bool bigEndian = true)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            if (BitConverter.IsLittleEndian == bigEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            output.Write(buffer, 0, 8);
        }

        public static float StreamReadFloat(Stream input)
        {
            byte[] buffer = new byte[4];
            input.Read(buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToSingle(buffer, 0);
        }
        public static void StreamWriteFloat(Stream output, float a)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            output.Write(buffer,0,4);
        }
        
        public static double StreamReadDouble(Stream input)
        {
            byte[] buffer = new byte[8];
            input.Read(buffer, 0, 8);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            return BitConverter.ToDouble(buffer, 0);
        }
        public static void StreamWriteDouble(Stream output, double a)
        {
            byte[] buffer = BitConverter.GetBytes(a);
            if (BitConverter.IsLittleEndian)
            {
                buffer = buffer.Reverse().ToArray();
            }
            output.Write(buffer, 0, 8);
        }

        static readonly string[] Java_MajorVersionNames =
        {
            "JDK1.1",
            "JDK1.2",
            "JDK1.3",
            "JDK1.4",
            "Java5",
            "Java6",
        };

        /// <summary>
        /// Gets a name for a specific Java <paramref name="version"/>
        /// </summary>
        /// <param name="version">The numeric version ID to get a name for</param>
        /// <returns>The name assigned to the specified <paramref name="version"/></returns>
        static string GetNameFromVersion(int version) => Java_MajorVersionNames[version - 45];

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
                    return $"{majorDotMinorVer} ({GetNameFromVersion(v)})";
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

        public static int TryParseJavaVersionString(string str)
        {
            try
            {
                string[] splitJDKVer = str.Split('.');
                string majorVersion = splitJDKVer[0];
                if (majorVersion == "1")
                {
                    majorVersion = splitJDKVer[1];
                }
                return int.Parse(majorVersion);
            }
            catch (Exception)
            {
            }
            return -1;
        }

        public static int TryParseJavaCVersionString(string str)
        {
            if (str.StartsWith("javac "))
            {
                try
                {
                    string[] splitJDKVer = str.Split(' ')[1].Split('.');
                    string majorVersion = splitJDKVer[0];
                    if (majorVersion == "1")
                    {
                        majorVersion = splitJDKVer[1];
                    }
                    return int.Parse(majorVersion);
                }
                catch (Exception)
                {
                }
            }
            return -1;
        }

        public static byte[] hexStringToAARRGGBBBytes(string a)
        {
            try
            {
                byte[] ret = new byte[4];
                ret[0] = Convert.ToByte(a.Substring(0, 2), 16);
                ret[1] = Convert.ToByte(a.Substring(2, 2), 16);
                ret[2] = Convert.ToByte(a.Substring(4, 2), 16);
                ret[3] = Convert.ToByte(a.Substring(6, 2), 16);
                return ret;
            } catch (Exception)
            {
                return null;
            }
        }

        public static System.Windows.Media.Brush hexStringToAARRGGBBBrush(string a, System.Windows.Media.Brush def = null)
        {
            byte[] col = hexStringToAARRGGBBBytes(a);
            if (col == null)
            {
                return def;
            } else
            {
                return new SolidColorBrush(WinColor.FromArgb(col[0], col[1], col[2], col[3]));
            }
        }

        public static string CleanStringForXAML(string a)
        {
            return a.Replace("_", "__");
        }

        public static string CleanStringForJava(string a)
        {
            return a.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n");
        }

        public static bool TryExtractPKFromExe(string filename, string targetfile)
        {
            using (FileStream inFile = File.OpenRead(filename))
            {
                byte[] searchHeader = new byte[] { (byte)'P', (byte)'K', 0x03, 0x04 };
                int searchProgress = 0;
                while (inFile.Position < inFile.Length)
                {
                    byte[] buffer = new byte[1];
                    inFile.Read(buffer, 0, 1);
                    if (buffer[0] == searchHeader[searchProgress])
                    {
                        if (++searchProgress == 4)
                        {
                            inFile.Position -= 4;
                            Console.WriteLine($"Found PK header at {inFile.Position}");
                            using (FileStream outFile = File.Open(targetfile, FileMode.Create, FileAccess.Write))
                            {
                                inFile.CopyTo(outFile);
                            }
                            return true;
                        }
                    }
                    else
                    {
                        searchProgress = 0;
                    }
                }
            }
            return false;
        }
    }
}
