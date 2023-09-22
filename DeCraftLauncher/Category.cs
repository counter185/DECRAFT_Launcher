using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher
{
    public class Category
    {
        public string name;
        public UInt32 color;

        public Category(string name, uint color)
        {
            this.name = name;
            this.color = color;
        }
    }
}
