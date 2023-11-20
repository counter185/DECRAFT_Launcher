using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace DeCraftLauncher.Configs.UI
{
    /// <summary>
    /// Interaction logic for WindowServerPropertiesEditor.xaml
    /// </summary>
    public partial class WindowServerPropertiesEditor : AcrylicWindow
    {
        private string targetFilePath;
        Dictionary<string, string> propertiesFile = new Dictionary<string, string>();

        public WindowServerPropertiesEditor(string path)
        {
            this.targetFilePath = path;
            InitializeComponent();
            LoadProperties();

            dgrid_properties.ItemsSource = propertiesFile;
        }

        public void LoadProperties()
        {
            string[] fileLines = File.ReadAllLines(targetFilePath);
            foreach (var pair in (from x in fileLines
                                  where x.Contains('=') && !x.StartsWith("#")
                                  select new KeyValuePair<string,string>(x.Split('=')[0], x.Split('=')[1])))
            {
                propertiesFile[pair.Key] = pair.Value;
            }
        }
    }
}
