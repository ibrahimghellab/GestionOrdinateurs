using System.Net;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;

    public KeycloakService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    //Renvoie null si l'utilisateur ou le mot de passe est incorrect
    public async Task<TokenKeycloak?> ConnecterAsync(Identifiants identifiants)
    {
        //grant_type "password" = Direct access grants : on transmet nous-mêmes le login et le mot de passe à Keycloak
        var formulaire = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = _configuration["Keycloak:ClientId"] ?? "",
            ["client_secret"] = _configuration["Keycloak:ClientSecret"] ?? "",
            ["username"] = identifiants.Login,
            ["password"] = identifiants.MotDePasse
        });

        var response = await _httpClient.PostAsync($"realms/{_configuration["Keycloak:Realm"]}/protocol/openid-connect/token", formulaire);

        //Keycloak répond 400 "invalid_grant" quand le login ou le mot de passe est faux
        if (response.StatusCode == HttpStatusCode.BadRequest)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TokenKeycloak>();
    }
}
