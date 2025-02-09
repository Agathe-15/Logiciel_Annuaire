using System.Windows;
using Logiciel_Annuaire.src.Models;

namespace Logiciel_Annuaire.src.Views
{
    public partial class DepartementFormWindow : Window
    {
        public Departement DepartementData { get; private set; }

        public DepartementFormWindow(Departement departement = null)
        {
            InitializeComponent();
            DepartementData = departement ?? new Departement();
            if (departement != null)
            {
                NomTextBox.Text = departement.Nom;
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomTextBox.Text))
            {
                MessageBox.Show("Le nom du département est requis.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DepartementData.Nom = NomTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
