using System.Net;
using System.Net.Http.Json;
using Blazored.SessionStorage;
using GestionOrdinateurs.Client.Utils;
using GestionOrdinateurs.Shared;

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

    //Renvoie false si l'utilisateur ou le mot de passe est incorrect
    public async Task<bool> ConnecterAsync(string login, string motDePasse)
    {
        var identifiants = new Identifiants { Login = login, MotDePasse = motDePasse };
        var response = await _httpClient.PostAsJsonAsync("api/Authentication/login-keycloak", identifiants);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return false;

        response.EnsureSuccessStatusCode();
        var tokenKeycloak = await response.Content.ReadFromJsonAsync<TokenKeycloak>();
        var token = tokenKeycloak?.AccessToken ?? "";

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
