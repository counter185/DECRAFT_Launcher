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

    public partial class PopupTextBox : AcrylicWindow
    {
        //this is meant to be a drop-in replacement for vb Interaction.InputBox
        string targetText = "";

        public PopupTextBox(string maintext, string titletext = "", string defaultText = "")
        {
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);
            label_maintext.Content = maintext;
            Title = titletext;
            tbox_main.Text = defaultText;
        }

        public static string ShowNewPopup(string maintext, string titletext = "", string defaultText = "")
        {
            PopupTextBox newPopup = new PopupTextBox(maintext, titletext, defaultText);
            newPopup.ShowDialog();
            return newPopup.targetText;
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            targetText = tbox_main.Text;
            this.Close();
        }        
        
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
