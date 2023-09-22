using MediaDevices;
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

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy WindowDeployMTP.xaml
    /// </summary>
    public partial class WindowDeployMTP : AcrylicWindow
    {
        JarEntry target;
        List<MediaDevice> availableDevices;
        MediaDevice connectedDevice = null;

        public WindowDeployMTP(JarEntry target)
        {
            InitializeComponent();
            this.target = target;

            label_header.Content = $"Deploy {target.jarFileName} to mobile device";

            this.availableDevices = MediaDevice.GetDevices().ToList();

            foreach (MediaDevice dev in availableDevices) {
                listbox_devices.Items.Add(dev.FriendlyName);
            }
        }

        public string GenerateDummyJson()
        {
            return $@"{{
    ""assetIndex"": {{
        ""id"": ""legacy"",
        ""url"": ""https://launchermeta.mojang.com/v1/packages/770572e819335b6c0a053f8378ad88eda189fc14/legacy.json""
    }},
    ""assets"": ""legacy"",
    ""downloads"": {{
        ""client"": {{
            ""url"": ""http://0.0.0.0/client.jar""
        }}
    }},
    ""id"": ""{target.jarFileName.Substring(0, target.jarFileName.Length - 4)}"",
    ""libraries"": [
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""net/minecraft/launchwrapper/1.5/launchwrapper-1.5.jar"",
                    ""sha1"": ""5150b9c2951f0fde987ce9c33496e26add1de224"",
                    ""size"": 27787,
                    ""url"": ""https://libraries.minecraft.net/net/minecraft/launchwrapper/1.5/launchwrapper-1.5.jar""
                }}
            }},
            ""name"": ""net.minecraft:launchwrapper: 1.5""
        }}
    ],
    ""mainClass"": ""net.minecraft.launchwrapper.Launch"",
    ""minecraftArguments"": ""${{auth_player_name}} ${{auth_session}} --gameDir ${{game_directory}} --assetsDir ${{game_assets}} --tweakClass net.minecraft.launchwrapper.AlphaVanillaTweaker"",
    ""minimumLauncherVersion"": 7
}}
";
        }

        void DeployTo(string path)
        {
            string baseName = target.jarFileName.Substring(0, target.jarFileName.Length - 4);
            if (!connectedDevice.DirectoryExists($"{path}/{baseName}"))
            {
                connectedDevice.CreateDirectory($"{path}/{baseName}");
                FileStream jarStream = File.OpenRead($"{MainWindow.jarDir}/{target.jarFileName}");
                connectedDevice.UploadFile(jarStream, $"{path}/{baseName}/{baseName}.jar");

                MemoryStream jsonStm = new MemoryStream(Encoding.ASCII.GetBytes(GenerateDummyJson()));
                connectedDevice.UploadFile(jsonStm, $"{path}/{baseName}/{baseName}.json");

                MessageBox.Show("Upload complete");

            } else
            {
                MessageBox.Show($"{path}/{baseName} already exists", "DECRAFT");
            }
        }

        private void listbox_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listbox_datapaths.Items.Clear();
            if (connectedDevice != null)
            {
                connectedDevice.Disconnect();
            }

            connectedDevice = availableDevices[listbox_devices.SelectedIndex];
            connectedDevice.Connect();

            string[] pojavDataPaths = new string[] {
                "/Android/data/net.kdt.pojavlaunch/files/.minecraft/versions",
                "/Android/data/net.kdt.pojavlaunch.debug/files/.minecraft/versions",
            };

            foreach (MediaDriveInfo mdi in connectedDevice.GetDrives())
            {
                foreach (string a in pojavDataPaths)
                {
                    if (connectedDevice.DirectoryExists(mdi.Name + a))
                    {
                        TextBlock mtpdTextBlock = new TextBlock();
                        mtpdTextBlock.Text = mdi.Name + a;
                        mtpdTextBlock.MouseLeftButtonDown += delegate
                        {
                            DeployTo(mdi.Name + a);
                            this.Close();
                        };
                        listbox_datapaths.Items.Add(mtpdTextBlock);
                    }
                }
            }
        }
    }
}
