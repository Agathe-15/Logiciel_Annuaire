using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Logiciel_Annuaire.src.Views
{
    public partial class SitesWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Site> _sites;

        public SitesWindow()
        {
            InitializeComponent();
            _apiService = new ApiService();
            _sites = new ObservableCollection<Site>();
            SitesListView.ItemsSource = _sites;
            _ = LoadSitesAsync();
        }

        private async Task LoadSitesAsync()
        {
            try
            {
                var sites = await _apiService.GetAsync<List<Site>>("http://localhost:3000/api/sites");
                _sites.Clear();
                foreach (var site in sites)
                    _sites.Add(site);
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur de chargement des sites : {ex.Message}");
                MessageBox.Show($"Erreur de chargement : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnAddClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Ajouter un site cliqué.");

            // 🔍 Vérifier si la base est verrouillée
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Ajout impossible : La base est verrouillée.");
                MessageBox.Show("❌ Impossible d'ajouter un site tant que la base est verrouillée.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Logger.Log("📌 Tentative d'ouverture de AddSiteWindow...");
                var addWindow = new EditSiteWindow();
                Logger.Log("✅ Fenêtre instanciée avec succès.");

                bool? result = addWindow.ShowDialog();
                Logger.Log("✅ Fenêtre fermée.");

                if (result == true)
                {
                    Logger.Log("✅ Site ajouté avec succès.");
                    _ = LoadSitesAsync();
                }
                else
                {
                    Logger.Log("❌ Ajout annulé.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de l'ajout d'un site : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ajout d'un site : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Modifier un site cliqué.");

            if (SitesListView.SelectedItem is not Site selectedSite)
            {
                Logger.Log("❌ Aucun site sélectionné.");
                MessageBox.Show("Sélectionnez un site à modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de modification pour Site ID={selectedSite.SiteId}, Nom={selectedSite.Nom}");

            if (selectedSite.SiteId <= 0)
            {
                Logger.Log("❌ Erreur : Le site sélectionné n'a pas d'ID valide !");
                MessageBox.Show("Le site sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérifier si la base est verrouillée
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Modification impossible : La base est verrouillée.");
                MessageBox.Show("❌ Impossible de modifier un site car la base est verrouillée.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Logger.Log("📌 Tentative d'ouverture de EditSiteWindow...");
                var editWindow = new EditSiteWindow(selectedSite);
                Logger.Log("✅ Fenêtre instanciée avec succès.");

                bool? result = editWindow.ShowDialog();
                Logger.Log("✅ Fenêtre fermée.");

                if (result == true)
                {
                    Logger.Log($"✅ Modification confirmée pour Site ID={selectedSite.SiteId} !");
                    _ = LoadSitesAsync();
                }
                else
                {
                    Logger.Log("❌ Modification annulée.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de l'ouverture de EditSiteWindow : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Supprimer un site cliqué.");

            if (SitesListView.SelectedItem is not Site selectedSite)
            {
                Logger.Log("❌ Aucun site sélectionné.");
                MessageBox.Show("Sélectionnez un site à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de suppression pour Site ID={selectedSite.SiteId}, Nom={selectedSite.Nom}");

            if (selectedSite.SiteId <= 0)
            {
                Logger.Log("❌ Erreur : Le site sélectionné n'a pas d'ID valide !");
                MessageBox.Show("Le site sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérifier si la base est verrouillée
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Suppression impossible : La base est verrouillée.");
                MessageBox.Show("❌ Impossible de supprimer un site car la base est verrouillée.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 🔥 Vérifie s'il y a des employés associés à ce site
                Logger.Log($"🔍 Vérification des employés liés au site ID={selectedSite.SiteId}...");
                var employes = await _apiService.GetAsync<List<Employe>>($"employes?site_id={selectedSite.SiteId}");

                if (employes != null && employes.Count > 0)
                {
                    Logger.Log($"❌ Impossible de supprimer le site ID={selectedSite.SiteId} ({selectedSite.Nom}), {employes.Count} employé(s) y sont rattachés.");
                    MessageBox.Show($"❌ Impossible de supprimer {selectedSite.Nom}, car {employes.Count} employé(s) y sont rattachés.",
                        "Suppression impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔥 Confirme la suppression
                Logger.Log($"✅ Aucun employé lié au site ID={selectedSite.SiteId}. Demande de confirmation...");
                if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedSite.Nom} ?", "Confirmation",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    Logger.Log($"🚀 Suppression en cours du site ID={selectedSite.SiteId}...");
                    await _apiService.DeleteAsync($"sites/{selectedSite.SiteId}");
                    _sites.Remove(selectedSite);
                    SitesListView.Items.Refresh();
                    Logger.Log($"✔️ Site ID={selectedSite.SiteId} supprimé avec succès !");
                    MessageBox.Show("✔️ Site supprimé avec succès.", "Suppression", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Logger.Log($"❌ Suppression annulée pour le site ID={selectedSite.SiteId}.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de la suppression du site ID={selectedSite.SiteId} : {ex.Message}");
                MessageBox.Show($"❌ Erreur lors de la suppression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
