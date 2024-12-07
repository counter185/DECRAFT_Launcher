using DeCraftLauncher.Configs;
using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public partial class WindowRuntimeConfig : AcrylicWindow
    {
        MainWindow parent;
        bool affectComboboxChanges = false;

        public WindowRuntimeConfig(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);
            jre_path.Text = MainWindow.mainRTConfig.javaHome;
            checkbox_isjava9.IsChecked = MainWindow.mainRTConfig.isJava9;
            checkbox_autoexitprocesslog.IsChecked = MainWindow.mainRTConfig.autoExitProcessLog;
            checkbox_enablediscord.IsChecked = MainWindow.mainRTConfig.enableDiscordRPC;
            checkbox_setheapdump.IsChecked = MainWindow.mainRTConfig.setHeapDump;
            checkbox_runautoupdater.IsChecked = MainWindow.mainRTConfig.runAutoUpdater;
            checkbox_disable_transparent_windows.IsChecked = MainWindow.mainRTConfig.disableWindowEffects;

            cbox_langs.Items.Add("English");
            if (Directory.Exists("./Localization/"))
            {
                (from x in Directory.GetFiles("./Localization")
                 where x.EndsWith(".decraft_lang")
                 select x.Substring(x.LastIndexOfAny("\\/".ToCharArray())+1)).ToList().ForEach((x) => cbox_langs.Items.Add(x.Substring(0, x.IndexOf("."))));
            }
            cbox_langs.SelectedValue = String.IsNullOrEmpty(MainWindow.mainRTConfig.useLocalizationFile) ? "English" : MainWindow.mainRTConfig.useLocalizationFile;

            affectComboboxChanges = true;

            GlobalVars.locManager.Translate(
                this,
                label_javapath,
                btn_find,
                label_header,
                label_javahint,
                label_usejava9,
                label_autoclose_processlog,
                label_enable_discordrpc,
                label_set_heapdump,
                label_language,
                label_run_updater,
                label_disable_transparent_windows,
                btn_identgpu,
                jreconfig_version
            );
        }

        public void FixJavaHomeString()
        {
            jre_path.Text = jre_path.Text.TrimEnd(' ').TrimStart(' ');
            if (jre_path.Text != "" && (!jre_path.Text.EndsWith("\\") && !jre_path.Text.EndsWith("/")))
            {
                jre_path.Text += "\\";
            }
        }

        public void UpdateJREVersionString()
        {
            FixJavaHomeString();

            if (jre_path.Text != "" && !File.Exists(jre_path.Text + "java.exe") && File.Exists(jre_path.Text + "bin\\java.exe"))
            {
                jre_path.Text += "bin\\";
            }

            string verre = JarUtils.GetJREInstalled(jre_path.Text);
            string verdk = JarUtils.GetJDKInstalled(jre_path.Text);
            if (verdk != null)
            {
                int JDKVer = Util.TryParseJavaCVersionString(verdk);
                Console.WriteLine($"Detected JDK Version: {JDKVer}");
                if (JDKVer != -1)
                {
                    checkbox_isjava9.IsChecked = JDKVer >= 9;
                }
            }

            string testString = GlobalVars.locManager.Translate("window.config.rt.codegen.enter_to_test_java") +
                $"\nJRE: {(verre != null ? verre : GlobalVars.locManager.Translate("window.config.rt.codegen.java_detect_none"))}" +
                $"\nJDK: {(verdk != null ? verdk : GlobalVars.locManager.Translate("window.config.rt.codegen.java_detect_none"))}" +
                $"\n{(Util.ListAllGPUs().Count > 1 ? GlobalVars.locManager.Translate("window.config.rt.codegen.multi_gpu_warning") : "")}";
            

            jreconfig_version.Text = Util.CleanStringForXAML(testString);

        }

        public void SetAndTestJavaPath(string path)
        {
            this.jre_path.Text = path;
            UpdateJREVersionString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void jre_path_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateJREVersionString();
            }
        }

        private void SaveConfig()
        {
            FixJavaHomeString();
            MainWindow.mainRTConfig.javaHome = jre_path.Text;
            MainWindow.mainRTConfig.isJava9 = checkbox_isjava9.IsChecked == true;
            MainWindow.mainRTConfig.autoExitProcessLog = checkbox_autoexitprocesslog.IsChecked == true;
            MainWindow.mainRTConfig.enableDiscordRPC = checkbox_enablediscord.IsChecked == true;
            MainWindow.mainRTConfig.setHeapDump = checkbox_setheapdump.IsChecked == true;
            MainWindow.mainRTConfig.useLocalizationFile = (string)cbox_langs.SelectedValue == "English" ? null : (string)cbox_langs.SelectedValue;
            MainWindow.mainRTConfig.runAutoUpdater = checkbox_runautoupdater.IsChecked == true;
            MainWindow.mainRTConfig.disableWindowEffects = checkbox_disable_transparent_windows.IsChecked == true;
            parent.SaveRuntimeConfig();
        }

        private void AcrylicWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                SaveConfig();
            } catch (Exception ex)
            {
                PopupOK.ShowNewPopup(ex.ToString(), "DECRAFT");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new WindowJavaFinder(this).Show();
        }

        private void checkbox_enablediscord_Checked(object sender, RoutedEventArgs e)
        {
            GlobalVars.discordRPCManager.Init(parent);
        }

        private void checkbox_enablediscord_Unchecked(object sender, RoutedEventArgs e)
        {
            GlobalVars.discordRPCManager.Close();
        }

        private void btn_identgpu_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
            MainWindow.EnsureDir("./java_temp");
            if (File.Exists("./java_temp/decraft_internal/LWJGLTestGPU.class"))
            {
                File.Delete("./java_temp/decraft_internal/LWJGLTestGPU.class");
            }
            File.WriteAllText("./java_temp/LWJGLTestGPU.java", JavaCode.GenerateLWJGLGPUTestCode());
            try
            {
                var compilerOut = JarUtils.RunProcessAndGetOutput(jre_path.Text + "javac", $"-cp \"lwjgl/2.9.3/*\" " +
                        $"./java_temp/LWJGLTestGPU.java " +
                        $"-d ./java_temp ", true);
            } catch (Exception ex)
            {
                if (ex is ApplicationException || ex is Win32Exception)
                {
                    PopupOK.ShowNewPopup(GlobalVars.locManager.Translate("popup.gpu_test_jdk_error"));
                    return;
                }
                else
                {
                    throw ex;
                }
            }

            JavaExec exec = new JavaExec("decraft_internal.LWJGLTestGPU");
            exec.classPath.Add("java_temp");
            exec.classPath.Add("lwjgl/2.9.3/lwjgl.jar");

            exec.jvmArgs.Add("-Djava.library.path=\"lwjgl/2.9.3/native\"");
            exec.Start(null, x =>
            {
                Console.WriteLine(String.Join(" ", x));
                PopupOK.ShowNewPopup(GlobalVars.locManager.Translate("popup.gpu_test_result") + String.Join("\n", x), "DECRAFT");
            });
            
            
        }

        private void cbox_langs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (affectComboboxChanges)
            {
                PopupOK.ShowNewPopup(GlobalVars.L.Translate("popup.restart_to_apply_lang"));
            }
        }
    }
}
