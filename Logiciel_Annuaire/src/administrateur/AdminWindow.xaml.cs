using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Views;
using Logiciel_Annuaire.src.Utils;

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
                var employes = await _apiService.GetAsync<List<Employe>>("http://localhost:3000/api/employes");

                if (employes == null || employes.Count == 0)
                {
                    Logger.Log("⚠️ Aucun employé récupéré depuis l'API !");
                    return;
                }

                _employes.Clear();
                foreach (var emp in employes)
                {
                    Logger.Log($"📌 Employé chargé -> ID: {emp.EmployeId}, Nom: {emp.Nom}, Prénom: {emp.Prenom}");
                    _employes.Add(emp);
                }

                EmployeListView.ItemsSource = _employes; // ✅ Assure que la liste est bien peuplée
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur de chargement des employés : {ex.Message}");
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 TEST : Début de OnAddClick()");

            try
            {
                Logger.Log("📌 Tentative d'instanciation de TestEmptyWindow...");
                var editWindow = new TestEmptyWindow(); // ✅ Création de la fenêtre
                Logger.Log("✅ Fenêtre instanciée avec succès.");

                Logger.Log("📌 Affichage de la fenêtre...");
                bool? result = editWindow.ShowDialog(); // ✅ Ouverture de la fenêtre
                Logger.Log("✅ Fenêtre fermée.");

                if (result == true)
                {
                    Logger.Log("✅ Un employé a été ajouté.");
                    _ = LoadEmployesAsync();
                }
                else
                {
                    Logger.Log("❌ Ajout annulé.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de l'ouverture de TestEmptyWindow : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ouverture de TestEmptyWindow : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Modifier cliqué.");

            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                Logger.Log($"📌 Tentative de modification pour ID={selectedEmploye.EmployeId}, Nom={selectedEmploye.Nom}, Prénom={selectedEmploye.Prenom}");

                if (selectedEmploye.EmployeId <= 0)
                {
                    Logger.Log("❌ Erreur : L'employé sélectionné n'a pas d'ID valide !");
                    MessageBox.Show("L'employé sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    Logger.Log("📌 Tentative d'ouverture de EditEmployeWindow...");
                    var editWindow = new modifEmployeWindow(selectedEmploye);
                    Logger.Log("✅ Fenêtre instanciée avec succès.");

                    bool? result = editWindow.ShowDialog();
                    Logger.Log("✅ Fenêtre fermée.");

                    if (result == true)
                    {
                        Logger.Log($"✅ Modification confirmée pour ID={selectedEmploye.EmployeId} !");
                        _ = LoadEmployesAsync();
                    }
                    else
                    {
                        Logger.Log("❌ Modification annulée.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"❌ Erreur lors de l'ouverture de EditEmployeWindow : {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                Logger.Log("❌ Aucun employé sélectionné.");
                MessageBox.Show("❌ Aucun employé sélectionné !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void OnTestClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 TEST : Ouverture de TestWindow.");

            try
            {
                var testWindow = new src.Views.TestEmptyWindow();
                testWindow.ShowDialog();
                Logger.Log("✅ TEST : Fenêtre TestWindow fermée.");
            }
            catch (Exception ex)
            {
              Logger.Log($"❌ Erreur lors de l'ouverture de TestWindow : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ouverture de TestWindow : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedEmploye.Nom} {selectedEmploye.Prenom} ?",
                                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _apiService.DeleteAsync($"employes/{selectedEmploye.EmployeId}");
                        _employes.Remove(selectedEmploye);
                        EmployeListView.Items.Refresh();
                        MessageBox.Show("✔️ Employé supprimé avec succès.", "Suppression", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Erreur de suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("❌ Aucun employé sélectionné pour la suppression.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
