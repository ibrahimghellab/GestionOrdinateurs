namespace GestionOrdinateurs.API.Services;
using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.Shared;

public interface IOrdinateurService
{
    event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    Task<List<Ordinateur>> GetAllAsync();

    Task<Ordinateur?> GetByIdAsync(int id);

    Task<bool> AddAsync(Ordinateur ordinateur);

    Task<bool> DeleteAsync(int id);

    Task<bool> UpdateAsync(int id, Ordinateur updated);

    Task<int> GetNextIdAsync();

    Task<List<Ordinateur>> GetByMarqueAsync(string marque);

    Task<List<Ordinateur>> GetDisponiblesAsync();

    Task<List<Ordinateur>> GetParPrixDecroissantAsync();

    string Diagnostiquer(Ordinateur ordinateur);

    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations) CalculerStatistiques();
}
