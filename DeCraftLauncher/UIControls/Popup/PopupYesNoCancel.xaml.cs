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
    public partial class PopupYesNoCancel : AcrylicWindow
    {
        public MessageBoxResult returnValue = MessageBoxResult.None;

        public PopupYesNoCancel(string maintext, string titletext = "", bool showCancel = true)
        {
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);
            label_maintext.Content = maintext;
            Title = titletext;
            if (!showCancel)
            {
                btn_cancel.Visibility = Visibility.Collapsed;
            }
        }

        public static MessageBoxResult ShowNewPopup(string maintext, string titletext = "")
        {
            PopupYesNoCancel newPopup = new PopupYesNoCancel(maintext, titletext, true);
            newPopup.ShowDialog();
            return newPopup.returnValue;
        }

        private void btn_yes_Click(object sender, RoutedEventArgs e)
        {
            returnValue = MessageBoxResult.Yes;
            Close();
        }

        private void btn_no_Click(object sender, RoutedEventArgs e)
        {
            returnValue = MessageBoxResult.No;
            Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            returnValue = MessageBoxResult.Cancel;
            Close();
        }
    }
}
