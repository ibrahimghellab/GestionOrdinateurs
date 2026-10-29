using System.Net;
using Blazored.SessionStorage;
using GestionOrdinateurs.Client.Utils;

namespace GestionOrdinateurs.Client.Services;

public class AuthentificationService : IAuthentificationService
{
    private readonly HttpClient _httpClient;

    private readonly ISessionStorageService _sessionStorage;

    private readonly JwtAuthenticationStateProvider _authProvider;

    public AuthentificationService(HttpClient httpClient, ISessionStorageService sessionStorage, JwtAuthenticationStateProvider authProvider)
    {
        _httpClient = httpClient;
        _sessionStorage = sessionStorage;
        _authProvider = authProvider;
    }

    //Renvoie false si l'utilisateur n'existe pas
    public async Task<bool> ConnecterAsync(string login)
    {
        var response = await _httpClient.PostAsync($"api/Authentication/login?login={Uri.EscapeDataString(login)}", null);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        var token = await response.Content.ReadAsStringAsync();

        await _sessionStorage.SetItemAsync(JwtAuthenticationStateProvider.CleToken, token);
        _authProvider.NotifyUserAuthentication(token);
        return true;
    }

    public async Task DeconnecterAsync()
    {
        await _sessionStorage.RemoveItemAsync(JwtAuthenticationStateProvider.CleToken);
        _authProvider.NotifyUserLogout();
    }
}
