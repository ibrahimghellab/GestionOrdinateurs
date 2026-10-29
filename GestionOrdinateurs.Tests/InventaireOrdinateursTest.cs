using FluentAssertions;
using GestionOrdinateurs.Shared;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class InventaireOrdinateursTest
{
    private InventaireOrdinateurs _inventaire;

    public InventaireOrdinateursTest()
    {
        _inventaire = new InventaireOrdinateurs();
        _inventaire.Ordinateurs.Add(CreerOrdinateur(1, "Dell XPS 15"));
        _inventaire.Ordinateurs.Add(CreerOrdinateur(5, "HP EliteDesk 800"));
    }

    //Creation d'un "mock" d'ordinateur
    private static Ordinateur CreerOrdinateur(int id, string nom)
    {
        return new Ordinateur
        {
            Id = id,
            Nom = nom,
            Stock = 1,
            Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Fixe, "Dell", "Intel Core i5")
        };
    }


    [Fact]
    public void IndexeurParId_IdExistant_RenvoieLOrdinateur()
    {
        //Id 5 alors que l'ordinateur est en 2e position : la recherche se fait bien sur l'Id
        _inventaire[5]!.Nom.Should().Be("HP EliteDesk 800");
    }

    [Fact]
    public void IndexeurParId_IdInexistant_RenvoieNull()
    {
        _inventaire[999].Should().BeNull();
    }

    [Fact]
    public void IndexeurParNom_NomExistant_RenvoieLOrdinateurSansTenirCompteDeLaCasse()
    {
        _inventaire["dell xps 15"]!.Id.Should().Be(1);
    }

    [Fact]
    public void IndexeurParNom_NomInexistant_RenvoieNull()
    {
        _inventaire["Inconnu"].Should().BeNull();
    }

    [Fact]
    public void Count_RenvoieLeNombreDOrdinateurs()
    {
        _inventaire.Count.Should().Be(2);
    }
}
