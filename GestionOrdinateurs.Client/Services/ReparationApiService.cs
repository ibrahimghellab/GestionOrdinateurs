using System.Net.Http.Json;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

public class ReparationApiService : IReparationApiService
{
    private readonly HttpClient _httpClient;

    public ReparationApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<HistoriqueReparationRecord>> GetHistoriqueAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<HistoriqueReparationRecord>>("api/Reparations/historique") ?? [];
    }
}
