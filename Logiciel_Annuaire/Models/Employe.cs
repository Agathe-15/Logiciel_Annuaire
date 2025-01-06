using System;
using System.ComponentModel;

namespace AnnuaireWPF.Models
{
    public class Employe : INotifyPropertyChanged
    {
        private int posteId;
        private int siteId;

        public int EmployeId { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Poste { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public int PosteId
        {
            get { return posteId; }
            set
            {
                if (posteId != value)
                {
                    posteId = value;
                    OnPropertyChanged(nameof(PosteId));
                }
            }
        }
        public int SiteId
        {
            get { return siteId; }
            set
            {
                if (siteId != value)
                {
                    siteId = value;
                    OnPropertyChanged(nameof(SiteId));
                }
            }
        }
        public int ServiceId { get; set; }
        public DateTime DateEmbauche { get; set; }
        public string Ville { get; set; }

        // Propriété imbriquée pour le site
        public Site Site { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
