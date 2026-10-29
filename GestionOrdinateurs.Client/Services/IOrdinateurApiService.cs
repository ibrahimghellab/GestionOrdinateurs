using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

public interface IOrdinateurApiService
{
    Task<List<Ordinateur>> GetOrdinateursAsync();
}
