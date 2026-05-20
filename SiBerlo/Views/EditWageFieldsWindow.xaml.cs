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

namespace SiBerlo.Views
{
    /// <summary>
    /// Interaction logic for EditWageFieldsWindow.xaml
    /// </summary>
    public partial class EditWageFieldsWindow : Window
    {
        public double Jutalek { get; set; }
        public double EgyebPotlek { get; set; }
        public double Eloleg { get; set; }

        public EditWageFieldsWindow(double jutalek, double egyebPotlek, double eloleg)
        {
            InitializeComponent();
            Jutalek = jutalek;
            EgyebPotlek = egyebPotlek;
            Eloleg = eloleg;
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }

}
