using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Views;
using Logiciel_Annuaire.src.Utils;
using Newtonsoft.Json;

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

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Modifier un employé cliqué.");

            if (EmployeListView.SelectedItem is not Employe selectedEmploye)
            {
                Logger.Log("❌ Aucun employé sélectionné.");
                MessageBox.Show("Sélectionnez un employé à modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de modification pour ID={selectedEmploye.EmployeId}, Nom={selectedEmploye.Nom}, Prénom={selectedEmploye.Prenom}");

            if (selectedEmploye.EmployeId <= 0)
            {
                Logger.Log("❌ Erreur : L'employé sélectionné n'a pas d'ID valide !");
                MessageBox.Show("L'employé sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérifier si la base est déjà verrouillée
            if (await _apiService.IsAdminLockedAsync())
            {
                Logger.Log("❌ Modification impossible : Un autre administrateur est en train de modifier.");
                MessageBox.Show("❌ Modification impossible : un administrateur est déjà en train de modifier.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 🔍 Vérification du verrouillage admin
                Logger.Log("🔍 Vérification du verrouillage admin...");
                var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

                if (lockStatus.locked == true)
                {
                    Logger.Log("❌ Accès refusé : Un autre administrateur est déjà en modification.");
                    MessageBox.Show("❌ Un autre administrateur est déjà en modification. Réessayez plus tard.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔒 Tentative de verrouillage
                Logger.Log("🔒 Tentative de verrouillage de la base...");

                // ✅ Correction : Envoyer `{}` au lieu de `null`
                var lockResponse = await _apiService.PostAsync<dynamic>("admin/lock", new { action = "lock" });

                if (lockResponse == null || (lockResponse.message != null && lockResponse.message.ToString().Contains("❌")))
                {
                    Logger.Log("❌ Impossible de verrouiller la base. Un autre admin modifie déjà.");
                    MessageBox.Show("❌ Impossible de modifier. Un autre administrateur modifie déjà la base.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Logger.Log("✅ Base verrouillée avec succès. Modification autorisée.");

                // ✅ Ouverture de la fenêtre de modification
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
            finally
            {
                // 🔓 Assurer le déverrouillage même en cas d'erreur
                Logger.Log("🔓 Déverrouillage de la base...");
                await _apiService.PostAsync<dynamic>("admin/unlock", new { action = "unlock" }); // ✅ Correction ici aussi
                Logger.Log("✅ Base déverrouillée.");
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
            Logger.Log("📌 Bouton Supprimer un employé cliqué.");

            if (EmployeListView.SelectedItem is not Employe selectedEmploye)
            {
                Logger.Log("❌ Aucun employé sélectionné.");
                MessageBox.Show("Sélectionnez un employé à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de suppression pour Employé ID={selectedEmploye.EmployeId}, Nom={selectedEmploye.Nom} {selectedEmploye.Prenom}");

            if (selectedEmploye.EmployeId <= 0)
            {
                Logger.Log("❌ Erreur : L'employé sélectionné n'a pas d'ID valide !");
                MessageBox.Show("L'employé sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérification du verrouillage admin
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Suppression impossible : La base est verrouillée.");
                MessageBox.Show("❌ Impossible de supprimer un employé car la base est verrouillée.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 🔥 Confirmation avant suppression
                Logger.Log($"✅ Demande de confirmation pour suppression de l'employé ID={selectedEmploye.EmployeId}...");
                if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedEmploye.Nom} {selectedEmploye.Prenom} ?",
                                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    Logger.Log($"❌ Suppression annulée pour l'employé ID={selectedEmploye.EmployeId}.");
                    return;
                }

                // 🔥 Suppression en cours
                Logger.Log($"🚀 Suppression en cours de l'employé ID={selectedEmploye.EmployeId}...");
                await _apiService.DeleteAsync($"employes/{selectedEmploye.EmployeId}");
                _employes.Remove(selectedEmploye);
                EmployeListView.Items.Refresh();
                Logger.Log($"✔️ Employé ID={selectedEmploye.EmployeId} supprimé avec succès !");
                MessageBox.Show("✔️ Employé supprimé avec succès.", "Suppression", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de la suppression de l'employé ID={selectedEmploye.EmployeId} : {ex.Message}");
                MessageBox.Show($"❌ Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnManageSitesClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Ouverture de la fenêtre de gestion des sites.");
            var sitesWindow = new SitesWindow();
            sitesWindow.ShowDialog();
            Logger.Log("✅ Fenêtre SitesWindow fermée.");
        }
        private void OnManageDepartementsClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Ouverture de la gestion des départements.");
            var departementsWindow = new DepartementsWindow();
            departementsWindow.ShowDialog();
        }

    }
}
