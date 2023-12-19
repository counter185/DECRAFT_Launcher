using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Permissions;
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
        public class ServerPropertiesDataGridItem
        {
            public ServerPropertiesDataGridItem()
            {
                Name = "";
                Value = "";
            }            
            public ServerPropertiesDataGridItem(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public string Value { get; set; }
        }

        private string targetFilePath;
        List<ServerPropertiesDataGridItem> propertiesFile = new List<ServerPropertiesDataGridItem>();

        public WindowServerPropertiesEditor(string path)
        {
            this.targetFilePath = path;
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
            LoadProperties();

            dgrid_properties.ItemsSource = propertiesFile;
        }

        public void LoadProperties()
        {
            try
            {
                string[] fileLines = File.ReadAllLines(targetFilePath);
                propertiesFile = (from x in fileLines
                                  where x.Contains('=') && !x.StartsWith("#")
                                  select new ServerPropertiesDataGridItem(x.Split('=')[0], x.Split('=')[1])).ToList();
            } catch (FileNotFoundException)
            {
                PopupOK.ShowNewPopup("No server.properties file found. Start the server once to generate it.");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            File.WriteAllLines(targetFilePath, from y in propertiesFile
                                               select y.Name+"="+y.Value);
            base.OnClosing(e);
        }
    }
}
