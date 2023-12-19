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

namespace DeCraftLauncher.UIControls.Popup
{
    /// <summary>
    /// Logika interakcji dla klasy PopupOK.xaml
    /// </summary>
    public partial class PopupOK : AcrylicWindow
    {
        public PopupOK(string maintext, string titletext = "")
        {
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);
            label_maintext.Content = maintext;
            Title = titletext;
        }

        public static void ShowNewPopup(string maintext, string titletext = "")
        {
            new PopupOK(maintext, titletext).ShowDialog();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
