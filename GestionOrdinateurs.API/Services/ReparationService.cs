using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

//Service gérant les réparations des ordinateurs
public class ReparationService : IReparationService
{
    private readonly ParcInformatiqueContext _context;

    private readonly IJournalService _journalService;

    //Un DbContext n'est pas thread-safe : une seule réparation à la fois l'utilise.
    //SemaphoreSlim plutôt que lock car on fait un await dans la section critique.
    private readonly SemaphoreSlim _accesContexte = new SemaphoreSlim(1, 1);

    public ReparationService(ParcInformatiqueContext context, IJournalService journalService)
    {
        _context = context;
        _journalService = journalService;
    }

    //Crée une réparation "En cours" pour l'ordinateur et l'enregistre en base
    private async Task<Reparation> CreerReparationAsync(Ordinateur ordinateur, string technicien)
    {
        var reparation = new Reparation
        {
            //Seulement la clé étrangère, pour ne pas risquer de recréer l'ordinateur
            OrdinateurId = ordinateur.Id,
            TypeReparation = "Révision générale",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = technicien
        };

        await _accesContexte.WaitAsync();
        try
        {
            _context.Reparations.Add(reparation);
            await _context.SaveChangesAsync();
        }
        finally
        {
            _accesContexte.Release();
        }

        return reparation;
    }

    //Simule une réparation de 2 secondes : création en base, attente, passage au statut "Terminée" puis écriture dans le journal
    public async Task SimulerReparationAsync(Ordinateur ordinateur, string technicien)
    {
        var reparation = await CreerReparationAsync(ordinateur, technicien);

        //L'attente est hors section critique : les réparations restent simultanées
        await Task.Delay(2000);

        await _accesContexte.WaitAsync();
        try
        {
            reparation.Statut = "Terminée";
            await _context.SaveChangesAsync();
        }
        finally
        {
            _accesContexte.Release();
        }

        Console.WriteLine($"{ordinateur.Nom} réparé");
        _journalService.EcrireReparation($"{ordinateur.Nom} réparé");
    }

    //Renvoie toutes les réparations, Include charge l'ordinateur de chacune
    public async Task<List<Reparation>> GetAllAsync()
    {
        return await _context.Reparations
            .Include(r => r.UnOrdinateur)
            .ToListAsync();
    }

    //Renvoie l'historique des réparations, de la plus récente à la plus ancienne
    //Le Select est traduit en SQL : seules les colonnes utiles sont lues, avec une jointure pour le nom de l'ordinateur
    public async Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync()
    {
        return await _context.Reparations
            .OrderByDescending(r => r.DateDebut)
            .Select(r => new HistoriqueReparationRecord(
                r.OrdinateurId,
                r.UnOrdinateur.Nom,
                r.Technicien,
                r.TypeReparation,
                r.DateDebut))
            .ToListAsync();
    }
}
