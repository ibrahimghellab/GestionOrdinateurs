using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

public interface IReparationApiService
{
    Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync();
}
