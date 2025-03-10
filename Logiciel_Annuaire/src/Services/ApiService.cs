﻿using Logiciel_Annuaire.src.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using Logiciel_Annuaire.src.Utils;
using System.Security.Policy;

namespace Logiciel_Annuaire.src.Services
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
            try
            {
                var response = await _httpClient.GetStringAsync(endpoint);
                Logger.Log($"🔍 Réponse brute API ({endpoint}) : {response}");

                // Désérialisation de la réponse JSON en type générique T
                var result = JsonConvert.DeserializeObject<T>(response, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore, // Ignore les valeurs nulles
                    MissingMemberHandling = MissingMemberHandling.Ignore // Ignore les propriétés manquantes
                });

                // 🔥 Vérification spécifique pour les employés
                if (result is List<Employe> employesList)
                {
                    foreach (var employe in employesList)
                    {
                        if (employe.EmployeDepartement == null && employe.DepartementId > 0)
                        {
                            employe.EmployeDepartement = new Departement
                            {
                                DepartementId = employe.DepartementId,
                                Nom = "Département inconnu" // Valeur par défaut si non fourni
                            };
                        }
                    }
                }
                else if (result is ObservableCollection<Employe> employesCollection)
                {
                    foreach (var employe in employesCollection)
                    {
                        if (employe.EmployeDepartement == null && employe.DepartementId > 0)
                        {
                            employe.EmployeDepartement = new Departement
                            {
                                DepartementId = employe.DepartementId,
                                Nom = "Département inconnu"
                            };
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de l'appel API ({endpoint}) : {ex.Message}");
                throw; // Propage l'erreur pour voir où ça bloque
            }
        }




        // POST : Ajouter une nouvelle donnée
        public async Task<T> PostAsync<T>(string endpoint, T data)
        {
            object jsonData;

            if (data is Employe employe)
            {
                jsonData = new
                {
                    nom = employe.Nom,
                    prenom = employe.Prenom,
                    departement_id = employe.DepartementId,
                    telephone = employe.Telephone,
                    email = employe.Email,
                    site_id = employe.SiteId,
                    date_embauche = employe.DateEmbauche
                };
            }
            else
            {
                // ✅ Correction : Si `data` est null, on envoie `{}` au lieu de `null`
                jsonData = data != null ? data : new { };
            }

            var json = JsonConvert.SerializeObject(jsonData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Logger.Log("🔍 JSON envoyé au backend: " + json); // ✅ Debug

            var response = await _httpClient.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = await response.Content.ReadAsStringAsync();
                Logger.Log("❌ Erreur API: " + errorMessage); // ✅ Voir le message d'erreur
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(result);
        }



        // Verification le statut du verrou
        public async Task<bool> IsAdminLockedAsync()
        {
            try
            {
                var response = await GetAsync<dynamic>("admin/lock-status");

                Logger.Log($"🔍 Réponse API (admin/lock-status) : {JsonConvert.SerializeObject(response)}");

                if (response != null && response.locked != null)
                {
                    return response.locked;
                }

                Logger.Log("⚠️ Réponse inattendue de l'API pour lock-status.");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors de la récupération du verrou admin : {ex.Message}");
                return false; // Ne pas bloquer l'application si l'API est inaccessible
            }
        }


        // Requete de vérouillage 
        public async Task<bool> LockDatabaseAsync()
        {
            try
            {
                var jsonBody = new { action = "lock" }; // 🔥 Envoyer un JSON valide
                var response = await PostAsync("admin/lock", jsonBody);

                if (response != null)
                {
                    Logger.Log("🔒 Base verrouillée avec succès.");
                    return true;
                }
                else
                {
                    Logger.Log("❌ Impossible de verrouiller la base.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"❌ Erreur lors du verrouillage de la base : {ex.Message}");
                return false;
            }
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
