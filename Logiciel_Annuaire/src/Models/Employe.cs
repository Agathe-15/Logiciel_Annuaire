using System;
using System.ComponentModel;
using Newtonsoft.Json; // ✅ Import pour JsonProperty

namespace Logiciel_Annuaire.src.Models
{
    public class Employe : INotifyPropertyChanged
    {
        private int _departementId;
        private Departement _employeDepartement;
        private Site _site;

        // ✅ AJOUT DU CONSTRUCTEUR PAR DÉFAUT
        public Employe()
        {
            Nom = "";
            Prenom = "";
            Telephone = "";
            Email = "";
            EmployeId = 0; // ✅ Empêche les erreurs sur l'ID
            DepartementId = 0;
            EmployeDepartement = new Departement { DepartementId = 0, Nom = "Non attribué" };
            SiteId = 0;
            Site = new Site { SiteId = 0, Nom = "Non attribué" };
            DateEmbauche = DateTime.Now;
        }

        [JsonProperty("employe_id")] // ✅ Assure que `employe_id` de l'API est bien converti en `EmployeId`
        public int EmployeId { get; set; }

        [JsonProperty("nom")]
        public string Nom { get; set; }

        [JsonProperty("prenom")]
        public string Prenom { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("site_id")]
        public int SiteId
        {
            get => _site?.SiteId ?? 0;
            set
            {
                if (_site == null || _site.SiteId != value)
                {
                    _site = new Site { SiteId = value };
                    OnPropertyChanged(nameof(SiteId));
                }
            }
        }

        [JsonProperty("date_embauche")]
        public DateTime DateEmbauche { get; set; }

        public Site Site
        {
            get => _site;
            set
            {
                if (_site != value)
                {
                    _site = value;
                    OnPropertyChanged(nameof(Site));
                    OnPropertyChanged(nameof(SiteId)); // 🔥 Mettre à jour SiteId aussi
                }
            }
        }

        [JsonProperty("departement_id")]
        public int DepartementId
        {
            get => _departementId;
            set
            {
                if (_departementId != value)
                {
                    _departementId = value;
                    OnPropertyChanged(nameof(DepartementId));
                    OnPropertyChanged(nameof(DepartementNom));
                }
            }
        }

        public Departement EmployeDepartement
        {
            get => _employeDepartement;
            set
            {
                if (_employeDepartement != value)
                {
                    _employeDepartement = value;
                    OnPropertyChanged(nameof(EmployeDepartement));
                    OnPropertyChanged(nameof(DepartementNom)); // ✅ Notifier aussi `DepartementNom`
                }
            }
        }

        public string DepartementNom => EmployeDepartement?.Nom ?? "Non attribué";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
