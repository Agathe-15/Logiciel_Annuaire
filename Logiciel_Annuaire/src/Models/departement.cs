using Newtonsoft.Json;

namespace Logiciel_Annuaire.src.Models
{
    public class Departement
    {
        [JsonProperty("Departement_Id")]
        public int DepartementId { get; set; }
        [JsonProperty("nom")]
        public string Nom { get; set; }
    }
}
