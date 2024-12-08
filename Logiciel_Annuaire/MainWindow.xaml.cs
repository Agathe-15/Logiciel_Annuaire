using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AnnuaireWPF.Models;
using AnnuaireWPF.Views;
using AnnuaireWPF.Services;

namespace Logiciel_Annuaire
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Employe> _employes;
        private ObservableCollection<Employe> _filteredEmployes;

        public MainWindow()
        {
            InitializeComponent(); // Assurez-vous que cette ligne n'est pas commentée
            _apiService = new ApiService();
            _employes = new ObservableCollection<Employe>();
            _filteredEmployes = new ObservableCollection<Employe>();
            EmployeListView.ItemsSource = _filteredEmployes;

            LoadEmployes();
        }

        private async void LoadEmployes()
        {
            var employes = await _apiService.GetAsync<ObservableCollection<Employe>>("employes");
            foreach (var employe in employes)
            {
                _employes.Add(employe);
                _filteredEmployes.Add(employe);
            }
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            _filteredEmployes.Clear();
            foreach (var emp in _employes.Where(e => e.Nom.ToLower().Contains(searchText)))
            {
                _filteredEmployes.Add(emp);
            }
        }

        private void OnEmployeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                var employeView = new EmployeView(selectedEmploye); // Instanciez la vue EmployeView
                employeView.Show(); // Affichez la nouvelle fenêtre
            }
        }
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Masque le placeholder lorsque l'utilisateur écrit
            PlaceholderText.Visibility = string.IsNullOrEmpty(SearchBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}
