using DeCraftLauncher.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static DeCraftLauncher.JarUtils;

namespace DeCraftLauncher
{
    public class AppletWrapper
    {
        public static string GenerateHTTPStreamInjectorCode(JarConfig jar)
        {
            return $@"
package decraft_internal;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.FileNotFoundException;
import java.io.InputStream;
import java.io.FileInputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLStreamHandler;
import java.net.URLStreamHandlerFactory;

public class InjectedStreamHandlerFactory implements URLStreamHandlerFactory {{

    class InjectedURLConnection extends HttpURLConnection {{
        URL thisUrl;
        InputStream injectedDataStream;
        int connectionType = 0;     //0: login
                                    //1: skin server
                                    //2: resource server
        String strippedConnUrl = """";

        protected InjectedURLConnection(URL url, int connectionType) {{
            super(url);
            this.connectionType = connectionType;
            strippedConnUrl = url+"""";
            if (strippedConnUrl.startsWith(""http://"")){{
                strippedConnUrl = strippedConnUrl.substring(7);
            }}
            if (strippedConnUrl.startsWith(""www."")){{
                strippedConnUrl = strippedConnUrl.substring(4);
            }}
            switch (this.connectionType){{
                case 0:
                    injectedDataStream = new ByteArrayInputStream(new byte[] {{0x30, 0x0A}});
                    break;
                case 1:
                    if (strippedConnUrl.startsWith(""minecraft.net/skin/"")){{
                        strippedConnUrl = strippedConnUrl.substring(19);
                    }}
                    strippedConnUrl = ""{jar.appletSkinRedirectPath.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")}/"" + strippedConnUrl;
                    System.out.println(""[InjectedUrlConnection(""+connectionType+"")] requested skin from path: "" + strippedConnUrl);
                    break;
                case 2:
                    break;
            }}
            thisUrl = url;
        }}

        @Override
        public void connect() throws IOException {{
            System.out.println(""[InjectedURLConnection(""+connectionType+"")] connect() url:"" + url);
            if (connectionType == 1){{
                try {{
                    injectedDataStream = new FileInputStream(strippedConnUrl);
                }} catch (FileNotFoundException e) {{
                    System.out.println(""[InjectedURLConnection(""+connectionType+"")] Skin file not found"");
                    throw new IOException();
                }}
            }}
        }}

        @Override
        public InputStream getInputStream(){{
            System.out.println(""[InjectedURLConnection(""+connectionType+"")] getInputStream() url:"" + url);
            return injectedDataStream;
        }}

        @Override
        public void disconnect() {{
            try {{
                if (injectedDataStream != null) {{
                    injectedDataStream.close();
                }}
            }} catch (IOException e) {{ }}
        }}

        @Override
        public boolean usingProxy() {{
            return false;
        }}
        
    }}

    class InjectedURLStreamHandler extends URLStreamHandler {{

        final boolean DEBUG_ALL_HTTP_CONNECTIONS = true;
        final boolean INJECT_SKIN_REQUESTS = {(jar.appletRedirectSkins ? "true" : "false")};
        String protocol;

        public InjectedURLStreamHandler(String protocol){{
            this.protocol = protocol;
        }}
        @Override
        protected URLConnection openConnection(URL u) throws IOException {{
            //System.out.println(""[InjectedURLStreamHandler] url:"" + u);
            //System.out.println(""[InjectedURLStreamHandler] path:"" + u.getPath());
            if (u.toString().contains(""?n="")){{
                return new InjectedURLConnection(u, 0);
            }} else if (u.toString().contains(""minecraft.net/skin/"") && INJECT_SKIN_REQUESTS) {{
                return new InjectedURLConnection(u, 1);
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

        public static string GenerateAppletWrapperCode(string className, JarConfig jar, Dictionary<string, string> appletParameters)
        {
            string additionalParameters = "";

            (from x in appletParameters
            select $@"
                else if (name.equals(""{x.Key}"")){{
                    return ""{x.Value.Replace("\"", "\\\"").Replace("\n", "_")}"";
                }}{'\n'}").ToList().ForEach((condition) =>
            {
                additionalParameters += condition;
            });

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
        {(jar.appletEmulateHTTP ? "" : "//")}URL.setURLStreamHandlerFactory(new InjectedStreamHandlerFactory());
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
                    return new URL(""{jar.documentBaseUrl.Replace("\"", "\\\"").Replace('\n', '_')}"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public URL getCodeBase() {{
				try {{
                    return new URL(""{jar.documentBaseUrl.Replace("\"", "\\\"").Replace('\n', '_')}"");
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
                else if (name.equals(""haspaid"")){{
                    return ""true"";
                }}
                {additionalParameters}
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

        public static void LaunchAppletWrapper(string className, JarConfig jar, Dictionary<string, string> appletParameters)
        {
            MainWindow.EnsureDir("./java_temp");
            File.WriteAllText("./java_temp/AppletWrapper.java", GenerateAppletWrapperCode(className, jar, appletParameters));
            if (jar.appletEmulateHTTP)
            {
                File.WriteAllText("./java_temp/InjectedStreamHandlerFactory.java", GenerateHTTPStreamInjectorCode(jar));
            }
            List<string> compilerOut = JarUtils.RunProcessAndGetOutput(MainWindow.mainRTConfig.javaHome + "javac", $"-cp \"{MainWindow.jarDir}/{jar.jarFileName}\" " +
                $"./java_temp/AppletWrapper.java " +
                (jar.appletEmulateHTTP ? $"./java_temp/InjectedStreamHandlerFactory.java " : "") +
                $"-d ./java_temp " +
                (jar.appletEmulateHTTP && MainWindow.mainRTConfig.isJava9 ? "--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED " : ""));
            Console.WriteLine("Compilation log:");
            foreach (string a in compilerOut)
            {
                Console.WriteLine(a);
            }

            MainWindow.EnsureDir(MainWindow.instanceDir + "/" + jar.instanceDirName);
            string args = "";
            args += "-cp ";
            //todo: make this cleaner (preferrably without getting rid of relative paths)
            args += "\"../../java_temp/";
            if (jar.LWJGLVersion != "+ built-in")
            {
                args += $";../../lwjgl/{jar.LWJGLVersion}/*";
            }
            args += $";../../{MainWindow.jarDir}/{jar.jarFileName}\" ";
            
            args += $"-Djava.library.path=../../lwjgl/{(jar.LWJGLVersion == "+ built-in" ? "_temp_builtin" : jar.LWJGLVersion)}/native ";
            args += jar.jvmArgs + " ";
            if (jar.proxyHost != "")
            {
                args += $"-Dhttp.proxyHost={jar.proxyHost.Replace(" ", "%20")} ";
            }
            if (jar.appletEmulateHTTP && MainWindow.mainRTConfig.isJava9)
            {
                args += "--add-exports java.base/sun.net.www.protocol.http=ALL-UNNAMED ";
            }
            args += "decraft_internal.AppletWrapper ";
            if (jar.gameArgs != "")
            {
                args += jar.gameArgs;
            }
            //args += " " + jar.playerName + " 0";
            Console.WriteLine("[LaunchAppletWrapper] Running command: java " + args);

            Process nproc = null;

            Directory.SetCurrentDirectory(Path.GetFullPath($"{MainWindow.currentDirectory}/{MainWindow.instanceDir}/{jar.instanceDirName}"));
            try
            {
                nproc = JarUtils.RunProcess(MainWindow.mainRTConfig.javaHome + "java", args, Path.GetFullPath("."));
            } catch (Win32Exception w32e)
            {
                MessageBox.Show($"Error launching java process: {w32e.Message}\n\nVerify that Java is installed in \"Runtime settings\".");
            }
            Directory.SetCurrentDirectory(MainWindow.currentDirectory);
            if (nproc != null)
            {
                new ProcessLog(nproc).Show();
            }


        }

        public static void TryLaunchAppletWrapper(string classpath, JarConfig jarConfig, Dictionary<string, string> appletParameters = null)
        {
            if (!classpath.Contains('.'))
            {
                MessageBox.Show("Launching default package applets is not implemented.", "DECRAFT");
            }
            else
            {
                try
                {
                    AppletWrapper.LaunchAppletWrapper(classpath, jarConfig, appletParameters != null ? appletParameters : new Dictionary<string, string>());
                }
                catch (Win32Exception)
                {
                    MessageBox.Show("Applet wrapper requires JDK installed.");
                }
            }
        }
    }
}
