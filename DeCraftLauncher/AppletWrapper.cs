using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DeCraftLauncher.JarUtils;

namespace DeCraftLauncher
{
    public class AppletWrapper
    {
        public static string GenerateAppletWrapperCode(string className)
        {
            return 
                $@"
package decraft_internal;

import java.applet.Applet;
import java.applet.AppletContext;
import java.applet.AppletStub;
import java.net.MalformedURLException;
import java.net.URL;
import javax.swing.JFrame;
import javax.swing.WindowConstants;
import {className};

public class AppletWrapper {{
    public static void main(String[] args){{
        //System.setSecurityManager(null);
        JFrame frame = new JFrame(""DECRAFT Applet Wrapper: {className}"");
        {className} a = new {className}();
        frame.add(a);
        frame.setSize(960, 540);
        frame.setVisible(true);
        frame.setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);

        a.setStub(new AppletStub() {{

			@Override
			public boolean isActive() {{
				return true;
			}}

			@Override
			public URL getDocumentBase() {{
				try {{
			        return new URL(""http://localhost:0"");
                    //return new URL(""http://minecraft.net"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public URL getCodeBase() {{
				try {{
			        return new URL(""http://localhost:0"");
                    //return new URL(""http://minecraft.net"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public String getParameter(String name) {{
				if (name.equals(""username"")){{
                    return ""AppletWrapperPlayer"";
                }}
                else if (name.equals(""sessionid"")){{
                    return ""0"";
                }}
                else if (name.equals(""mppass"")){{
                    return ""password"";
                }}
				return null;
			}}

			@Override
			public AppletContext getAppletContext() {{
				// TODO Auto-generated method stub
				return null;
			}}

			@Override
			public void appletResize(int width, int height) {{
				// TODO Auto-generated method stub
				
			}}
			
		}});

        a.init();
        a.start();
    }}
}}
";
        }

        public static void LaunchAppletWrapper(string className, JarConfig jar)
        {
            MainWindow.EnsureDir("./java_temp");
            File.WriteAllText("./java_temp/AppletWrapper.java", GenerateAppletWrapperCode(className));
            List<string> compilerOut = JarUtils.RunProcessAndGetOutput(MainWindow.javaHome + "javac", $"-cp {MainWindow.jarDir}/{jar.jarFileName} ./java_temp/AppletWrapper.java -d ./java_temp");
            /*foreach (string a in compilerOut)
            {
                Console.WriteLine(a);
            }*/

            MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jar.instanceDirName);
            string args = "";
            args += "-cp ";
            args += "./java_temp/;";
            args += MainWindow.jarDir + "/" + jar.jarFileName;
            args += ";./lwjgl/2.9.3/* ";
            args += "-Djava.library.path=lwjgl/2.9.3/native ";
            args += "decraft_internal.AppletWrapper";
            //args += " " + jar.playerName + " 0";
            Console.WriteLine("[LaunchAppletWrapper] Running command: java " + args);

            Process nproc = JarUtils.RunProcess(MainWindow.javaHome + "java", args, MainWindow.instanceDir + "/" + jar.instanceDirName);
            new ProcessLog(nproc).Show();


        }
    }
}
