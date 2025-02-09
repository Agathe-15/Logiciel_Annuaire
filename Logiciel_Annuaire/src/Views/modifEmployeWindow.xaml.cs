using System;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using System.Threading.Tasks;

namespace Logiciel_Annuaire.src.Views
{
    public partial class modifEmployeWindow : Window
    {
        private readonly ApiService _apiService;
        private Employe UpdatedEmploye;

        public modifEmployeWindow(Employe employeToEdit)
        {
            InitializeComponent(); // Si cette ligne plante, ton XAML est mal lié !
            _apiService = new ApiService();
            _ = LoadSitesAsync();
            _ = LoadDepartementsAsync();

            UpdatedEmploye = employeToEdit ?? new Employe();

            Logger.Log($"📌 Employé chargé -> ID={UpdatedEmploye.EmployeId}, Nom={UpdatedEmploye.Nom}, Prénom={UpdatedEmploye.Prenom}");

            if (UpdatedEmploye.EmployeId > 0)
            {
                Logger.Log($"📌 Mode modification pour {UpdatedEmploye.Nom} {UpdatedEmploye.Prenom} (ID={UpdatedEmploye.EmployeId})");

                //  Remplissage des champs avec les données de l'employé
                NomTextBox.Text = UpdatedEmploye.Nom;
                PrenomTextBox.Text = UpdatedEmploye.Prenom;
                TelephoneTextBox.Text = UpdatedEmploye.Telephone;
                EmailTextBox.Text = UpdatedEmploye.Email;
                DateEmbauchePicker.SelectedDate = UpdatedEmploye.DateEmbauche;
                SiteComboBox.SelectedValue = UpdatedEmploye.SiteId;
                DepartementComboBox.SelectedValue = UpdatedEmploye.DepartementId;
            }
        }
        private void OnCancelEditClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("❌ Modification annulée.");
            this.DialogResult = false;
            this.Close();
        }

        private async void OnSaveEditClick(object sender, RoutedEventArgs e)
        {
            if (UpdatedEmploye == null)
            {
                Logger.Log("❌ Erreur : Aucun employé à modifier !");
                return;
            }

            Logger.Log($"📌 Enregistrement des modifications pour {UpdatedEmploye.Nom} {UpdatedEmploye.Prenom} (ID={UpdatedEmploye.EmployeId})");

            UpdatedEmploye.Nom = NomTextBox.Text.Trim();
            UpdatedEmploye.Prenom = PrenomTextBox.Text.Trim();
            UpdatedEmploye.Telephone = TelephoneTextBox.Text.Trim();
            UpdatedEmploye.Email = EmailTextBox.Text.Trim();
            UpdatedEmploye.DateEmbauche = DateEmbauchePicker.SelectedDate ?? DateTime.Now;
            UpdatedEmploye.SiteId = (int)SiteComboBox.SelectedValue;
            UpdatedEmploye.DepartementId = (int)DepartementComboBox.SelectedValue;

            try
            {
                await _apiService.PutAsync($"employes/{UpdatedEmploye.EmployeId}", UpdatedEmploye);
                Logger.Log("✅ Modification enregistrée !");
                MessageBox.Show("✅ Employé modifié avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur API lors de l'opération : {ex.Message}");
                MessageBox.Show($"❌ Erreur API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSitesAsync()
        {
            try
            {
                var sites = await _apiService.GetAsync<List<Site>>("http://localhost:3000/api/sites");

                if (sites == null || sites.Count == 0)
                {
                    Logger.Log("⚠️ Aucun site récupéré depuis l'API !");
                    return;
                }

                foreach (var site in sites)
                {
                    Logger.Log($"📌 Site chargé -> ID: {site.SiteId}, Nom: {site.Nom}, Ville: {site.Ville}");
                }

                SiteComboBox.ItemsSource = sites;
                SiteComboBox.DisplayMemberPath = "Nom";
                SiteComboBox.SelectedValuePath = "SiteId";
                Logger.Log("✅ Sites chargés avec succès.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur de chargement des sites : {ex.Message}");
            }
        }

        private async Task LoadDepartementsAsync()
        {
            try
            {
                var departements = await _apiService.GetAsync<List<Departement>>("http://localhost:3000/api/departements");

                if (departements == null || departements.Count == 0)
                {
                    Logger.Log("⚠️ Aucun département récupéré depuis l'API !");
                    return;
                }

                foreach (var departement in departements)
                {
                    Logger.Log($"📌 Département chargé -> ID: {departement.DepartementId}, Nom: {departement.Nom}");
                }

                DepartementComboBox.ItemsSource = departements;
                DepartementComboBox.DisplayMemberPath = "Nom";
                DepartementComboBox.SelectedValuePath = "DepartementId";
                Logger.Log("✅ Départements chargés avec succès.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur de chargement des départements : {ex.Message}");
            }
        }
    }
}
