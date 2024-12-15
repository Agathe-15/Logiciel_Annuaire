using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Logiciel_Annuaire
{
    public partial class PasswordWindow : Window
    {
        public bool IsPasswordValid { get; private set; } = false;

        public PasswordWindow()
        {
            InitializeComponent();
        }

        private async void OnValidatePasswordClick(object sender, RoutedEventArgs e)
        {
            string inputPassword = PasswordBox.Password;

            try
            {
                // Récupérer le mot de passe haché depuis l'API
                string storedHashedPassword = await GetStoredPasswordHashFromApi();

                if (storedHashedPassword != null && VerifyPassword(inputPassword, storedHashedPassword))
                {
                    IsPasswordValid = true;
                    this.DialogResult = true; // Fermer la fenêtre avec succès
                }
                else
                {
                    MessageBox.Show("Mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la vérification du mot de passe : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Récupérer le mot de passe haché depuis l'API
        private async Task<string> GetStoredPasswordHashFromApi()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:3000/");

                try
                {
                    HttpResponseMessage response = await client.GetAsync("api/admin/password");

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new Exception("Mot de passe non trouvé dans la base de données.");
                    }
                    else
                    {
                        throw new Exception($"Erreur de l'API : {response.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Impossible de se connecter à l'API.", ex);
                }
            }
        }


        // Vérifier le mot de passe saisi avec le mot de passe haché
        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            string hashedInput = ComputeHash(inputPassword); // Hacher le mot de passe saisi
            return hashedInput == storedHashedPassword;      // Comparer les deux hachages
        }

        // Générer un hachage SHA-256
        private string ComputeHash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convertir chaque byte en hexadécimal
                }
                return builder.ToString();
            }
        }
    }
}
