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
using System.Xml.Linq;
using static DeCraftLauncher.NBTReader.NBTData;

namespace DeCraftLauncher.Utils.NBTEditor
{
    /// <summary>
    /// Logika interakcji dla klasy NBTListUIElement.xaml
    /// </summary>
    public partial class NBTListUIElement : UserControl
    {

        public Brush GetTypeColor(byte type)
        {
            switch (type)
            {
                case 1: //byte
                    return Brushes.IndianRed;
                case 5: //float
                    return Brushes.Wheat;                
                case 6: //double
                    return Brushes.LightGreen;
                case 2: //short
                    return Brushes.LightPink;
                case 3: //int
                    return Brushes.LightSteelBlue;
                case 4: //long
                    return Brushes.Turquoise;
                case 8: //string
                    return Brushes.LightSlateGray;
                case 10: //compound
                    return Brushes.WhiteSmoke;
                default:
                    return Brushes.White;
            }
        }

        public NBTBase targetNode;
        public NBTListUIElement parentNode = null;

        public NBTListUIElement(NBTBase node, NBTListUIElement parent, string overrideName = null)
        {
            this.targetNode = node;
            this.parentNode = parent;
            InitializeComponent();

            label_tagvalue.Content = node.GetValue();
            label_nbtname.Content = overrideName ?? (node.Name == "" ? "<empty name>" : node.Name);
            label_nbtname.Foreground = GetTypeColor(node.Tag);
            label_typename.Content = node.GetTypeName();
            PopulateNBTChildren();
            tbox_valedit.KeyDown += (e, k) =>
            {
                if (k.Key == Key.Enter || k.Key == Key.Escape)
                {
                    if (IsTextValidForNBT())
                    {
                        DoModifyNBTValue();
                        OnValueModified();
                    }
                    label_tagvalue.Visibility = Visibility.Visible;
                    tbox_valedit.Visibility = Visibility.Collapsed;
                    label_tagvalue.Content = targetNode.GetValue();
                }
            };
            tbox_valedit.TextChanged += delegate
            {
                tbox_valedit.Background = IsTextValidForNBT() ? Brushes.Transparent : Brushes.DarkRed;
            };
        }

        public void PopulateNBTChildren()
        {
            label_tagvalue.Content = targetNode.GetValue();
            panel_nbtdata.Children.Clear();
            if (targetNode is NBTTagListNode)
            {
                List<NBTBase> nodeList = ((NBTTagListNode)targetNode).Value;
                label_nbtname.Foreground = GetTypeColor(((NBTTagListNode)targetNode).innerType);

                int x = 0;
                foreach (NBTBase nnode in nodeList)
                {
                    panel_nbtdata.Children.Add(new NBTListUIElement(nnode, this, $"[{x++}]"));
                }
                panel_nbtdata.Children.Add(new NBTListAddNewListElement(this));
            }
            else if (targetNode is NBTTagCompoundNode)
            {
                List<NBTBase> nodeList = ((NBTTagCompoundNode)targetNode).Value;

                foreach (NBTBase nnode in nodeList)
                {
                    panel_nbtdata.Children.Add(new NBTListUIElement(nnode, this));
                }
                panel_nbtdata.Children.Add(new NBTListAddNewToCompound(this));
            }
            else
            {
                panel_nbtdata.Visibility = Visibility.Collapsed;
            }
        }

        private void OnValueModified()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(0x30, 0xf0, 0xb5, 0x56));
        }

        private bool IsTextValidForNBT()
        {
            string ltext = tbox_valedit.Text;
            switch (targetNode.Tag)
            {
                case 1:
                    return Byte.TryParse(ltext, out byte whatev0);
                case 2:
                    return Int16.TryParse(ltext, out short whatev1);
                case 3:
                    return Int32.TryParse(ltext, out int whatev2);
                case 4:
                    return Int64.TryParse(ltext, out long whatev3);
                case 5:
                    return Single.TryParse(ltext, out float whatev4);
                case 6:
                    return Double.TryParse(ltext, out double whatev5);
                case 8:
                    return true;
            }
            return false;
        }        
        private bool DoModifyNBTValue()
        {
            string ltext = tbox_valedit.Text;
            switch (targetNode.Tag)
            {
                case 1:
                    return Byte.TryParse(ltext, out ((NBTNode<byte>)targetNode).Value);
                case 2:
                    return Int16.TryParse(ltext, out ((NBTNode<short>)targetNode).Value);
                case 3:
                    return Int32.TryParse(ltext, out ((NBTNode<int>)targetNode).Value);
                case 4:
                    return Int64.TryParse(ltext, out ((NBTNode<long>)targetNode).Value);
                case 5:
                    return Single.TryParse(ltext, out ((NBTNode<float>)targetNode).Value);
                case 6:
                    return Double.TryParse(ltext, out ((NBTNode<double>)targetNode).Value);
                case 8:
                    ((NBTNode<string>)targetNode).Value = ltext;
                    return true;
            }
            return false;
        }

        private void label_tagvalue_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (new int[] { 1, 2, 3, 4, 5, 6, 8 }.Contains(targetNode.Tag))
            {
                label_tagvalue.Visibility = Visibility.Collapsed;
                tbox_valedit.Visibility = Visibility.Visible;
                tbox_valedit.Text = targetNode.GetValue();
                tbox_valedit.Focus();
            }
        }

        public void ResetModifiedStatus()
        {
            this.Background = Brushes.Transparent;
            foreach (object a in panel_nbtdata.Children)
            {
                if (a is NBTListUIElement)
                {
                    ((NBTListUIElement)a).ResetModifiedStatus();
                }
            }
        }

        private void ContextDeleteNBT_Click(object sender, RoutedEventArgs e)
        {
            if (parentNode != null)
            {
                if (parentNode.targetNode is NBTTagCompoundNode)
                {
                    ((NBTTagCompoundNode)parentNode.targetNode).Value.Remove(targetNode);
                } 
                parentNode.PopulateNBTChildren();
            }
            ctxMenu.IsOpen = false;
        }
    }
}
