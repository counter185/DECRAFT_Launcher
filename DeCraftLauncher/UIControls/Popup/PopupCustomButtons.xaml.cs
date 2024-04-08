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
    public partial class PopupCustomButtons : AcrylicWindow
    {
        public MessageBoxResult returnValue = MessageBoxResult.None;

        public class CustomButton {
            public string text;
            public Action<PopupCustomButtons> action;
        }

        public PopupCustomButtons(string maintext, string titletext, params CustomButton[] customButtons)
        {
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);
            label_maintext.Content = maintext;
            Title = titletext;
            foreach (CustomButton a in customButtons)
            {
                Button nbutton = new Button
                {
                    Style = (Style)Application.Current.FindResource("ButtonMainStyle"),
                    Margin = new Thickness(5,0,5,0),
                    Content = a.text,
                    MinWidth = 70,
                    Height = 25
                };
                nbutton.Click += (s, o) =>
                {
                    a.action(this);
                    this.Close();
                };
                panel_buttons.Children.Add(nbutton);
            }
        }

        public static MessageBoxResult ShowNewPopup(string maintext, string titletext, params CustomButton[] customButtons)
        {
            PopupCustomButtons newPopup = new PopupCustomButtons(maintext, titletext, customButtons);
            newPopup.ShowDialog();
            return newPopup.returnValue;
        }
    }
}
