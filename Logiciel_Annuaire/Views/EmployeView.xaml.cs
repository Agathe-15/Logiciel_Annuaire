using System.Collections.ObjectModel;
using System.Windows;
using AnnuaireWPF.Models;

namespace AnnuaireWPF.Views
{
    public partial class EmployeView : Window
    {
        public EmployeView(Employe employe)
        {
            InitializeComponent();
            DataContext = employe;
        }

        public static ObservableCollection<Employe> ItemsSource { get; internal set; }

        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
