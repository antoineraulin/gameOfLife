using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace life
{
    /// <summary>
    /// Logique d'interaction pour init.xaml
    /// </summary>
    public partial class init : Window
    {
        bool visuState = false;
        public init()
        {
            InitializeComponent();
          
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void visuStateB_Click(object sender, RoutedEventArgs e)
        {
            visuStateB.Content = visuState? "OFF":"ON";
            visuState = !visuState;
        }

        private void startB_Click(object sender, RoutedEventArgs e)
        {
            int x, y;
            double t;
            Int32.TryParse(gridSizeTBx.Text, out x);
            Int32.TryParse(gridSizeTBy.Text, out y);
            Double.TryParse(tauxRTB.Text, out t);
            MainWindow main = new MainWindow(x, y, t, visuState);
            this.Close();
            main.Show();
        }
    }
}
