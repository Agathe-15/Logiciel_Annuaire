using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace Logiciel_Annuaire.src.Views
{
    public partial class TestEmptyWindow : Window
    {
        private readonly ApiService _apiService;
        private Employe UpdatedEmploye;  // ✅ Ajout ici, variable globale pour modification

        // 🔥 Constructeur pour modification OU ajout
        public TestEmptyWindow(Employe employeToEdit = null)
        {
            Logger.Log("📌 Début de l'initialisation de TestEmptyWindow...");

            try
            {
                InitializeComponent(); // ✅ Vérification si le chargement XAML pose problème
                Logger.Log("✅ InitializeComponent exécuté.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de InitializeComponent : {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement de la fenêtre : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _apiService = new ApiService();
            _ = LoadSitesAsync();
            _ = LoadDepartementsAsync();

            Logger.Log("📌 TestEmptyWindow ouverte.");
            System.Threading.Thread.Sleep(2000); // 🔥 TEMPORAIRE : Attends 2 secondes pour voir la fenêtre


            UpdatedEmploye = employeToEdit ?? new Employe();
            Logger.Log($"📌 Employé chargé -> ID={UpdatedEmploye.EmployeId}, Nom={UpdatedEmploye.Nom}, Prénom={UpdatedEmploye.Prenom}");

            if (UpdatedEmploye.EmployeId > 0)
            {
                Logger.Log($"📌 Mode modification pour {UpdatedEmploye.Nom} {UpdatedEmploye.Prenom} (ID={UpdatedEmploye.EmployeId})");
                NomTextBox.Text = UpdatedEmploye.Nom;
                PrenomTextBox.Text = UpdatedEmploye.Prenom;
                TelephoneTextBox.Text = UpdatedEmploye.Telephone;
                EmailTextBox.Text = UpdatedEmploye.Email;
                DateEmbauchePicker.SelectedDate = UpdatedEmploye.DateEmbauche;
                SiteComboBox.SelectedValue = UpdatedEmploye.SiteId;
                DepartementComboBox.SelectedValue = UpdatedEmploye.DepartementId;
            }
            else
            {
                Logger.Log("📌 Mode ajout d'un nouvel employé.");
            }
        }



        // 🔥 Sauvegarde des modifications
        // 🔥 Vérification des valeurs sélectionnées
        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrenomTextBox.Text) ||
                string.IsNullOrWhiteSpace(TelephoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                SiteComboBox.SelectedValue == null ||
                DepartementComboBox.SelectedValue == null)
            {
                Logger.Log("❌ Erreur : Un champ obligatoire est vide.");
                MessageBox.Show("Veuillez remplir tous les champs obligatoires.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SiteComboBox.SelectedValue == null || DepartementComboBox.SelectedValue == null)
            {
                Logger.Log("❌ Erreur : Site ou département non sélectionné !");
                MessageBox.Show("Veuillez sélectionner un site et un département.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int siteId = (int)SiteComboBox.SelectedValue;
            int departementId = (int)DepartementComboBox.SelectedValue;

            Logger.Log($"🔍 Site sélectionné: ID={siteId}");
            Logger.Log($"🔍 Département sélectionné: ID={departementId}");

            if (siteId <= 0 || departementId <= 0)
            {
                Logger.Log("❌ Erreur : Site et département doivent être sélectionnés !");
                MessageBox.Show("Veuillez sélectionner un site et un département.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔥 Création de l'objet employé
            var employeData = new
            {
                employe_id = UpdatedEmploye != null && UpdatedEmploye.EmployeId > 0 ? UpdatedEmploye.EmployeId : 0,
                nom = NomTextBox.Text.Trim(),
                prenom = PrenomTextBox.Text.Trim(),
                telephone = TelephoneTextBox.Text.Trim(),
                email = EmailTextBox.Text.Trim(),
                date_embauche = DateEmbauchePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                site_id = siteId,
                departement_id = departementId
            };

            try
            {
                if (UpdatedEmploye != null && UpdatedEmploye.EmployeId > 0)
                {
                    Logger.Log($"📌 Modification de l'employé : ID={UpdatedEmploye.EmployeId}, Nom={UpdatedEmploye.Nom}, Prénom={UpdatedEmploye.Prenom}");
                    await _apiService.PutAsync($"employes/{UpdatedEmploye.EmployeId}", employeData);
                    Logger.Log("✅ Modification enregistrée avec succès !");
                    MessageBox.Show("✅ Employé modifié avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Logger.Log($"📌 Ajout d'un nouvel employé : {employeData.nom} {employeData.prenom}...");
                    await _apiService.PostAsync("employes", employeData);
                    Logger.Log("✅ Employé ajouté avec succès !");
                    MessageBox.Show("✅ Employé ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur API lors de l'opération : {ex.Message}");
                MessageBox.Show($"❌ Erreur API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // 🔥 Chargement des sites
        private async Task LoadSitesAsync()
        {
            try
            {
                var sites = await _apiService.GetAsync<List<Site>>("http://localhost:3000/api/sites"); // ✅ Correction ici

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


        // 🔥 Chargement des départements
        private async Task LoadDepartementsAsync()
        {
            try
            {
                var departements = await _apiService.GetAsync<List<Departement>>("http://localhost:3000/api/departements"); // ✅ Correction ici

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

       //  🔥 Annuler la modification
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("❌ Modification annulée par l'utilisateur.");
            this.DialogResult = false;
            this.Close();
        }
    }
}
