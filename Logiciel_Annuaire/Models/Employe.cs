namespace AnnuaireWPF.Models
{
    public class Employe
    {
        public int EmployeId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Poste { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int SiteId { get; set; }
        public int ServiceId { get; set; }
        public DateTime DateEmbauche { get; set; }
        public string Site { get; set; }
        public string Ville { get; set; }
    }
}
