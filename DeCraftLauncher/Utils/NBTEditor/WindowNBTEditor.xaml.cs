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
using System.Windows.Shapes;
using DeCraftLauncher.NBTReader;
using SourceChord.FluentWPF;

namespace DeCraftLauncher.Utils.NBTEditor
{
    /// <summary>
    /// Logika interakcji dla klasy WindowNBTEditor.xaml
    /// </summary>
    public partial class WindowNBTEditor : AcrylicWindow
    {
        public string NBTPath;
        public NBTData readNBT;


        public WindowNBTEditor(string path)
        {
            this.NBTPath = path;
            readNBT = NBTData.FromFile(path);
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);

            panel_nbtdata.Children.Add(new NBTListUIElement(readNBT.rootNode, "<root node>"));
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            readNBT.ToFile(NBTPath + "2");
        }
    }
}
