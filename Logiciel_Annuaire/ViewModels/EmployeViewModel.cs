using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AnnuaireWPF.Models;
using AnnuaireWPF.Services;

namespace AnnuaireWPF.ViewModels
{
    public class EmployeViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<Employe> Employes { get; set; }

        public EmployeViewModel()
        {
            _apiService = new ApiService();
            Employes = new ObservableCollection<Employe>();
            LoadEmployes();
        }

        private async Task LoadEmployes()
        {
            var employes = await _apiService.GetAsync<List<Employe>>("employes");
            foreach (var employe in employes)
            {
                Employes.Add(employe);
            }
        }
        public void CloseWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close()
        {
            throw new NotImplementedException();
        }
    }
}
