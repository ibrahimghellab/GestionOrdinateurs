using FluentAssertions;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionOrdinateurs.Tests;

//Même collection que les autres tests qui écrivent dans les journaux : ils ne s'exécutent pas en même temps
[Collection("Journaux")]
public class OrdinateurServiceTest
{
    int IdInexistant = 9999;

    private ParcInformatiqueContext _context;

    private OrdinateurService _service;

    public OrdinateurServiceTest()
    {
        _context = CreerContexte();
        _service = new OrdinateurService(_context, new JournalService());
    }

    //Base en mémoire unique à chaque test
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    //Creation d'un "mock" d'ordinateur fixe disponible (Stock = 1)
    private static Ordinateur CreerOrdinateur(int id)
    {
        return new Ordinateur
        {
            Id = id,
            Nom = "PC-TEST",
            Stock = 1,
            Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Fixe, "Dell", "Intel Core i5")
        };
    }


    [Fact]
    public async Task Add_NouvelOrdinateur_AugmenteLeNombre()
    {
        var nombreAvant = (await _service.GetAllAsync()).Count;
        var ordinateur = CreerOrdinateur(await _service.GetNextIdAsync());

        var resultat = await _service.AddAsync(ordinateur);

        resultat.Should().BeTrue();
        (await _service.GetAllAsync()).Should().HaveCount(nombreAvant + 1);
    }

    [Fact]
    public async Task Add_NouvelOrdinateur_EstRetrouvableParId()
    {
        var ordinateur = CreerOrdinateur(await _service.GetNextIdAsync());

        await _service.AddAsync(ordinateur);

        (await _service.GetByIdAsync(ordinateur.Id)).Should().BeSameAs(ordinateur);
    }

    [Fact]
    public async Task Add_IdDejaExistant_RetourneFalseEtNAjouteRien()
    {
        var id = await _service.GetNextIdAsync();
        await _service.AddAsync(CreerOrdinateur(id));
        var nombreAvant = (await _service.GetAllAsync()).Count;

        var resultat = await _service.AddAsync(CreerOrdinateur(id));

        resultat.Should().BeFalse();
        (await _service.GetAllAsync()).Should().HaveCount(nombreAvant);
    }

    [Fact]
    public async Task Delete_OrdinateurExistant_RetourneTrueEtDiminueLeNombre()
    {
        var ordinateur = CreerOrdinateur(await _service.GetNextIdAsync());
        await _service.AddAsync(ordinateur);
        var nombreAvant = (await _service.GetAllAsync()).Count;

        var resultat = await _service.DeleteAsync(ordinateur.Id);

        resultat.Should().BeTrue();
        (await _service.GetAllAsync()).Should().HaveCount(nombreAvant - 1);
        (await _service.GetByIdAsync(ordinateur.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_OrdinateurInexistant_RetourneFalseEtNeChangeRien()
    {
        var nombreAvant = (await _service.GetAllAsync()).Count;

        var resultat = await _service.DeleteAsync(IdInexistant);

        resultat.Should().BeFalse();
        (await _service.GetAllAsync()).Should().HaveCount(nombreAvant);
    }

    [Fact]
    public async Task Update_OrdinateurExistant_MetAJourLeStockEtLePrix()
    {
        await _service.AddAsync(CreerOrdinateur(1));
        var modifie = CreerOrdinateur(1);
        modifie.Stock = 5;
        modifie.Prix = 999;

        var resultat = await _service.UpdateAsync(1, modifie);

        resultat.Should().BeTrue();
        var ordinateur = await _service.GetByIdAsync(1);
        ordinateur!.Stock.Should().Be(5);
        ordinateur.Prix.Should().Be(999);
    }

    [Fact]
    public async Task Update_OrdinateurInexistant_RetourneFalse()
    {
        var resultat = await _service.UpdateAsync(IdInexistant, CreerOrdinateur(IdInexistant));

        resultat.Should().BeFalse();
    }

    [Fact]
    public async Task GetDisponibles_RenvoieUniquementLesOrdinateursEnStock()
    {
        var enStock = CreerOrdinateur(1);
        var horsStock = CreerOrdinateur(2);
        horsStock.Stock = 0;
        await _service.AddAsync(enStock);
        await _service.AddAsync(horsStock);

        var disponibles = await _service.GetDisponiblesAsync();

        disponibles.Should().ContainSingle().Which.Should().BeSameAs(enStock);
    }

    [Fact]
    public async Task GetByMarque_RenvoieLesOrdinateursDeLaMarqueSansTenirCompteDeLaCasse()
    {
        var dell = CreerOrdinateur(1);
        var hp = CreerOrdinateur(2);
        hp.Caracteristiques.Marque = "HP";
        await _service.AddAsync(dell);
        await _service.AddAsync(hp);

        var resultats = await _service.GetByMarqueAsync("dell");

        resultats.Should().ContainSingle().Which.Should().BeSameAs(dell);
    }

    [Fact]
    public async Task GetParPrixDecroissant_TrieDuPlusCherAuMoinsCher()
    {
        var moyen = CreerOrdinateur(1);
        moyen.Prix = 1000;
        var cher = CreerOrdinateur(2);
        cher.Prix = 1500;
        var pasCher = CreerOrdinateur(3);
        pasCher.Prix = 500;
        await _service.AddAsync(moyen);
        await _service.AddAsync(cher);
        await _service.AddAsync(pasCher);

        var resultats = await _service.GetParPrixDecroissantAsync();

        //On compare les Id pour vérifier l'ordre exact
        resultats.Select(o => o.Id).Should().Equal(2, 1, 3);
    }

    [Fact]
    public void Diagnostiquer_OrdinateurHorsService_RenvoieRemplacement()
    {
        var ordinateur = CreerOrdinateur(1);
        ordinateur.Etat = EtatOrdinateur.HorsService;

        _service.Diagnostiquer(ordinateur).Should().Be("Remplacement");
    }

    [Fact]
    public void Diagnostiquer_OrdinateurLentAvecMoinsDe8GoDeRam_RenvoieAjouterRam()
    {
        var ordinateur = CreerOrdinateur(1);
        ordinateur.Etat = EtatOrdinateur.Lent;
        ordinateur.Caracteristiques.RamGo = 4;

        _service.Diagnostiquer(ordinateur).Should().Be("Ajouter RAM");
    }

    [Fact]
    public void Diagnostiquer_OrdinateurLentAvec8GoDeRamOuPlus_RenvoieAucuneAction()
    {
        //8 Go n'est pas < 8 : le cas "Ajouter RAM" ne doit pas correspondre
        var ordinateur = CreerOrdinateur(1);
        ordinateur.Etat = EtatOrdinateur.Lent;
        ordinateur.Caracteristiques.RamGo = 8;

        _service.Diagnostiquer(ordinateur).Should().Be("Aucune action");
    }

    [Fact]
    public void Diagnostiquer_OrdinateurEnService_RenvoieAucuneAction()
    {
        var ordinateur = CreerOrdinateur(1);
        ordinateur.Etat = EtatOrdinateur.EnService;

        _service.Diagnostiquer(ordinateur).Should().Be("Aucune action");
    }

    [Fact]
    public void CalculerStatistiques_AucunOrdinateur_RenvoieDesZeros()
    {
        var statistiques = _service.CalculerStatistiques();

        statistiques.Should().Be((0, 0, 0m, 0));
    }

    [Fact]
    public async Task CalculerStatistiques_RenvoieLeTotalLesDisponiblesLaValeurDuStockEtLesReparations()
    {
        var ordinateur1 = CreerOrdinateur(1);
        ordinateur1.Prix = 1000;
        ordinateur1.Stock = 2;
        var ordinateur2 = CreerOrdinateur(2);
        ordinateur2.Prix = 500;
        ordinateur2.Stock = 0;
        var ordinateur3 = CreerOrdinateur(3);
        ordinateur3.Prix = 250.5m;
        ordinateur3.Stock = 4;
        await _service.AddAsync(ordinateur1);
        await _service.AddAsync(ordinateur2);
        await _service.AddAsync(ordinateur3);
        _context.Reparations.Add(new Reparation { OrdinateurId = 1, TypeReparation = "Nettoyage", DateDebut = DateTime.Now, Statut = "Terminée", Technicien = "Karim" });
        await _context.SaveChangesAsync();

        var statistiques = _service.CalculerStatistiques();

        statistiques.TotalOrdinateur.Should().Be(3);
        statistiques.OrdiDisponibles.Should().Be(2);
        statistiques.ValeurStock.Should().Be(3002m); //1000 x 2 + 500 x 0 + 250,5 x 4
        statistiques.NbReparations.Should().Be(1);
    }

    [Fact]
    public async Task AddUpdateDelete_EcriventChaqueActionDansLeJournal()
    {
        var ordinateur = CreerOrdinateur(1);
        //Nom unique pour retrouver les lignes parmi celles des exécutions précédentes
        ordinateur.Nom = "PC-" + Guid.NewGuid();

        await _service.AddAsync(ordinateur);
        await _service.UpdateAsync(1, ordinateur);
        await _service.DeleteAsync(1);

        var date = $"{DateTime.Now:dd/MM/yyyy}";
        File.ReadAllLines(Path.Combine("Logs", "Ordinateurs", "ordinateurs.log"))
            .Where(ligne => ligne.Contains(ordinateur.Nom))
            .Should().Equal(
                $"{date} - {ordinateur.Nom} ajouté (Id 1)",
                $"{date} - {ordinateur.Nom} modifié (Id 1)",
                $"{date} - {ordinateur.Nom} supprimé (Id 1)");
    }
}
