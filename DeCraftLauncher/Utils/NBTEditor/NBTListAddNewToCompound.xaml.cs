using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeCraftLauncher.Utils.NBTEditor
{
    /// <summary>
    /// Logika interakcji dla klasy NBTListAddNewToCompound.xaml
    /// </summary>
    public partial class NBTListAddNewToCompound : UserControl
    {
        NBTListUIElement parent;
        public NBTListAddNewToCompound(NBTListUIElement parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void label_addnew_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            new WindowNBTAddToCompound(parent).Show();
        }
    }
}
