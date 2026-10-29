using System.Net.Http.Headers;
using Blazored.SessionStorage;

namespace GestionOrdinateurs.Client.Utils;

//Ajoute le token de la session dans le header Authorization de chaque requête envoyée
public class JwtHandler : DelegatingHandler
{
    private readonly ISessionStorageService _sessionStorage;

    public JwtHandler(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _sessionStorage.GetItemAsync<string>(JwtAuthenticationStateProvider.CleToken, cancellationToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
