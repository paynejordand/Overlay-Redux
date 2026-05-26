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

namespace Overlay_Redux
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MedsWindow : Window
    {
        public MedsViewModel ViewModel { get; } = new();
        public MedsWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
