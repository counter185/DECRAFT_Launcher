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
        private T value;

        public T Value
        {
            get => this.value;
            set => this.value = value;
        }

        public ReferenceType(T value)
        {
            this.value = value;
        }
    }
}
