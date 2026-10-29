using FluentAssertions;
using GestionOrdinateurs.API.Services;
using Xunit;

namespace GestionOrdinateurs.Tests;

[Collection("Journaux")]
public class JournalServiceTest
{
    //Les fichiers sont écrits dans le dossier d'exécution des tests (bin\Debug\net10.0)
    private static readonly string FichierReparations = Path.Combine("Logs", "Reparations", "reparations.log");
    private static readonly string FichierOrdinateurs = Path.Combine("Logs", "Ordinateurs", "ordinateurs.log");

    private JournalService _service;

    public JournalServiceTest()
    {
        _service = new JournalService();
    }


    [Fact]
    public void Constructeur_CreeLesDossiersDesJournaux()
    {
        Directory.Exists(Path.Combine("Logs", "Reparations")).Should().BeTrue();
        Directory.Exists(Path.Combine("Logs", "Ordinateurs")).Should().BeTrue();
    }

    [Fact]
    public void EcrireReparation_AjouteLaLigneAvecLaDateDuJourALaFinDuFichier()
    {
        //Guid : message unique, pour ne pas confondre avec les lignes des exécutions précédentes
        var message = "Test réparation " + Guid.NewGuid();

        _service.EcrireReparation(message);

        File.ReadAllLines(FichierReparations).Last()
            .Should().Be($"{DateTime.Now:dd/MM/yyyy} - {message}");
    }

    [Fact]
    public void EcrireOrdinateur_AjouteLaLigneDansLeJournalDesOrdinateurs()
    {
        var message = "Test ordinateur " + Guid.NewGuid();

        _service.EcrireOrdinateur(message);

        File.ReadAllLines(FichierOrdinateurs).Last()
            .Should().Be($"{DateTime.Now:dd/MM/yyyy} - {message}");
    }

    [Fact]
    public void EcrireReparation_DixFoisEnParallele_EcritLesDixLignes()
    {
        var identifiant = Guid.NewGuid().ToString();

        //Même situation que plusieurs réparations simultanées : sans le lock, une IOException est levée
        Parallel.For(
            0,
            10,
            i => _service.EcrireReparation($"{identifiant} - réparation {i}"));

        File.ReadAllLines(FichierReparations)
            .Count(ligne => ligne.Contains(identifiant))
            .Should().Be(10);
    }
}
