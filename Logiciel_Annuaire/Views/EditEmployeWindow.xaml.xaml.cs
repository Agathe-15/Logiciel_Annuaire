using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AnnuaireWPF.Models;
using AnnuaireWPF.Services;

namespace Logiciel_Annuaire.Views
{
    public partial class EditEmployeWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Site> _sites;
        private ObservableCollection<Departement> _postes;

        public ObservableCollection<Site> Sites
        {
            get => _sites;
            set
            {
                _sites = value;
                OnPropertyChanged(nameof(Sites));
            }
        }

        public ObservableCollection<Departement> Postes
        {
            get => _postes;
            set
            {
                _postes = value;
                OnPropertyChanged(nameof(Postes));
            }
        }

        public Employe UpdatedEmploye { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public EditEmployeWindow(Employe employeToEdit)
        {
            InitializeComponent();
            _apiService = new ApiService();
            UpdatedEmploye = employeToEdit ?? new Employe();

            Sites = new ObservableCollection<Site>();
            Postes = new ObservableCollection<Departement>();

            _ = LoadSitesAsync();
            _ = LoadPostesAsync();

            DataContext = this;
            InitializeFields(employeToEdit);
        }

        private void SiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SiteComboBox.SelectedValue != null)
            {
                UpdatedEmploye.SiteId = (int)SiteComboBox.SelectedValue;
                Console.WriteLine($"Site sélectionné : {SiteComboBox.Text}, SiteId : {UpdatedEmploye.SiteId}");
            }
            else
            {
                UpdatedEmploye.SiteId = 0;
                Console.WriteLine("Aucun site sélectionné");
            }
        }

        private void PosteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PosteComboBox.SelectedValue != null)
            {
                UpdatedEmploye.PosteId = (int)PosteComboBox.SelectedValue;
                Console.WriteLine($"Poste sélectionné : {PosteComboBox.Text}, PosteId : {UpdatedEmploye.PosteId}");
            }
            else
            {
                UpdatedEmploye.PosteId = 0;
                Console.WriteLine("Aucun poste sélectionné");
            }
        }


        private async Task LoadSitesAsync()
        {
            try
            {
                var sites = await _apiService.GetAsync<ObservableCollection<Site>>("sites");
                Sites.Clear();
                foreach (var site in sites)
                {
                    Sites.Add(site);
                }
                // Debug : Vérifiez que les sites sont bien chargés
                Console.WriteLine($"Sites chargés : {Sites.Count}");
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Erreur de chargement des sites : {ex.Message}");
            }
        }

        private async Task LoadPostesAsync()
        {
            try
            {
                var departements = await _apiService.GetAsync<ObservableCollection<Departement>>("departements");
                Postes.Clear();
                foreach (var poste in departements)
                {
                    Postes.Add(poste);
                }
                // Debug : Vérifiez que les postes sont bien chargés
                Console.WriteLine($"Postes chargés : {Postes.Count}");
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Erreur de chargement des postes : {ex.Message}");
            }
        }

        private void InitializeFields(Employe employeToEdit)
        {
            NomTextBox.Text = employeToEdit.Nom;
            PrenomTextBox.Text = employeToEdit.Prenom;
            TelephoneTextBox.Text = employeToEdit.Telephone;
            EmailTextBox.Text = employeToEdit.Email;
            DateEmbauchePicker.SelectedDate = employeToEdit.DateEmbauche == default ? DateTime.Now : employeToEdit.DateEmbauche;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            UpdatedEmploye.SiteId = SiteComboBox.SelectedValue != null
          ? (int)SiteComboBox.SelectedValue
          : 0; // Valeur par défaut

            UpdatedEmploye.PosteId = PosteComboBox.SelectedValue != null
                ? (int)PosteComboBox.SelectedValue
                : 0; // Valeur par défaut
            UpdatedEmploye.Nom = NomTextBox.Text.Trim();
            UpdatedEmploye.Prenom = PrenomTextBox.Text.Trim();
            UpdatedEmploye.Telephone = TelephoneTextBox.Text.Trim();
            UpdatedEmploye.Email = EmailTextBox.Text.Trim();
            UpdatedEmploye.DateEmbauche = DateEmbauchePicker.SelectedDate ?? DateTime.Now;

            this.DialogResult = true;
            this.Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer le nom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PrenomTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer le prénom.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelephoneTextBox.Text))
            {
                MessageBox.Show("Veuillez entrer le téléphone.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !EmailTextBox.Text.Contains("@"))
            {
                MessageBox.Show("Veuillez entrer un email valide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (SiteComboBox.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un site.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (PosteComboBox.SelectedValue == null)
            {
                MessageBox.Show("Veuillez sélectionner un poste.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void LogError(Exception ex)
        {
            System.IO.File.AppendAllText("errors.log", $"{DateTime.Now}: {ex.Message}{Environment.NewLine}");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
