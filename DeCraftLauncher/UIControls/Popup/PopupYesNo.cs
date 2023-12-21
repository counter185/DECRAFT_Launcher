using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeCraftLauncher.UIControls.Popup
{
    public class PopupYesNo : PopupYesNoCancel
    {
        public PopupYesNo(string maintext, string titletext = "") : base(maintext, titletext, false)
        {
        }

        public static new MessageBoxResult ShowNewPopup(string maintext, string titletext = "")
        {
            PopupYesNo newPopup = new PopupYesNo(maintext, titletext);
            newPopup.ShowDialog();
            return newPopup.returnValue;
        }
    }
}
