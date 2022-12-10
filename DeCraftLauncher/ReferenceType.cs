using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace DeCraftLauncher
{
    public class ReferenceType<T>
    {
        public T value;

        public ReferenceType(T value)
        {
            this.value = value;
        }

        public void Set(T v)
        {
            value = v;
        }

        public T Get()
        {
            return value;
        }
    }
}
