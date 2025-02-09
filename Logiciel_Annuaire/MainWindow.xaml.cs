using AnnuaireWPF.Views;
using Logiciel_Annuaire.src.Models;
using Logiciel_Annuaire.src.Services;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using Logiciel_Annuaire.src.Utils;
using System.Windows.Threading;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Logiciel_Annuaire
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Employe> _employes;
        private ObservableCollection<Employe> _filteredEmployes;
        private readonly DispatcherTimer _refreshTimer; // 🔥 Ajout du timer

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown; // Surveiller les touches

            _apiService = new ApiService();
            _employes = new ObservableCollection<Employe>();
            _filteredEmployes = new ObservableCollection<Employe>();
            EmployeListView.ItemsSource = _filteredEmployes;

            LoadEmployes();

            // 🔥 Initialisation du timer pour rafraîchir toutes les 2 minutes
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(30);
            _refreshTimer.Tick += (s, e) => LoadEmployes();
            _refreshTimer.Start();
            Logger.Log("🔄 Rafraîchissement automatique activé toutes les 30secs.");

            // Effectuer la mise à jour du charset sans bloquer l'interface utilisateur
            _ = UpdateDatabaseCharsetAsync();
        }

        private async Task UpdateDatabaseCharsetAsync()
        {
            using (HttpClient client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
            {
                client.BaseAddress = new Uri("http://localhost:3000/api/admin/");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("update-charset", null);

                    if (response.IsSuccessStatusCode)
                    {
                        Logger.Log("✅ La table Administrateur a été mise à jour pour utiliser utf8mb4.");
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Erreur lors de la mise à jour du charset : {response.StatusCode}\n{errorMessage}",
                            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Erreur de connexion à l'API : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void LoadEmployes()
        {
            try
            {
                Logger.Log("🔄 Rafraîchissement des employés en cours...");

                var employes = await _apiService.GetAsync<ObservableCollection<Employe>>("employes");
                var sites = await _apiService.GetAsync<ObservableCollection<Site>>("sites");

                _employes.Clear();
                _filteredEmployes.Clear();

                foreach (var employe in employes)
                {
                    var site = sites.FirstOrDefault(s => s.SiteId == employe.SiteId);
                    if (site != null)
                    {
                        employe.Site = site;
                    }

                    _employes.Add(employe);
                    _filteredEmployes.Add(employe);
                }

                Logger.Log("✅ Liste des employés mise à jour.");
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors du rafraîchissement des employés : {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des employés : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Veuillez entrer un texte pour la recherche.", "Recherche");
                return;
            }

            Logger.Log("📌 Liste complète des employés avant filtrage :");
            foreach (var emp in _employes)
            {
                Logger.Log($"EmployeId: {emp.EmployeId}, Nom: {emp.Nom}, Prénom: {emp.Prenom}, Département: {emp.DepartementId}, Site: {emp.Site?.Nom ?? "Aucun"}");
            }

            var results = _employes.Where(emp =>
                emp.Nom.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                emp.Prenom.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (emp.Site?.Nom?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (emp.Site?.Ville?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                emp.DepartementId.ToString().Contains(searchText)
            ).ToList();

            _filteredEmployes.Clear();
            foreach (var emp in results)
            {
                _filteredEmployes.Add(emp);
            }

            if (!_filteredEmployes.Any())
            {
                MessageBox.Show("Aucun résultat trouvé pour votre recherche.", "Recherche");
            }

            Logger.Log($"🔍 Résultats après filtrage pour '{searchText}' :");
            foreach (var emp in results)
            {
                Logger.Log($"✅ Match: EmployeId: {emp.EmployeId}, Nom: {emp.Nom}, Site: {emp.Site?.Nom ?? "Aucun"}");
            }
        }

        private void OnEmployeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                var employeView = new EmployeView(selectedEmploye);
                employeView.Show();
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.D)
                {
                    PromptForAdminAccess();
                }
            }
        }

        private void PromptForAdminAccess()
        {
            PasswordWindow passwordWindow = new PasswordWindow();
            if (passwordWindow.ShowDialog() == true && passwordWindow.IsPasswordValid)
            {
                OpenAdminInterface();
            }
            else
            {
                MessageBox.Show("Accès refusé.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenAdminInterface()
        {
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            _refreshTimer?.Stop();
            Logger.Log("⏹️ Rafraîchissement automatique arrêté.");
            base.OnClosed(e);
        }
    }
}
