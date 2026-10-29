using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services;

public interface IReparationService
{
    Task SimulerReparationAsync(Ordinateur ordinateur, string technicien);

    Task<List<Reparation>> GetAllAsync();

    Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync();
}
