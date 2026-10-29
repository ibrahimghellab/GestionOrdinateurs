using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Utils;

//Indique à Blazor qui est connecté, à partir du token enregistré en session
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    public const string CleToken = "jwtToken";

    private readonly ISessionStorageService _sessionStorage;

    public JwtAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetItemAsync<string>(CleToken);
        var identity = new ClaimsIdentity();

        //Un token expiré (15 min) ne doit plus faire apparaître l'utilisateur comme connecté
        if (!string.IsNullOrWhiteSpace(token) && new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo > DateTime.UtcNow)
        {
            identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    //Le token contient les noms courts "role" et "unique_name" : on les remet sous la forme attendue par Blazor
    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var token = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
        var claims = new List<Claim>();

        foreach (var claim in token.Claims)
        {
            if (claim.Type == "role")
                claims.Add(new Claim(ClaimTypes.Role, claim.Value));
            else if (claim.Type == "unique_name")
                claims.Add(new Claim(ClaimTypes.Name, claim.Value));
            else
                claims.Add(claim);
        }

        return claims;
    }
}
