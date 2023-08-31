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
        public static string GenerateHTTPStreamInjectorCode()
        {
            return $@"
package decraft_internal;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLStreamHandler;
import java.net.URLStreamHandlerFactory;

public class InjectedStreamHandlerFactory implements URLStreamHandlerFactory {{

    class InjectedURLConnection extends HttpURLConnection {{
        URL thisUrl;
        InputStream injectedDataStream;

        protected InjectedURLConnection(URL url) {{
            super(url);
            injectedDataStream = new ByteArrayInputStream(new byte[] {{0x30, 0x0A}});
            thisUrl = url;
        }}

        @Override
        public void connect() throws IOException {{
            System.out.println(""[InjectedURLConnection] connect() url:"" + url);
        }}

        @Override
        public InputStream getInputStream(){{
            System.out.println(""[InjectedURLConnection] getInputStream() url:"" + url);
            return injectedDataStream;
        }}

        @Override
        public void disconnect() {{
        }}

        @Override
        public boolean usingProxy() {{
            return false;
        }}
        
    }}

    class InjectedURLStreamHandler extends URLStreamHandler {{

        String protocol;

        public InjectedURLStreamHandler(String protocol){{
            this.protocol = protocol;
        }}
        @Override
        protected URLConnection openConnection(URL u) throws IOException {{
            //System.out.println(""[InjectedURLStreamHandler] url:"" + u);
            //System.out.println(""[InjectedURLStreamHandler] path:"" + u.getPath());
            if (u.toString().contains(""?n="")){{
                return new InjectedURLConnection(u);
            }} else {{
                return new URL(null, u.toString(), new sun.net.www.protocol.http.Handler()).openConnection();
            }}
        }}

    }}

	public InjectedStreamHandlerFactory(){{
	}}

	@Override
	public URLStreamHandler createURLStreamHandler(String protocol) {{
        if (protocol.equals(""http"")){{
		    return new InjectedURLStreamHandler(protocol);
        }}
        return null;
	}}
}}

";
        }

        public static string GenerateAppletWrapperCode(string className, JarConfig jar)
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

    static JFrame mainFrame;    

    public static void main(String[] args){{
        //System.setSecurityManager(null);
        URL.setURLStreamHandlerFactory(new InjectedStreamHandlerFactory());
        mainFrame = new JFrame(""DECRAFT Applet Wrapper: {className}"");
        {className} a = new {className}();
        mainFrame.add(a);
        mainFrame.setSize({jar.windowW}, {jar.windowH});
        mainFrame.setVisible(true);
        mainFrame.setDefaultCloseOperation(WindowConstants.EXIT_ON_CLOSE);

        a.setStub(new AppletStub() {{

			@Override
			public boolean isActive() {{
				return true;
			}}

			@Override
			public URL getDocumentBase() {{
				try {{
                    return new URL(""http://www.minecraft.net/"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public URL getCodeBase() {{
				try {{
			        //return new URL(""http://localhost:0"");
                    return new URL(""http://www.minecraft.net/"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public String getParameter(String name) {{
				if (name.equals(""username"")){{
                    return ""{jar.playerName.Replace("\"", "\\\"").Replace("\n", "_")}"";
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
				mainFrame.setSize(width, height);
				
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
            File.WriteAllText("./java_temp/AppletWrapper.java", GenerateAppletWrapperCode(className, jar));
            File.WriteAllText("./java_temp/InjectedStreamHandlerFactory.java", GenerateHTTPStreamInjectorCode());
            List<string> compilerOut = JarUtils.RunProcessAndGetOutput(MainWindow.javaHome + "javac", $"-cp {MainWindow.jarDir}/{jar.jarFileName} ./java_temp/AppletWrapper.java ./java_temp/InjectedStreamHandlerFactory.java -d ./java_temp "
                + "--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED ");
            Console.WriteLine("Compilation log:");
            foreach (string a in compilerOut)
            {
                Console.WriteLine(a);
            }

            MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jar.instanceDirName);
            string args = "";
            args += "-cp ";
            args += "./java_temp/;";
            args += MainWindow.jarDir + "/" + jar.jarFileName;
            args += $";./lwjgl/{jar.LWJGLVersion}/* ";
            args += $"-Djava.library.path=lwjgl/{jar.LWJGLVersion}/native ";
            args += jar.jvmArgs + " ";
            if (jar.proxyHost != "")
            {
                args += $"-Dhttp.proxyHost={jar.proxyHost.Replace(" ", "%20")} ";
            }
            args += "--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED ";
            args += "decraft_internal.AppletWrapper";
            //args += " " + jar.playerName + " 0";
            Console.WriteLine("[LaunchAppletWrapper] Running command: java " + args);

            Process nproc = JarUtils.RunProcess(MainWindow.javaHome + "java", args, MainWindow.instanceDir + "/" + jar.instanceDirName);
            new ProcessLog(nproc).Show();


        }
    }
}
