using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.RightsManagement;
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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy WindowNewCategory.xaml
    /// </summary>
    public partial class WindowNewCategory : AcrylicWindow
    {
        MainWindow caller;

        public WindowNewCategory(MainWindow caller)
        {
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
            this.caller = caller;
        }

        private void tbox_colorargb_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (rect_colorPreview == null)
            {
                return;
            }
            string colorInAARRGGBB = tbox_colorargb.Text;
            bool validColor = false;
            
            if (colorInAARRGGBB.Length == 8)
            {
                try
                {
                    byte a, r, g, b;
                    a = byte.Parse(colorInAARRGGBB.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    r = byte.Parse(colorInAARRGGBB.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    g = byte.Parse(colorInAARRGGBB.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    b = byte.Parse(colorInAARRGGBB.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    validColor = true;
                    rect_colorPreview.Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(a, r, g, b));
                } catch (FormatException)
                {
                }
            }

            if (!validColor)
            {
                rect_colorPreview.Fill = System.Windows.Media.Brushes.Transparent;
            }
        }

        private void btn_addCategory_Click(object sender, RoutedEventArgs e)
        {
            string catName = tbox_categoryName.Text;

            if (!(from x in MainWindow.mainRTConfig.jarCategories
                where x.name == catName
                select x).Any())
            {
                string colorInAARRGGBB = tbox_colorargb.Text;
                if (colorInAARRGGBB.Length == 8)
                {
                    try
                    {
                        uint color = Convert.ToUInt32(colorInAARRGGBB, 16);
                        
                        MainWindow.mainRTConfig.jarCategories.Add(new Category(catName, colorInAARRGGBB));
                        caller.SaveRuntimeConfig();
                        this.Close();
                    } 
                    catch (FormatException)
                    {
                        PopupOK.ShowNewPopup("Invalid color value.", "DECRAFT");
                    }
                } else
                {
                    PopupOK.ShowNewPopup("Invalid color value.", "DECRAFT");
                }
            } 
            else
            {
                PopupOK.ShowNewPopup("A category with this name already exists.", "DECRAFT");
            }

        }
    }
}
