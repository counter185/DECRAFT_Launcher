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

namespace DECRAFTModdingEnvironment.InitialSetup
{
    /// <summary>
    /// Logika interakcji dla klasy SetupStage1.xaml
    /// </summary>
    public partial class SetupStage1
    {
        public SetupStage1()
        {
            InitializeComponent();
            DME.Utils.Util.UpdateAcrylicWindowBackground(this);
        }
    }
}
