using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DECRAFTModdingEnvironment
{
    public class WorkspaceConfig
    {
        public string jdkPath;
        public string workspaceName = "New DME Workspace";
        public string versionName;

        public static WorkspaceConfig LoadFromXML(string path)
        {
            WorkspaceConfig config = new WorkspaceConfig();

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(path);
            XmlNode root = xdoc.DocumentElement;
            config.jdkPath = root.SelectSingleNode("JDKPath").InnerText;
            config.workspaceName = root.SelectSingleNode("WorkspaceName").InnerText;
            config.versionName = root.SelectSingleNode("VersionName").InnerText;

            return config;

        }
    }
}
