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

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy LabelURLDownload.xaml
    /// </summary>
    public partial class LabelURLDownload : UserControl
    {
        public string url = "";
        public string mainText = "";
        public LabelURLDownload(string mainText, string url)
        {
            InitializeComponent();
            label_maintext.Content = mainText;
            this.mainText = mainText;
            this.url = url;
        }
    }
}
