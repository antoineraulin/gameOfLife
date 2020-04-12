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
        bool version = false;
        public init()
        {
            InitializeComponent();
            versionB.Click += new RoutedEventHandler(delegate (object sender, RoutedEventArgs e)
            {
                versionB.Content = version ? "Classique" : "Variante";
                version = !version;
                Application.Current.MainWindow.Height = 305 + (version ? 100 : 0);
                slider.Visibility = version ? Visibility.Visible : Visibility.Hidden;
                sliderL.Visibility = version ? Visibility.Visible : Visibility.Hidden;
                popL.Visibility = version ? Visibility.Visible : Visibility.Hidden;
                if (!version)
                {
                    slider.Value = 1.0;
                }
            });
          
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
            MainWindow main = new MainWindow(x, y, t, visuState, version,(int) slider.Value);
            this.Close();
            main.Show();
        }
    }
}
