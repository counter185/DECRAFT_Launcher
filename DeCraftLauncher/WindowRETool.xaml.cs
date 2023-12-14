using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using static DeCraftLauncher.Utils.JavaClassReader;

namespace DeCraftLauncher
{
    /// <summary>
    /// Interaction logic for WindowRETool.xaml
    /// </summary>
    public partial class WindowRETool : AcrylicWindow
    {
        string targetJarFile;
        List<string> classFiles;
        ZipArchive arc = null;
        JavaClassInfo currentClassInfo = null;

        public WindowRETool(string target)
        {
            this.targetJarFile = target;
            InitializeComponent();
            Utils.Util.UpdateAcrylicWindowBackground(this);
            try
            {
                arc = ZipFile.OpenRead(target);
                classFiles = (from x in arc.Entries
                                where x.FullName.EndsWith(".class")
                                select x.FullName).ToList();
                classFiles.ForEach(x => listbox_classlist.Items.Add(x));

            } catch (IOException ex)
            {
                MessageBox.Show($"Error reading archive: {ex.Message}");
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (arc != null)
            {
                arc.Dispose();
            }
        }

        private void listbox_classlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stream stream = arc.GetEntry((string)listbox_classlist.SelectedItem).Open();
            currentClassInfo = JavaClassReader.ReadJavaClassFromStream(stream);
            stream.Close();
        }
    }
}
