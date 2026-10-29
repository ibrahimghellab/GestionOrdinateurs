using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;

//Noms des champs imposés par la réponse JSON de Keycloak
public class TokenKeycloak
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
