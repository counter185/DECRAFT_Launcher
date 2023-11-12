using DeCraftLauncher.Utils;
using MediaDevices;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
            Util.UpdateAcrylicWindowBackground(this);
            this.target = target;

            label_header.Content = $"Deploy {target.jarFileName} to mobile device";

            this.availableDevices = MediaDevice.GetDevices().ToList();

            foreach (MediaDevice dev in availableDevices) {
                listbox_devices.Items.Add(dev.FriendlyName);
            }
        }

        public string GenerateDummyJson(string jarSHA1)
        {
            //todo: calc the size
            //TODO: ONLY GIVE THE NEEDED DOWNLOADS
            return $@"{{
    ""assetIndex"": {{
        ""id"": ""legacy"",
        ""sha1"": ""770572e819335b6c0a053f8378ad88eda189fc14"",
        ""size"": 109634,
        ""totalSize"": 153475165,
        ""url"": ""https://launchermeta.mojang.com/v1/packages/770572e819335b6c0a053f8378ad88eda189fc14/legacy.json""
    }},
    ""assets"": ""legacy"",
    ""downloads"": {{
        ""client"": {{
            ""sha1"": ""{jarSHA1}"",
            ""size"": 0,
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
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""net/sf/jopt-simple/jopt-simple/4.5/jopt-simple-4.5.jar"",
                    ""sha1"": ""6065cc95c661255349c1d0756657be17c29a4fd3"",
                    ""size"": 61311,
                    ""url"": ""https://libraries.minecraft.net/net/sf/jopt-simple/jopt-simple/4.5/jopt-simple-4.5.jar""
                }}
            }},
            ""name"": ""net.sf.jopt-simple:jopt-simple:4.5""
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""org/ow2/asm/asm-all/4.1/asm-all-4.1.jar"",
                    ""sha1"": ""054986e962b88d8660ae4566475658469595ef58"",
                    ""size"": 214592,
                    ""url"": ""https://libraries.minecraft.net/org/ow2/asm/asm-all/4.1/asm-all-4.1.jar""
                }}
            }},
            ""name"": ""org.ow2.asm:asm-all:4.1""
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""net/java/jinput/jinput/2.0.5/jinput-2.0.5.jar"",
                    ""sha1"": ""39c7796b469a600f72380316f6b1f11db6c2c7c4"",
                    ""size"": 208338,
                    ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput/2.0.5/jinput-2.0.5.jar""
                }}
            }},
            ""name"": ""net.java.jinput:jinput:2.0.5""
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""net/java/jutils/jutils/1.0.0/jutils-1.0.0.jar"",
                    ""sha1"": ""e12fe1fda814bd348c1579329c86943d2cd3c6a6"",
                    ""size"": 7508,
                    ""url"": ""https://libraries.minecraft.net/net/java/jutils/jutils/1.0.0/jutils-1.0.0.jar""
                }}
            }},
            ""name"": ""net.java.jutils:jutils:1.0.0""
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""org/lwjgl/lwjgl/lwjgl/2.9.0/lwjgl-2.9.0.jar"",
                    ""sha1"": ""5654d06e61a1bba7ae1e7f5233e1106be64c91cd"",
                    ""size"": 994633,
                    ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl/2.9.0/lwjgl-2.9.0.jar""
                }}
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl:2.9.0"",
            ""rules"": [
                {{
                    ""action"": ""allow""
                }},
                {{
                    ""action"": ""disallow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""org/lwjgl/lwjgl/lwjgl_util/2.9.0/lwjgl_util-2.9.0.jar"",
                    ""sha1"": ""a778846b64008fc7f48ead2377f034e547991699"",
                    ""size"": 173360,
                    ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl_util/2.9.0/lwjgl_util-2.9.0.jar""
                }}
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl_util:2.9.0"",
            ""rules"": [
                {{
                    ""action"": ""allow""
                }},
                {{
                    ""action"": ""disallow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""classifiers"": {{
                    ""natives-linux"": {{
                        ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-linux.jar"",
                        ""sha1"": ""2ba5dcb11048147f1a74eff2deb192c001321f77"",
                        ""size"": 569061,
                        ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-linux.jar""
                    }},
                    ""natives-osx"": {{
                        ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-osx.jar"",
                        ""sha1"": ""6621b382cb14cc409b041d8d72829156a87c31aa"",
                        ""size"": 518924,
                        ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-osx.jar""
                    }},
                    ""natives-windows"": {{
                        ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-windows.jar"",
                        ""sha1"": ""3f11873dc8e84c854ec7c5a8fd2e869f8aaef764"",
                        ""size"": 609967,
                        ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.0/lwjgl-platform-2.9.0-natives-windows.jar""
                    }}
                }}
            }},
            ""extract"": {{
                ""exclude"": [
                    ""META-INF/""
                ]
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl-platform:2.9.0"",
            ""natives"": {{
                ""linux"": ""natives-linux"",
                ""osx"": ""natives-osx"",
                ""windows"": ""natives-windows""
            }},
            ""rules"": [
                {{
                    ""action"": ""allow""
                }},
                {{
                    ""action"": ""disallow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""org/lwjgl/lwjgl/lwjgl/2.9.1-nightly-20130708-debug3/lwjgl-2.9.1-nightly-20130708-debug3.jar"",
                    ""sha1"": ""884511652c756fac16b37236f863f346bd1ea121"",
                    ""size"": 996625,
                    ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl/2.9.1-nightly-20130708-debug3/lwjgl-2.9.1-nightly-20130708-debug3.jar""
                }}
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl:2.9.1-nightly-20130708-debug3"",
            ""rules"": [
                {{
                    ""action"": ""allow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""artifact"": {{
                    ""path"": ""org/lwjgl/lwjgl/lwjgl_util/2.9.1-nightly-20130708-debug3/lwjgl_util-2.9.1-nightly-20130708-debug3.jar"",
                    ""sha1"": ""fb693ba4e22a85432a32e8a048893dc7a92f42ac"",
                    ""size"": 173338,
                    ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl_util/2.9.1-nightly-20130708-debug3/lwjgl_util-2.9.1-nightly-20130708-debug3.jar""
                }}
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl_util:2.9.1-nightly-20130708-debug3"",
            ""rules"": [
                {{
                    ""action"": ""allow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""classifiers"": {{
                    ""natives-osx"": {{
                        ""path"": ""org/lwjgl/lwjgl/lwjgl-platform/2.9.1-nightly-20130708-debug3/lwjgl-platform-2.9.1-nightly-20130708-debug3-natives-osx.jar"",
                        ""sha1"": ""a9b83ad85742cad09c3574a91b0423bac3f7a0f5"",
                        ""size"": 458181,
                        ""url"": ""https://libraries.minecraft.net/org/lwjgl/lwjgl/lwjgl-platform/2.9.1-nightly-20130708-debug3/lwjgl-platform-2.9.1-nightly-20130708-debug3-natives-osx.jar""
                    }}
                }}
            }},
            ""extract"": {{
                ""exclude"": [
                    ""META-INF/""
                ]
            }},
            ""name"": ""org.lwjgl.lwjgl:lwjgl-platform:2.9.1-nightly-20130708-debug3"",
            ""natives"": {{
                ""linux"": ""natives-linux"",
                ""osx"": ""natives-osx"",
                ""windows"": ""natives-windows""
            }},
            ""rules"": [
                {{
                    ""action"": ""allow"",
                    ""os"": {{
                        ""name"": ""osx"",
                        ""version"": ""^10\\.5\\.\\d$""
                    }}
                }}
            ]
        }},
        {{
            ""downloads"": {{
                ""classifiers"": {{
                    ""natives-linux"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-linux.jar"",
                        ""sha1"": ""7ff832a6eb9ab6a767f1ade2b548092d0fa64795"",
                        ""size"": 10362,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-linux.jar""
                    }},
                    ""natives-osx"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-osx.jar"",
                        ""sha1"": ""53f9c919f34d2ca9de8c51fc4e1e8282029a9232"",
                        ""size"": 12186,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-osx.jar""
                    }},
                    ""natives-windows"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-windows.jar"",
                        ""sha1"": ""385ee093e01f587f30ee1c8a2ee7d408fd732e16"",
                        ""size"": 155179,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-windows.jar""
                    }}
                }}
            }},
            ""extract"": {{
                ""exclude"": [
                    ""META-INF/""
                ]
            }},
            ""name"": ""net.java.jinput:jinput-platform:2.0.5"",
            ""natives"": {{
                ""linux"": ""natives-linux"",
                ""osx"": ""natives-osx"",
                ""windows"": ""natives-windows""
            }}
        }},
        {{
            ""downloads"": {{
                ""classifiers"": {{
                    ""natives-linux"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-linux.jar"",
                        ""sha1"": ""7ff832a6eb9ab6a767f1ade2b548092d0fa64795"",
                        ""size"": 10362,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-linux.jar""
                    }},
                    ""natives-osx"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-osx.jar"",
                        ""sha1"": ""53f9c919f34d2ca9de8c51fc4e1e8282029a9232"",
                        ""size"": 12186,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-osx.jar""
                    }},
                    ""natives-windows"": {{
                        ""path"": ""net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-windows.jar"",
                        ""sha1"": ""385ee093e01f587f30ee1c8a2ee7d408fd732e16"",
                        ""size"": 155179,
                        ""url"": ""https://libraries.minecraft.net/net/java/jinput/jinput-platform/2.0.5/jinput-platform-2.0.5-natives-windows.jar""
                    }}
                }}
            }},
            ""extract"": {{
                ""exclude"": [
                    ""META-INF/""
                ]
            }},
            ""name"": ""net.java.jinput:jinput-platform:2.0.5"",
            ""natives"": {{
                ""linux"": ""natives-linux"",
                ""osx"": ""natives-osx"",
                ""windows"": ""natives-windows""
            }}
        }}
    ],
    ""mainClass"": ""net.minecraft.client.Minecraft"",
    ""minecraftArguments"": ""${{auth_player_name}} ${{auth_session}}"",
    ""minimumLauncherVersion"": 7,
    ""releaseTime"": ""2000-06-06T00:00:00+09:00"",
    ""time"": ""2000-06-06T00:00:00+09:00"",
    ""type"": ""old_alpha""
}}
";
        }

        void DeployTo(string path)
        {
            try
            {
                string baseName = target.jarFileName.Substring(0, target.jarFileName.Length - 4);
                if (!connectedDevice.DirectoryExists($"{path}/{baseName}"))
                {
                    SHA1 sha1Calc = SHA1.Create();
                    string sha1Hash = "";
                    using (FileStream sha1Stm = File.Open($"{MainWindow.jarDir}/{target.jarFileName}", FileMode.Open, FileAccess.Read))
                    {
                        sha1Hash = BitConverter.ToString(sha1Calc.ComputeHash(sha1Stm));
                    }

                    connectedDevice.CreateDirectory($"{path}/{baseName}");
                    FileStream jarStream = File.OpenRead($"{MainWindow.jarDir}/{target.jarFileName}");
                    connectedDevice.UploadFile(jarStream, $"{path}/{baseName}/{baseName}.jar");

                    MemoryStream jsonStm = new MemoryStream(Encoding.ASCII.GetBytes(GenerateDummyJson(sha1Hash)));
                    connectedDevice.UploadFile(jsonStm, $"{path}/{baseName}/{baseName}.json");

                    MessageBox.Show("Upload complete");

                }
                else
                {
                    MessageBox.Show($"{path}/{baseName} already exists", "DECRAFT");
                }
            } catch (Exception e)
            {
                MessageBox.Show($"Error: {e.Message}", "DECRAFT");
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
