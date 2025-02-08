using Newtonsoft.Json;

namespace Logiciel_Annuaire.src.Models
{
    public class Site
    {
        [JsonProperty("site_id")]
        public int SiteId { get; set; }
        [JsonProperty("nom")]
        public string Nom { get; set; }
        [JsonProperty("ville")]
        public string Ville { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("adresse")]
        public string Adresse { get; set; }
        [JsonProperty("telephone")]
        public string Telephone { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }


    }
}
