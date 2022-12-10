using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeCraftLauncher.JarUtils;

namespace DeCraftLauncher
{
    public class Test
    {
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assertion failed.");
            }
        } 

        public static void TestXMLSaveLoad()
        {
            JarConfig a = new JarConfig("a1.0.16.jar");
            a.LWJGLVersion = "2.9.3";
            a.playerName = "cntrpl";
            a.friendlyName = "Alpha 1.0.16";
            a.entryPointsScanned = true;
            a.entryPoints.Add(new EntryPoint("net.minecraft.client.Minecraft", EntryPointType.STATIC_VOID_MAIN));
            a.entryPoints.Add(new EntryPoint("net.minecraft.isom.IsomPreviewApplet", EntryPointType.APPLET));

            a.SaveToXML("a1.0.16.xml");
            JarConfig b = JarConfig.LoadFromXML("a1.0.16.xml", "a1.0.16.jar");

            Assert(a.LWJGLVersion == b.LWJGLVersion);
            Assert(a.playerName == b.playerName);
            Assert(a.friendlyName == b.friendlyName);

            Console.WriteLine("TestXMLSaveLoad pass");

        }
    }
}
