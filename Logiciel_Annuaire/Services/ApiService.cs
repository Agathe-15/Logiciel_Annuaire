using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AnnuaireWPF.Models;
using Newtonsoft.Json;

namespace AnnuaireWPF.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:3000/api/"); // URL de votre API
        }

        // GET : Récupérer toutes les données
        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }


        // POST : Ajouter une nouvelle donnée
        public async Task<T> PostAsync<T>(string endpoint, T data)
        {
            // Si l'objet est un Employe, remappons ses propriétés pour correspondre à l'API
            object jsonData;
            if (data is Employe employe)
            {
                jsonData = new
                {
                    nom = employe.Nom,
                    prenom = employe.Prenom,
                    poste = employe.Poste,
                    telephone = employe.Telephone,
                    email = employe.Email,
                    site_id = employe.SiteId,
                    date_embauche = employe.DateEmbauche
                };
            }
            else
            {
                jsonData = data; // Pour les autres objets, utiliser directement
            }

            // Sérialisation et envoi des données
            var json = JsonConvert.SerializeObject(jsonData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }


        // PUT : Mettre à jour une donnée
        public async Task<T> PutAsync<T>(string endpoint, T data)
        {
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }

        // DELETE : Supprimer une donnée
        public async Task DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }


    }
}
