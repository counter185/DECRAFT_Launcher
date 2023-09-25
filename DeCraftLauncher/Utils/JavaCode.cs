using DeCraftLauncher.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher.Utils
{
    public static class JavaCode
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
                                    //3: 1_6_has_been_released.flag
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
                    //todo: handle cloak url
                    //   http://skins.minecraft.net/MinecraftCloaks/(whatever player name).png

                    if (strippedConnUrl.startsWith(""minecraft.net/skin/"")){{
                        strippedConnUrl = strippedConnUrl.substring({"minecraft.net/skin/".Length});
                    }}
                    else if (strippedConnUrl.startsWith(""skins.minecraft.net/MinecraftSkins/"")){{
                        strippedConnUrl = strippedConnUrl.substring({"skins.minecraft.net/MinecraftSkins/".Length});
                    }}
                    strippedConnUrl = ""{jar.appletSkinRedirectPath.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n")}/"" + strippedConnUrl;
                    System.out.println(""[InjectedUrlConnection(""+connectionType+"")] requested skin from path: "" + strippedConnUrl);
                    break;
                case 2:
                    break;
                case 3:
                    injectedDataStream = new ByteArrayInputStream(""https://github.com/counter185/DECRAFT_Launcher"".getBytes());
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

        final boolean LOG_ALL_HTTP_CONNECTIONS = {(jar.appletLogHTTPRequests ? "true" : "false")};
        final boolean IS_ONEPOINTSIX_HERE_YET = {(jar.appletIsOnePointSixHereYet ? "true" : "false")};
        final boolean INJECT_SKIN_REQUESTS = {(jar.appletRedirectSkins ? "true" : "false")};
        String protocol;

        public InjectedURLStreamHandler(String protocol){{
            this.protocol = protocol;
        }}
        @Override
        protected URLConnection openConnection(URL u) throws IOException {{
            //System.out.println(""[InjectedURLStreamHandler] url:"" + u);
            //System.out.println(""[InjectedURLStreamHandler] path:"" + u.getPath());
            if (u.toString().contains(""?n=""))
            {{
                return new InjectedURLConnection(u, 0);
            }} else if ((u.toString().contains(""minecraft.net/skin/"")
                        || u.toString().contains(""skins.minecraft.net/MinecraftSkins/""))
                        && INJECT_SKIN_REQUESTS) 
            {{
                return new InjectedURLConnection(u, 1);
            }} else if (u.toString().contains(""assets.minecraft.net/1_6_has_been_released.flag"")
                        && IS_ONEPOINTSIX_HERE_YET) 
            {{
                return new InjectedURLConnection(u, 3);
            }} else {{
                if (LOG_ALL_HTTP_CONNECTIONS) {{
                    System.out.println(""[InjectedURLStreamHandler] url: "" + u + "" , path: "" + u.getPath());
                    //new Exception().printStackTrace();
                }}
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
                    return ""{Util.CleanStringForJava(x.Value)}"";
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
                    return new URL(""{Util.CleanStringForJava(jar.documentBaseUrl)}"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public URL getCodeBase() {{
				try {{
                    return new URL(""{Util.CleanStringForJava(jar.documentBaseUrl)}"");
		        }} catch (MalformedURLException e) {{
			        return null;
		        }}
			}}

			@Override
			public String getParameter(String name) {{
				if (name.equals(""username"")){{
                    return ""{Util.CleanStringForJava(jar.playerName)}"";
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
        
        public static string GenerateMainFunctionWrapperCode(string className, JarConfig jar)
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

public class MainFunctionWrapper {{

    public static void main(String[] args){{
        {(jar.appletEmulateHTTP ? "" : "//")}URL.setURLStreamHandlerFactory(new InjectedStreamHandlerFactory());
        {className}.main(args);
    }}
}}
";
        }
    }
}
