using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using AnnuaireWPF.Models;
using AnnuaireWPF.Views;
using AnnuaireWPF.Services;
using System.Windows.Input;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http;


namespace Logiciel_Annuaire
{
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Employe> _employes;
        private ObservableCollection<Employe> _filteredEmployes;

        public MainWindow()
        {
            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown; // Surveiller les touches

            _apiService = new ApiService();
            _employes = new ObservableCollection<Employe>();
            _filteredEmployes = new ObservableCollection<Employe>();
            EmployeListView.ItemsSource = _filteredEmployes;

            LoadEmployes();

            // Effectuer la mise à jour du charset sans bloquer l'interface utilisateur
            _ = UpdateDatabaseCharsetAsync();
        }

        private async Task UpdateDatabaseCharsetAsync()
        {
            using (HttpClient client = new HttpClient {  Timeout = TimeSpan.FromSeconds(10) })
            {
                // URL de l'API 
                client.BaseAddress = new Uri("http://localhost:3000/api/admin/");

                try
                {
                    HttpResponseMessage response = await client.PostAsync("update-charset", null);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("La table Administrateur a été mise à jour pour utiliser utf8mb4.");
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
                // Charger les employés avec leur SiteId
                var employes = await _apiService.GetAsync<ObservableCollection<Employe>>("employes");

                // Charger les sites
                var sites = await _apiService.GetAsync<ObservableCollection<Site>>("sites");

                // Associer le nom et la ville du site à chaque employé
                foreach (var employe in employes)
                {
                    var site = sites.FirstOrDefault(s => s.SiteId == employe.SiteId);
                    if (site != null)
                    {
                        employe.Site = site.Nom;
                        employe.Ville = site.Ville;
                    }

                    _employes.Add(employe);
                    _filteredEmployes.Add(employe);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des employés : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            // Récupérer le texte saisi dans le champ de recherche
            string searchText = SearchBox.Text.ToLower();

            // Vérifier si le champ de recherche est vide
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Veuillez entrer un texte pour la recherche.", "Recherche");
                return;
            }

            // Filtrer les employés par plusieurs critères
            var results = _employes.Where(emp =>
                emp.Nom.ToLower().Contains(searchText) ||                // Rechercher par nom
                emp.Prenom.ToLower().Contains(searchText) ||             // Rechercher par prénom
                (emp.Site?.ToLower().Contains(searchText) ?? false) ||   // Rechercher par site
                (emp.Ville?.ToLower().Contains(searchText) ?? false) ||  // Rechercher par ville
                emp.Poste.ToLower().Contains(searchText));               // Rechercher par poste

            // Mettre à jour la liste filtrée
            _filteredEmployes.Clear();
            foreach (var emp in results)
            {
                _filteredEmployes.Add(emp);
            }

            // Afficher un message si aucun résultat n'est trouvé
            if (!_filteredEmployes.Any())
            {
                MessageBox.Show("Aucun résultat trouvé pour votre recherche.", "Recherche");
            }
        }

        private void OnEmployeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (EmployeListView.SelectedItem is Employe selectedEmploye)
            {
                var employeView = new EmployeView(selectedEmploye); // Instancier la vue EmployeView
                employeView.Show(); // Afficher la nouvelle fenêtre
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Masquer le placeholder si du texte est saisi, sinon l'afficher
            PlaceholderText.Visibility = string.IsNullOrEmpty(SearchBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Vérifier si la combinaison Ctrl + D est pressée
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Key == Key.D)
                {
                    // Demander un mot de passe
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
        

    }
}
