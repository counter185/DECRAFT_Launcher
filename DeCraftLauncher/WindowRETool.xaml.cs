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
using DeCraftLauncher.UIControls;
using DeCraftLauncher.UIControls.Popup;
using DeCraftLauncher.UIControls.RETool;
using DeCraftLauncher.Utils;
using SourceChord.FluentWPF;
using static DeCraftLauncher.UIControls.RETool.WindowREToolOutgoingRefsScanResult;
using static DeCraftLauncher.Utils.JavaClassReader;
using static DeCraftLauncher.Utils.JavaClassReader.ConstantPoolEntry;

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
            GlobalVars.L.Translate(
                    this,
                    btn_tools,
                    label_header
                );
            label_filename.Content = Util.CleanStringForXAML(target);
            label_jvmVersion.Content = "";
            label_panelheader.Content = "";
            try
            {
                /*IEnumerable<JavaClassInfo> classes = (from x in arc.Entries
                                                      where x.FullName.EndsWith(".class")
                                                      orderby x.FullName
                                                      select new Func<ZipArchiveEntry, JavaClassInfo>((z)=> {
                                                          Stream zipStream = z.Open();
                                                          JavaClassInfo classInf = JavaClassReader.ReadJavaClassFromStream(zipStream);
                                                          zipStream.Close();
                                                          return classInf;
                                                      }).Invoke(x));*/

                arc = ZipFile.OpenRead(target);
                classFiles = (from x in arc.Entries
                                where x.FullName.EndsWith(".class")
                                orderby x.FullName
                                select x.FullName).ToList();
                classFiles.ForEach(x => listbox_classlist.Items.Add(x));

            } catch (IOException ex)
            {
                PopupOK.ShowNewPopup(GlobalVars.L.Translate("popup.error_reading_archive", ex.Message));
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

            label_panelheader.Content = $"class {currentClassInfo.ThisClassName(currentClassInfo.entries)} (extends {currentClassInfo.SuperClassName(currentClassInfo.entries)})";
            label_jvmVersion.Content = $"Class version: {Util.JavaVersionFriendlyName($"{currentClassInfo.versionMajor}.{currentClassInfo.versionMinor}")}";

            listbox_constpool.Items.Clear();
            List<int> alreadyDone = new List<int>();
            for (int x = 0; x < currentClassInfo.entries.Count; x++)
            {
                if (!alreadyDone.Contains(x))
                {
                    REToolConstPoolEntry newReToolUIEntry = new REToolConstPoolEntry(x, currentClassInfo.entries);
                    listbox_constpool.Items.Add(newReToolUIEntry);
                    alreadyDone.AddRange(newReToolUIEntry.subEntries);
                    alreadyDone.Add(x);
                }
            }

            listbox_methods.Items.Clear();
            currentClassInfo.methods.ForEach(x =>
            {
                listbox_methods.Items.Add(new REToolMethodEntry(x, currentClassInfo.entries));
            });

            listbox_fields.Items.Clear();
            currentClassInfo.fields.ForEach(x =>
            {
                listbox_fields.Items.Add(new REToolFieldEntry(x, currentClassInfo.entries));
            });
        }
        private void btn_tools_Click(object sender, RoutedEventArgs e)
        {
            ctxmenu_tools.IsOpen = true;
        }

        private void btn_ctx_findalloutgoingrefs_Click(object sender, RoutedEventArgs e)
        {
            var all = (from y in (from x in classFiles
                                  select new Func<string, JavaClassInfo>(f =>
                                  {
                                      Stream stream = arc.GetEntry(x).Open();
                                      JavaClassInfo classInf = JavaClassReader.ReadJavaClassFromStream(stream);
                                      stream.Close();
                                      return classInf;
                                  }).Invoke(x))
                       select (from z in y.entries
                               where z is MethodReferenceEntry && (!classFiles.Contains(((MethodReferenceEntry)z).ClassReferenceName(y.entries) + ".class"))
                               //select $"[{((MethodReferenceEntry)z).ClassReferenceName(y.entries)}] {((MethodReferenceEntry)z).FunctionNameAndDescriptor(y.entries)} <- {y.ThisClassName(y.entries)}")
                               select new RefScanEntry
                               {
                                   ClassName = ((MethodReferenceEntry)z).ClassReferenceName(y.entries),
                                   Name = ((MethodReferenceEntry)z).Name(y.entries),
                                   Descriptor = ((MethodReferenceEntry)z).Descriptor(y.entries),
                                   CallingClassName = y.ThisClassName(y.entries)
                               })
                       ).SelectMany(q=>q).OrderBy(k=>k.ClassName);
            new WindowREToolOutgoingRefsScanResult(all).Show();
        }

        private void btn_ctx_findallfnrefs_Click(object sender, RoutedEventArgs e)
        {
            //todo: maybe put these two into  one function
            var all = (from y in (from x in classFiles
                                  select new Func<string, JavaClassInfo>(f =>
                                  {
                                      Stream stream = arc.GetEntry(x).Open();
                                      JavaClassInfo classInf = JavaClassReader.ReadJavaClassFromStream(stream);
                                      stream.Close();
                                      return classInf;
                                  }).Invoke(x))
                       select (from z in y.entries
                               where z is MethodReferenceEntry
                               select new RefScanEntry
                               {
                                   ClassName = ((MethodReferenceEntry)z).ClassReferenceName(y.entries),
                                   Name = ((MethodReferenceEntry)z).Name(y.entries),
                                   Descriptor = ((MethodReferenceEntry)z).Descriptor(y.entries),
                                   CallingClassName = y.ThisClassName(y.entries)
                               })
                       ).SelectMany(q => q).OrderBy(k => k.ClassName);
            new WindowREToolOutgoingRefsScanResult(all).Show();
        }
    }
}
