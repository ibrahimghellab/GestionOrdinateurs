using FluentAssertions;
using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionOrdinateurs.Tests;

[Collection("Journaux")]
public class ReparationServiceTest
{
    //Unique à chaque test : xUnit crée une nouvelle instance de la classe par test
    private readonly string _nomBase = Guid.NewGuid().ToString();

    private ParcInformatiqueContext _context;

    private ReparationService _service;

    public ReparationServiceTest()
    {
        _context = CreerContexte();
        _service = new ReparationService(_context, new JournalService());
    }

    //Plusieurs contextes créés dans un même test partagent les mêmes données
    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(_nomBase)
                .Options;

        return new ParcInformatiqueContext(options);
    }

    //Creation d'un "mock" d'ordinateur fixe disponible (Stock = 1)
    private static Ordinateur CreerOrdinateur(int id)
    {
        return new Ordinateur
        {
            Id = id,
            Nom = "PC-TEST-" + id,
            Stock = 1,
            Caracteristiques = new Caracteristiques(16, 512, TypeOrdinateur.Fixe, "Dell", "Intel Core i5")
        };
    }

    //Une réparation doit toujours concerner un ordinateur existant
    private async Task<Ordinateur> AjouterOrdinateurAsync(int id)
    {
        var ordinateur = CreerOrdinateur(id);
        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync();
        return ordinateur;
    }


    [Fact]
    public async Task SimulerReparation_CreeUneReparationTermineePourLOrdinateur()
    {
        var ordinateur = await AjouterOrdinateurAsync(1);

        await _service.SimulerReparationAsync(ordinateur, "Karim");

        var reparations = await _service.GetAllAsync();
        reparations.Should().ContainSingle();
        reparations[0].OrdinateurId.Should().Be(ordinateur.Id);
        reparations[0].Statut.Should().Be("Terminée");
        reparations[0].Technicien.Should().Be("Karim");
    }

    [Fact]
    public async Task SimulerReparation_TroisEnParallele_CreeTroisReparationsTerminees()
    {
        var ordinateur1 = await AjouterOrdinateurAsync(1);
        var ordinateur2 = await AjouterOrdinateurAsync(2);
        var ordinateur3 = await AjouterOrdinateurAsync(3);

        //Même appel que l'endpoint SimulerReparation : sans le sémaphore, ce test lève une InvalidOperationException
        await Task.WhenAll(
            _service.SimulerReparationAsync(ordinateur1, "Karim"),
            _service.SimulerReparationAsync(ordinateur2, "Julie"),
            _service.SimulerReparationAsync(ordinateur3, "Thomas")
        );

        var reparations = await _service.GetAllAsync();
        reparations.Should().HaveCount(3);
        reparations.Should().OnlyContain(r => r.Statut == "Terminée");
        reparations.Select(r => r.OrdinateurId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task GetAll_AucuneReparation_RenvoieUneListeVide()
    {
        var reparations = await _service.GetAllAsync();

        reparations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_RenvoieLesReparationsAvecLesInformationsDeLOrdinateur()
    {
        await AjouterOrdinateurAsync(1);
        _context.Reparations.Add(new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Changement SSD",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = "Julie"
        });
        await _context.SaveChangesAsync();

        //Nouveau contexte sur la même base : l'ordinateur n'est pas déjà chargé en mémoire,
        //UnOrdinateur n'est donc rempli que si GetAllAsync fait bien l'Include
        var service = new ReparationService(CreerContexte(), new JournalService());

        var reparations = await service.GetAllAsync();

        reparations.Should().ContainSingle();
        reparations[0].UnOrdinateur.Should().NotBeNull();
        reparations[0].UnOrdinateur.Nom.Should().Be("PC-TEST-1");
    }

    [Fact]
    public async Task GetHistorique_AucuneReparation_RenvoieUneListeVide()
    {
        var historique = await _service.GetHistoriqueAsync();

        historique.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHistorique_RenvoieLesInformationsDeLaReparationEtLeNomDeLOrdinateur()
    {
        await AjouterOrdinateurAsync(1);
        var dateIntervention = new DateTime(2026, 10, 15, 9, 30, 0);
        _context.Reparations.Add(new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Changement SSD",
            DateDebut = dateIntervention,
            Statut = "Terminée",
            Technicien = "Julie"
        });
        await _context.SaveChangesAsync();

        var historique = await _service.GetHistoriqueAsync();

        historique.Should().ContainSingle();

        //Déconstruction du record, dans l'ordre du constructeur
        var (ordinateurId, nomOrdinateur, technicien, typeReparation, date) = historique[0];
        ordinateurId.Should().Be(1);
        nomOrdinateur.Should().Be("PC-TEST-1");
        technicien.Should().Be("Julie");
        typeReparation.Should().Be("Changement SSD");
        date.Should().Be(dateIntervention);
    }

    [Fact]
    public async Task GetHistorique_TrieDeLaPlusRecenteALaPlusAncienne()
    {
        await AjouterOrdinateurAsync(1);
        await AjouterOrdinateurAsync(2);
        await AjouterOrdinateurAsync(3);
        _context.Reparations.AddRange(
            new Reparation { OrdinateurId = 1, TypeReparation = "Nettoyage", DateDebut = new DateTime(2026, 10, 10), Statut = "Terminée", Technicien = "Karim" },
            new Reparation { OrdinateurId = 2, TypeReparation = "Nettoyage", DateDebut = new DateTime(2026, 10, 20), Statut = "Terminée", Technicien = "Julie" },
            new Reparation { OrdinateurId = 3, TypeReparation = "Nettoyage", DateDebut = new DateTime(2026, 10, 15), Statut = "Terminée", Technicien = "Thomas" });
        await _context.SaveChangesAsync();

        var historique = await _service.GetHistoriqueAsync();

        //On compare les Id des ordinateurs pour vérifier l'ordre exact
        historique.Select(h => h.OrdinateurId).Should().Equal(2, 3, 1);
    }

    [Fact]
    public async Task CreationReparation_EstEnregistreeEtRattacheeALOrdinateur()
    {
        await AjouterOrdinateurAsync(1);
        var reparation = new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Changement SSD",
            DateDebut = new DateTime(2026, 10, 15),
            Statut = "En cours",
            Technicien = "Thomas"
        };

        _context.Reparations.Add(reparation);
        await _context.SaveChangesAsync();

        //Nouveau contexte : la liste Reparations n'est remplie que si la relation HasMany/WithOne est bien configurée
        var ordinateur = await CreerContexte().Ordinateurs
            .Include(o => o.Reparations)
            .FirstAsync(o => o.Id == 1);

        reparation.Id.Should().BeGreaterThan(0);
        ordinateur.Reparations.Should().ContainSingle();
        ordinateur.Reparations[0].TypeReparation.Should().Be("Changement SSD");
        ordinateur.Reparations[0].Technicien.Should().Be("Thomas");
    }

    [Fact]
    public async Task SimulerReparation_EcritLaReparationDansLeJournal()
    {
        var ordinateur = CreerOrdinateur(1);
        //Nom unique pour retrouver la ligne parmi celles des exécutions précédentes
        ordinateur.Nom = "PC-" + Guid.NewGuid();
        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync();

        await _service.SimulerReparationAsync(ordinateur, "Karim");

        File.ReadAllLines(Path.Combine("Logs", "Reparations", "reparations.log")).Last()
            .Should().Be($"{DateTime.Now:dd/MM/yyyy} - {ordinateur.Nom} réparé");
    }
}
