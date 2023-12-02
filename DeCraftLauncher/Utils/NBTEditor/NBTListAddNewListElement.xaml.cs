using DeCraftLauncher.NBTReader;
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
using static DeCraftLauncher.NBTReader.NBTData;

namespace DeCraftLauncher.Utils.NBTEditor
{
    /// <summary>
    /// Logika interakcji dla klasy NBTListAddNewListElement.xaml
    /// </summary>
    public partial class NBTListAddNewListElement : UserControl
    {
        NBTListUIElement parent;

        public NBTListAddNewListElement(NBTListUIElement parent)
        {
            this.parent = parent;
            InitializeComponent();
        }

        private void label_addnew_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            NBTTagListNode listNode = (NBTData.NBTTagListNode)parent.targetNode;
            NBTBase listAdd = null;
            switch (listNode.innerType)
            {
                case 1:
                    listAdd = new NBTNode<byte>();
                    break;
                case 2:
                    listAdd = new NBTNode<short>();
                    break;
                case 3:
                    listAdd = new NBTNode<int>();
                    break;
                case 4:
                    listAdd = new NBTNode<long>();
                    break;
                case 5:
                    listAdd = new NBTNode<float>();
                    break;
                case 6:
                    listAdd = new NBTNode<double>();
                    break;
                case 8:
                    listAdd = new NBTNode<string>();
                    break;
                case 10:
                    listAdd = new NBTTagCompoundNode();
                    break;
                default:
                    throw new NotImplementedException();
            }
            listAdd.Tag = listNode.innerType;
            listNode.Value.Add(listAdd);
            parent.PopulateNBTChildren();
        }
    }
}
