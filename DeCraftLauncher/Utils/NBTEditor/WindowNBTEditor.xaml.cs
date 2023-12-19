using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DeCraftLauncher.NBTReader;
using DeCraftLauncher.UIControls.Popup;
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
        NBTListUIElement root;

        public WindowNBTEditor(string path)
        {
            this.NBTPath = path;
            readNBT = NBTData.FromFile(path);
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);

            root = new NBTListUIElement(readNBT.rootNode, null, "<root node>");
            panel_nbtdata.Children.Add(root);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            readNBT.ToFile(NBTPath);
            root.ResetModifiedStatus();
            PopupOK.ShowNewPopup($"Saved NBT to {NBTPath}", "DECRAFT");
        }
             
        private void btn_saveas_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Compressed NBT file|*.dat|Raw NBT File|*.nbt|All files|*.*";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "DECRAFT: Save NBT file";
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                readNBT.ToFile(saveFileDialog.FileName, saveFileDialog.FileName.EndsWith(".dat"));
                root.ResetModifiedStatus();
                PopupOK.ShowNewPopup($"Saved NBT to {saveFileDialog.FileName}", "DECRAFT");
            }
            
        }
    }
}
