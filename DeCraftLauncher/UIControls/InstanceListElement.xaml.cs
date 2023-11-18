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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeCraftLauncher.UIControls
{
    /// <summary>
    /// Logika interakcji dla klasy InstanceListElement.xaml
    /// </summary>
    public partial class InstanceListElement : UserControl
    {
        public class RunningInstanceData
        {
            public string InstanceName;
            public WindowProcessLog processLog;

            public RunningInstanceData(string instanceName, WindowProcessLog processLog)
            {
                InstanceName = instanceName;
                this.processLog = processLog;
            }
        }

        private RunningInstanceData targetLog;

        public InstanceListElement(RunningInstanceData targetLog)
        {
            this.targetLog = targetLog;
            InitializeComponent();
            btn_main.Content = targetLog.InstanceName;
            btn_main.Click += delegate
            {
                this.targetLog.processLog.Show();
                this.targetLog.processLog.Focus();
            };
            
        }
    }
}
