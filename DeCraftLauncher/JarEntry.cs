using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher
{
    public class JarEntry
    {
        public string jarFileName;
        public string friendlyName = "";
        public Category category = null;

        public JarEntry(string jarFileName)
        {
            this.jarFileName = jarFileName;
        }
    }
}
