using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class OrdinateurService:IOrdinateurService
{
    private readonly ParcInformatiqueContext _context;

    private readonly IJournalService _journalService;

    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    public OrdinateurService(ParcInformatiqueContext context, IJournalService journalService)
    {
        _context = context;
        _journalService = journalService;
    }

    public async Task<List<Ordinateur>> GetAllAsync()
    {
        return await _context.Ordinateurs.ToListAsync();
    }

    public async Task<Ordinateur?> GetByIdAsync(int id)
    {
        return await _context.Ordinateurs.FirstOrDefaultAsync(o => o.Id == id);
    }

    //Ajoute un ordinateur, retourne false si l'Id existe déjà
    public async Task<bool> AddAsync(Ordinateur ordinateur)
    {
        if (await _context.Ordinateurs.AnyAsync(o => o.Id == ordinateur.Id))
            return false;

        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync();
        _journalService.EcrireOrdinateur($"{ordinateur.Nom} ajouté (Id {ordinateur.Id})");
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs { Ordinateur = ordinateur });
        return true;
    }

    //Supprime l'ordinateur, retourne false s'il n'existe pas
    public async Task<bool> DeleteAsync(int id)
    {
        var existant = await GetByIdAsync(id);
        if (existant is null)
            return false;

        _context.Ordinateurs.Remove(existant);
        await _context.SaveChangesAsync();
        _journalService.EcrireOrdinateur($"{existant.Nom} supprimé (Id {id})");
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs { Id = id });
        return true;
    }

    //Met à jour l'ordinateur avec les valeurs de updated, retourne false s'il n'existe pas
    public async Task<bool> UpdateAsync(int id, Ordinateur updated)
    {
        var existant = await GetByIdAsync(id);
        if (existant is null)
            return false;

        existant.Nom = updated.Nom;
        existant.Etat = updated.Etat;
        existant.Stock = updated.Stock;
        existant.Prix = updated.Prix;
        existant.Caracteristiques = updated.Caracteristiques;

        await _context.SaveChangesAsync();
        _journalService.EcrireOrdinateur($"{existant.Nom} modifié (Id {id})");
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs { Ordinateur = existant });
        return true;
    }

    //Renvoie l'Id maximum + 1 (0 + 1 si la table est vide)
    public async Task<int> GetNextIdAsync()
    {
        var idMax = await _context.Ordinateurs.MaxAsync(o => (int?)o.Id) ?? 0;
        return idMax + 1;
    }

    //Recherche par marque sans tenir compte de la casse
    public async Task<List<Ordinateur>> GetByMarqueAsync(string marque)
    {
        return await _context.Ordinateurs
            .Where(o => o.Caracteristiques.Marque.ToLower() == marque.ToLower())
            .ToListAsync();
    }

    //Ordinateurs en stock
    public async Task<List<Ordinateur>> GetDisponiblesAsync()
    {
        return await _context.Ordinateurs
            .Where(o => o.Stock > 0)
            .ToListAsync();
    }

    //Du plus cher au moins cher, puis par nom à prix égal
    public async Task<List<Ordinateur>> GetParPrixDecroissantAsync()
    {
        return await _context.Ordinateurs
            .OrderByDescending(o => o.Prix)
            .ThenBy(o => o.Nom)
            .ToListAsync();
    }

    //Renvoie l'action à mener selon l'état et les caractéristiques de l'ordinateur (premier cas qui correspond)
    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            {
                Etat: EtatOrdinateur.HorsService
            }
                => "Remplacement",

            {
                Etat: EtatOrdinateur.Lent,
                Caracteristiques:
                {
                    RamGo: < 8
                }
            }
                => "Ajouter RAM",

            _
                => "Aucune action"
        };
    }

    //Statistiques du parc renvoyées dans un tuple nommé, calculées en SQL (COUNT, SUM)
    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques()
    {
        var totalOrdinateur = _context.Ordinateurs.Count();
        var ordiDisponibles = _context.Ordinateurs.Count(o => o.Stock > 0);
        var nbReparations = _context.Reparations.Count();

        //Valeur du stock = somme de prix x quantité en stock
        var valeurStock = _context.Ordinateurs.Sum(o => o.Prix * o.Stock);

        return (totalOrdinateur, ordiDisponibles, valeurStock, nbReparations);
    }
}
