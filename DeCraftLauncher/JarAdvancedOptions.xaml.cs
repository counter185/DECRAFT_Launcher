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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy JarAdvancedOptions.xaml
    /// </summary>
    public partial class JarAdvancedOptions : AcrylicWindow
    {
        JarConfig targetConfig;

        public JarAdvancedOptions(JarConfig target)
        {
            this.targetConfig = target;
            InitializeComponent();
            label_title.Content = $"Advanced options: {target.jarFileName}";
            LoadConfig();
            Utils.UpdateAcrylicWindowBackground(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveConfig();
        }

        public void LoadConfig()
        {
            tbox_sessionid.Text = targetConfig.sessionID;
            tbox_gameargs.Text = targetConfig.gameArgs;
            checkbox_emulatehttp.IsChecked = targetConfig.appletEmulateHTTP;
            tbox_appletdocumenturl.Text = targetConfig.documentBaseUrl;
        }

        public void SaveConfig()
        {
            targetConfig.sessionID = tbox_sessionid.Text;
            targetConfig.gameArgs = tbox_gameargs.Text;
            targetConfig.appletEmulateHTTP = checkbox_emulatehttp.IsChecked == true;
            targetConfig.documentBaseUrl = tbox_appletdocumenturl.Text;
            targetConfig.SaveToXML(MainWindow.configDir + "/" + targetConfig.jarFileName + ".xml");
        }
    }
}
