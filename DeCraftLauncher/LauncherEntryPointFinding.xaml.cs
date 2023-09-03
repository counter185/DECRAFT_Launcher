using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeCraftLauncher
{
    /// <summary>
    /// Logika interakcji dla klasy LaunchEntryPoint.xaml
    /// </summary>
    public partial class LaunchEntryPointFinding : UserControl
    {
        ReferenceType<float> progress;

        public LaunchEntryPointFinding(ReferenceType<float> target)
        {
            InitializeComponent();
            progress = target;
            new Thread(ThreadUpdate).Start();
        }

        public void ThreadUpdate()
        {
            bool cont = true;
            while (cont)
            {
                Dispatcher.Invoke(delegate
                {
                    cont = this.Parent != null;
                    float v = progress.Value;
                    search_progress.Value = v * 100;
                    progress_pct.Content = Math.Round(v * 100, 1) + "%";
                });
            }
            Console.WriteLine("ThreadUpdate finished");
        }
    }
}
