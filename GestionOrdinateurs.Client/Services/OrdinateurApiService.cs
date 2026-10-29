using System.Net.Http.Json;
using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.Client.Services;

public class OrdinateurApiService : IOrdinateurApiService
{
    private readonly HttpClient _httpClient;

    public OrdinateurApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Ordinateur>> GetOrdinateursAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<Ordinateur>>("api/Ordinateurs") ?? [];
    }
}
