using Logiciel_Annuaire.src.Models;
using System.Windows.Controls;
using Xunit;
using Logiciel_Annuaire.src.Utils;
using Logiciel_Annuaire;

public class EditEmployeWindowTests // <-- Classe englobante requise
{
    [Fact]

    public void SiteComboBox_SelectionChanged_SetsUpdatedEmployeSiteIdCorrectly()
    {
        // Arrange : Création d'un employé et d'une fenêtre
        var employe = new Employe();
        var window = new TestEmptyWindow(employe);

        // Création d'un ComboBox simulé
        var comboBox = new ComboBox();
        var site = new Site { SiteId = 1, Nom = "Site A" };

        // Ajout du site à la liste des éléments du ComboBox
        comboBox.Items.Add(site);
        comboBox.SelectedItem = site; // Simulation d'une sélection utilisateur

        // Act : Appel de la méthode à tester
        window.TestableSiteComboBox_SelectionChanged(comboBox, null);

        // Assert : Vérification du résultat attendu
        Assert.Equal(1, window.UpdatedEmploye.SiteId);
        Logger.Log("✅ Test réussi : SiteId mis à jour correctement.");
    }

}

public partial class TestEmptyWindow
{
    private Employe newEmploye;

    public TestEmptyWindow(Employe newEmploye)
    {
        this.newEmploye = newEmploye;
    }

    public TestEmptyWindow() : this(new Employe())
    {
    }

    public Employe UpdatedEmploye { get; internal set; }
    public AdminWindow Owner { get; internal set; }

    public void TestableSiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SiteComboBox_SelectionChanged(sender, e);
    }

    private void SiteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    internal bool ShowDialog()
    {
        return true; // Simule un succès
    }
}

