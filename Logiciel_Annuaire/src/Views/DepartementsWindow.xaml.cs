﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using Logiciel_Annuaire.src.Utils;
using Logiciel_Annuaire.src.Views;
using Newtonsoft.Json;

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
            Logger.Log("📌 Bouton Modifier un département cliqué.");

            if (DepartementsListView.SelectedItem is not Departement selectedDepartement)
            {
                Logger.Log("❌ Aucun département sélectionné.");
                MessageBox.Show("Sélectionnez un département à modifier.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de modification pour Département ID={selectedDepartement.DepartementId}, Nom={selectedDepartement.Nom}");

            if (selectedDepartement.DepartementId <= 0)
            {
                Logger.Log("❌ Erreur : Le département sélectionné n'a pas d'ID valide !");
                MessageBox.Show("Le département sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérifier si la base est déjà verrouillée
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Modification impossible : Un autre administrateur est en train de modifier.");
                MessageBox.Show("❌ Modification impossible : un administrateur est déjà en train de modifier.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔓 Déverrouillage forcé au cas où la base soit bloquée par une erreur précédente
            Logger.Log("🔓 Déverrouillage manuel avant tentative de verrouillage...");
            await _apiService.PostAsync<dynamic>("admin/unlock", new { action = "unlock" });

            try
            {
                // 🔒 Tentative de verrouillage
                Logger.Log("🔒 Tentative de verrouillage de la base...");
                var lockResponse = await _apiService.PostAsync<dynamic>("admin/lock", new { action = "lock" });

                if (lockResponse == null || (lockResponse.message != null && lockResponse.message.ToString().Contains("❌")))
                {
                    Logger.Log("❌ Impossible de verrouiller la base. Un autre administrateur modifie déjà.");
                    MessageBox.Show("❌ Impossible de modifier. Un autre administrateur modifie déjà la base.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Logger.Log("✅ Base verrouillée avec succès. Modification autorisée.");

                // ✅ Ouverture de la fenêtre de modification
                Logger.Log("📌 Tentative d'ouverture de DepartementFormWindow...");
                var editDepWindow = new DepartementFormWindow(selectedDepartement);
                Logger.Log("✅ Fenêtre instanciée avec succès.");

                bool? result = editDepWindow.ShowDialog();
                Logger.Log("✅ Fenêtre fermée.");

                if (result == true)
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
                else
                {
                    Logger.Log("❌ Modification annulée.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de l'ouverture de DepartementFormWindow : {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 🔓 Assurer le déverrouillage même en cas d'erreur
                Logger.Log("🔓 Déverrouillage de la base...");
                await _apiService.PostAsync<dynamic>("admin/unlock", new { action = "unlock" });
                Logger.Log("✅ Base déverrouillée.");
            }
        }




        // Supprimer un département
        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            Logger.Log("📌 Bouton Supprimer un département cliqué.");

            if (DepartementsListView.SelectedItem is not Departement selectedDepartement)
            {
                Logger.Log("❌ Aucun département sélectionné.");
                MessageBox.Show("Sélectionnez un département à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Logger.Log($"📌 Tentative de suppression pour Département ID={selectedDepartement.DepartementId}, Nom={selectedDepartement.Nom}");

            if (selectedDepartement.DepartementId <= 0)
            {
                Logger.Log("❌ Erreur : Le département sélectionné n'a pas d'ID valide !");
                MessageBox.Show("Le département sélectionné n'est pas valide !", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 🔍 Vérification du verrouillage admin
            Logger.Log("🔍 Vérification du verrouillage admin...");
            var lockStatus = await _apiService.GetAsync<dynamic>("admin/lock-status");

            Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(lockStatus)}");

            if (lockStatus != null && lockStatus.locked == true)
            {
                Logger.Log("❌ Suppression impossible : La base est verrouillée.");
                MessageBox.Show("❌ Impossible de supprimer un département car la base est verrouillée.", "Accès refusé", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 🔥 Vérifie s'il y a des employés associés à ce département
                Logger.Log($"🔍 Vérification des employés liés au département ID={selectedDepartement.DepartementId}...");
                var employes = await _apiService.GetAsync<List<Employe>>($"employes?departement_id={selectedDepartement.DepartementId}");

                if (employes != null && employes.Count > 0)
                {
                    Logger.Log($"❌ Impossible de supprimer le département ID={selectedDepartement.DepartementId} ({selectedDepartement.Nom}), {employes.Count} employé(s) y sont rattachés.");
                    MessageBox.Show($"❌ Impossible de supprimer {selectedDepartement.Nom}, car {employes.Count} employé(s) y sont rattachés.",
                        "Suppression impossible", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 🔥 Confirmation de la suppression
                Logger.Log($"✅ Aucun employé lié au département ID={selectedDepartement.DepartementId}. Demande de confirmation...");
                if (MessageBox.Show($"Voulez-vous vraiment supprimer {selectedDepartement.Nom} ?", "Confirmation",
                                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
