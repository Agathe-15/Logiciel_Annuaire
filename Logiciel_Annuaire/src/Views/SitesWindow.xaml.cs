using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;

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

        private void OnAddClick(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditSiteWindow();
            bool? result = editWindow.ShowDialog();
            if (result == true)
                _ = LoadSitesAsync();
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (SitesListView.SelectedItem is Site selectedSite)
            {
                var editWindow = new EditSiteWindow(selectedSite);
                bool? result = editWindow.ShowDialog();
                if (result == true)
                    _ = LoadSitesAsync();
            }
            else
            {
                MessageBox.Show("❌ Aucun site sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            if (SitesListView.SelectedItem is Site selectedSite)
            {
                Logger.Log($"📌 Tentative de suppression du site : ID={selectedSite.SiteId}, Nom={selectedSite.Nom}");

                try
                {
                    // Vérifie s'il y a des employés associés à ce site
                    Logger.Log($"🔍 Vérification des employés liés au site ID={selectedSite.SiteId}...");
                    var employes = await _apiService.GetAsync<List<Employe>>($"employes?site_id={selectedSite.SiteId}");

                    if (employes != null && employes.Count > 0)
                    {
                        Logger.Log($"❌ Impossible de supprimer le site ID={selectedSite.SiteId} ({selectedSite.Nom}), {employes.Count} employé(s) y sont rattachés.");
                        MessageBox.Show($"❌ Impossible de supprimer {selectedSite.Nom}, car {employes.Count} employé(s) y sont rattachés.",
                            "Suppression impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Confirme la suppression
                    Logger.Log($"✅ Aucun employé lié au site ID={selectedSite.SiteId}. Demande de confirmation...");
                    if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedSite.Nom} ?",
                                        "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
            else
            {
                Logger.Log("❌ Aucun site sélectionné pour suppression.");
                MessageBox.Show("❌ Aucun site sélectionné.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
