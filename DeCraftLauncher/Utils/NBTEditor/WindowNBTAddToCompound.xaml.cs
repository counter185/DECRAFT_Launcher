using DeCraftLauncher.NBTReader;
using SourceChord.FluentWPF;
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
using static DeCraftLauncher.NBTReader.NBTData;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace DeCraftLauncher.Utils.NBTEditor
{
    /// <summary>
    /// Logika interakcji dla klasy WindowNBTAddToCompound.xaml
    /// </summary>
    public partial class WindowNBTAddToCompound : AcrylicWindow
    {
        NBTListUIElement parent;
        public WindowNBTAddToCompound(NBTListUIElement target)
        {
            this.parent = target;
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);

        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            byte[] comboboxToTag = new byte[] { 1,2,3,4,5,6,8,10};
            NBTTagCompoundNode listNode = (NBTTagCompoundNode)parent.targetNode;
            NBTBase listAdd;
            if (checkbox_islist.IsChecked == true)
            {
                listAdd = new NBTTagListNode
                {
                    innerType = comboboxToTag[cbox_type.SelectedIndex],
                    Tag = 9,
                };
            }
            else
            {
                switch (cbox_type.SelectedIndex)
                {
                    case 0:
                        listAdd = new NBTNode<byte>();
                        break;
                    case 1:
                        listAdd = new NBTNode<short>();
                        break;
                    case 2:
                        listAdd = new NBTNode<int>();
                        break;
                    case 3:
                        listAdd = new NBTNode<long>();
                        break;
                    case 4:
                        listAdd = new NBTNode<float>();
                        break;
                    case 5:
                        listAdd = new NBTNode<double>();
                        break;
                    case 6:
                        listAdd = new NBTNode<string>();
                        break;
                    case 7:
                        listAdd = new NBTTagCompoundNode();
                        break;
                    default:
                        throw new NotImplementedException();
                }
                listAdd.Tag = comboboxToTag[cbox_type.SelectedIndex];
            }
            listAdd.Name = tbox_name.Text;
            listNode.Value.Add(listAdd);
            parent.PopulateNBTChildren();
            Close();
        }
    }
}
