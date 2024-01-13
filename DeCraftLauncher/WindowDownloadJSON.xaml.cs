﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.Utils;
using Newtonsoft.Json.Linq;
using SourceChord.FluentWPF;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy WindowDownloadJSON.xaml
    /// </summary>
    public partial class WindowDownloadJSON : AcrylicWindow
    {
        MainWindow caller;
        List<RadioButton> jarBtns = new List<RadioButton>();
        List<CheckBox> libBtns = new List<CheckBox>();

        public WindowDownloadJSON(MainWindow caller, string jsonPath)
        {
            this.caller = caller;
            InitializeComponent();
            Util.UpdateAcrylicWindowBackground(this);

            TryToMakeSenseOfJson(jsonPath);

            GlobalVars.L.Translate(this,
                label_header,
                label_jardls_hint,
                label_libdls_hint,
                label_saveas_hint,
                btn_download
                );
        }

        void TryToMakeSenseOfJson(string a)
        {
            try
            {
                //todo: clean this up
                JObject rootObj = JObject.Parse(File.ReadAllText(a));
                string versionID = rootObj.SelectToken("id").Value<string>();
                JObject dlElement = rootObj.SelectToken("downloads").Value<JObject>().SelectToken("client").Value<JObject>();

                tbox_jarsave_name.Text = versionID;

                foreach (JObject jobj in from x in rootObj.SelectToken("downloads").Value<JObject>().Values()
                                         select x.Value<JObject>())
                {
                    string url = jobj.SelectToken("url").Value<string>();
                    if (url.EndsWith(".jar"))
                    {
                        RadioButton nRadioButton = new RadioButton
                        {
                            GroupName = "jardl_group",
                            Content = url.Substring(url.LastIndexOfAny("/\\".ToCharArray())),
                            IsChecked = false,
                            Foreground = Brushes.White,
                            Padding = new Thickness(0, 4, 0, 4),
                            VerticalContentAlignment = VerticalAlignment.Center
                        };
                        jarBtns.Add(nRadioButton);
                        panel_jardls.Children.Add(nRadioButton);
                    }
                }

                foreach (JObject jobj in from x in rootObj.SelectToken("libraries").Value<JArray>()
                                         select x.Value<JObject>())
                {
                    string libname = jobj.SelectToken("name").Value<string>();
                    string url = jobj.SelectToken("downloads").SelectToken("artifact").SelectToken("url").Value<string>();
                    JToken rulesObj = jobj.SelectToken("rules");
                    if (rulesObj != null)
                    {
                        if ((from x in rulesObj.Value<JArray>()
                             where x.SelectToken("action") != null && x.SelectToken("os") != null
                                && x.SelectToken("action").Value<string>() == "allow"
                                && x.SelectToken("os").SelectToken("name").Value<string>() == "osx"
                             select x).Any()){
                            continue;
                        }
                    }

                    CheckBox nCheckbox = new CheckBox
                    {
                        Content = libname,
                        IsChecked = true,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(0, 4, 0, 4),
                        Foreground = Brushes.White
                    };
                    libBtns.Add(nCheckbox);
                    panel_libdls.Children.Add(nCheckbox);
                }

                    /*if (!caller.currentJarDownloads.Contains($"{versionID}.jar"))
                    {
                        if (PopupYesNo.ShowNewPopup(GlobalVars.locManager.Translate("popup.ask_download", Util.CleanStringForXAML(versionID), dlElement.SelectToken("size").Value<UInt64>() + "", dlElement.SelectToken("url").Value<string>()), "DECRAFT") == MessageBoxResult.Yes)
                        {
                            using (var client = new WebClient())
                            {
                                caller.currentJarDownloads.Add($"{versionID}.jar");
                                client.DownloadFileCompleted += (sender2, evt) =>
                                {
                                    caller.currentJarDownloads.Remove($"{versionID}.jar");
                                    if (evt.Error != null)
                                    {
                                        string errorString = GlobalVars.locManager.Translate("popup.download_error1", evt.Error.Message);
                                        if (evt.Error is System.Net.WebException && evt.Error.Message.Contains("SSL/TLS"))
                                        {
                                            errorString += GlobalVars.locManager.Translate("popup.download_error_ssl_expired");
                                        }
                                        PopupOK.ShowNewPopup(errorString, "DECRAFT");
                                    }
                                    else
                                    {
                                        PopupOK.ShowNewPopup(GlobalVars.locManager.Translate("popup.download_complete"), "DECRAFT");
                                        caller.ResetJarlist();
                                    }
                                };
                                //todo: progress bar for this
                                client.DownloadFileAsync(new Uri(dlElement.SelectToken("url").Value<string>()), $"{MainWindow.jarDir}/{versionID}.jar");
                            }
                        }
                    }
                    else
                    {
                        PopupOK.ShowNewPopup(GlobalVars.locManager.Translate("popup.error_download_already_downloading", Util.CleanStringForXAML(versionID)), "DECRAFT");
                    }*/
                }
            catch (ArgumentNullException ex)
            {
                PopupOK.ShowNewPopup($"Error reading {a}.\nThis JSON file does not contain a download URL at /downloads/client/url.\n\nError details:\n{ex.Message}", "DECRAFT");
            }
            catch (Exception ex)
            {
                PopupOK.ShowNewPopup($"Error reading {a}.\nThe JSON file may be invalid or not in a standard launcher format.\n\n{ex.Message}", "DECRAFT");
            }
        }

        private void btn_download_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
