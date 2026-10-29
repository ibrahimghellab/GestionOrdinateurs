using System.Net;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;

    //HttpClient fourni par HttpClientFactory, configuré dans Program.cs
    public UserApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserByLoginAsync(string login)
    {
        var response = await _httpClient.GetAsync($"api/Users/login/{Uri.EscapeDataString(login)}");

        //404 = utilisateur inconnu, les autres erreurs lèvent une exception
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<User>();
    }
}
