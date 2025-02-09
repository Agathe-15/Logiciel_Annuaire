using System;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using System.Threading.Tasks;

namespace Logiciel_Annuaire.src.Views
{
    public partial class EditSiteWindow : Window
    {
        private readonly ApiService _apiService;
        private Site UpdatedSite;

        public EditSiteWindow(Site siteToEdit = null)
        {
            InitializeComponent();
            _apiService = new ApiService();
            UpdatedSite = siteToEdit ?? new Site();

            if (UpdatedSite.SiteId > 0)
            {
                NomTextBox.Text = UpdatedSite.Nom;
                VilleTextBox.Text = UpdatedSite.Ville;
                TypeTextBox.Text = UpdatedSite.Type;
                AdresseTextBox.Text = UpdatedSite.Adresse;
                TelephoneTextBox.Text = UpdatedSite.Telephone;
                EmailTextBox.Text = UpdatedSite.Email;
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("❌ Modification annulée.");
            this.DialogResult = false;
            this.Close();
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            UpdatedSite.Nom = NomTextBox.Text.Trim();
            UpdatedSite.Ville = VilleTextBox.Text.Trim();
            UpdatedSite.Type = TypeTextBox.Text.Trim();
            UpdatedSite.Adresse = AdresseTextBox.Text.Trim();
            UpdatedSite.Telephone = TelephoneTextBox.Text.Trim();
            UpdatedSite.Email = EmailTextBox.Text.Trim();

            try
            {
                if (UpdatedSite.SiteId > 0)
                    await _apiService.PutAsync($"sites/{UpdatedSite.SiteId}", UpdatedSite);
                else
                    await _apiService.PostAsync("sites", UpdatedSite);

                Logger.Log("✅ Modification enregistrée !");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur API : {ex.Message}");
                MessageBox.Show($"❌ Erreur API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
