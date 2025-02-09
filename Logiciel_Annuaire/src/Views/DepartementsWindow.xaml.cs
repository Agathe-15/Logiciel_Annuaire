using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using Logiciel_Annuaire.src.Views;

namespace Logiciel_Annuaire.src.Views
{
    public partial class DepartementsWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Departement> _departements;

        public DepartementsWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _departements = new ObservableCollection<Departement>();
            DepartementsListView.ItemsSource = _departements;
            _ = LoadDepartementsAsync();
        }

        // Charger les départements depuis l'API
        private async Task LoadDepartementsAsync()
        {
            try
            {
                var departements = await _apiService.GetAsync<List<Departement>>("departements");
                _departements.Clear();
                foreach (var dep in departements)
                {
                    Logger.Log($"📌 Département chargé -> ID: {dep.DepartementId}, Nom: {dep.Nom}");
                    _departements.Add(dep);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur de chargement des départements : {ex.Message}");
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ajouter un département
        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            var newDepWindow = new DepartementFormWindow();
            if (newDepWindow.ShowDialog() == true)
            {
                var newDep = newDepWindow.DepartementData;
                try
                {
                    await _apiService.PostAsync("departements", newDep);
                    Logger.Log($"✅ Département ajouté : {newDep.Nom}");
                    _ = LoadDepartementsAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log($"❌ Erreur lors de l'ajout du département : {ex.Message}");
                }
            }
        }

        // Modifier un département
        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (DepartementsListView.SelectedItem is not Departement selectedDepartement)
            {
                MessageBox.Show("Sélectionnez un département à modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editDepWindow = new DepartementFormWindow(selectedDepartement);
            if (editDepWindow.ShowDialog() == true)
            {
                var updatedDep = editDepWindow.DepartementData;
                try
                {
                    await _apiService.PutAsync($"departements/{selectedDepartement.DepartementId}", updatedDep);
                    Logger.Log($"✅ Département modifié : {updatedDep.Nom}");
                    _ = LoadDepartementsAsync();
                }
                catch (Exception ex)
                {
                    Logger.Log($"❌ Erreur lors de la modification du département : {ex.Message}");
                }
            }
        }

        // Supprimer un département
        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (DepartementsListView.SelectedItem is not Departement selectedDepartement)
            {
                Logger.Log("❌ Aucun département sélectionné pour suppression.");
                MessageBox.Show("Sélectionnez un département à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de suppression du département : ID={selectedDepartement.DepartementId}, Nom={selectedDepartement.Nom}");

            try
            {
                // Vérification des employés rattachés
                Logger.Log($"🔍 Vérification des employés liés au département ID={selectedDepartement.DepartementId}...");
                var employes = await _apiService.GetAsync<List<Employe>>($"employes?departement_id={selectedDepartement.DepartementId}");

                if (employes != null && employes.Count > 0)
                {
                    Logger.Log($"❌ Impossible de supprimer le département ID={selectedDepartement.DepartementId} ({selectedDepartement.Nom}), {employes.Count} employé(s) y sont rattachés.");
                    MessageBox.Show($"❌ Impossible de supprimer {selectedDepartement.Nom}, car {employes.Count} employé(s) y sont rattachés.",
                        "Suppression impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirme la suppression
                Logger.Log($"✅ Aucun employé lié au département ID={selectedDepartement.DepartementId}. Demande de confirmation...");
                if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedDepartement.Nom} ?",
                                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Logger.Log($"🚀 Suppression en cours du département ID={selectedDepartement.DepartementId}...");
                    await _apiService.DeleteAsync($"departements/{selectedDepartement.DepartementId}");
                    _departements.Remove(selectedDepartement);
                    DepartementsListView.Items.Refresh();
                    Logger.Log($"✔️ Département ID={selectedDepartement.DepartementId} supprimé avec succès !");
                    MessageBox.Show("✔️ Département supprimé avec succès.", "Suppression", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Logger.Log($"❌ Suppression annulée pour le département ID={selectedDepartement.DepartementId}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de la suppression du département ID={selectedDepartement.DepartementId} : {ex.Message}");
                MessageBox.Show($"❌ Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
