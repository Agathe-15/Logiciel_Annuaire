using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AnnuaireWPF.Models;
using AnnuaireWPF.Services;
using Logiciel_Annuaire.Views;

namespace Logiciel_Annuaire
{
    public partial class AdminWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Employe> _employes;

        public AdminWindow()
        {
            InitializeComponent();

            _apiService = new ApiService();
            _employes = new ObservableCollection<Employe>();

            EmployeListView.ItemsSource = _employes;
            _ = LoadEmployesAsync();
        }

        private async Task LoadEmployesAsync()
        {
            try
            {
                var employes = await _apiService.GetAsync<ObservableCollection<Employe>>("employes");
                _employes.Clear();
                foreach (var emp in employes)
                    _employes.Add(emp);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Erreur de chargement : {ex.Message}");
            }
        }

        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            var newEmploye = new Employe();
            var editWindow = new EditEmployeWindow(newEmploye);

            if (editWindow.ShowDialog() == true)
            {
                try
                {
                    await _apiService.PostAsync("employes", newEmploye);
                    _employes.Add(newEmploye);
                    EmployeListView.Items.Refresh();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                var editWindow = new EditEmployeWindow(selectedEmploye);
                if (editWindow.ShowDialog() == true)
                {
                    try
                    {
                        await _apiService.PutAsync($"employes/{selectedEmploye.EmployeId}", selectedEmploye);
                        EmployeListView.Items.Refresh();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Erreur de modification : {ex.Message}");
                    }
                }
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                if (MessageBox.Show("Voulez-vous vraiment supprimer cet employé ?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _apiService.DeleteAsync($"employes/{selectedEmploye.EmployeId}");
                        _employes.Remove(selectedEmploye);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Erreur de suppression : {ex.Message}");
                    }
                }
            }
        }
    }
}
