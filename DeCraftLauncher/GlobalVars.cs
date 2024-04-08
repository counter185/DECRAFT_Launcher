using DeCraftLauncher.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DeCraftLauncher
{
    public static class GlobalVars
    {
        public static string versionCode = "1.3-loc_v3-dev";
        public static DiscordRPCManager discordRPCManager = new DiscordRPCManager();
        public static LocalizationManager locManager = new LocalizationManager();

        public static LocalizationManager L { get => locManager; }


        public static readonly DependencyProperty LocalizationKeyProperty = DependencyProperty.RegisterAttached("LocKey", typeof(string), typeof(GlobalVars), new PropertyMetadata(default(string)));
        public static void SetLocKey(UIElement element, string value) => element.SetValue(LocalizationKeyProperty, value);
        public static string GetLocKey(UIElement element) => (string)element.GetValue(LocalizationKeyProperty);
    }
}
