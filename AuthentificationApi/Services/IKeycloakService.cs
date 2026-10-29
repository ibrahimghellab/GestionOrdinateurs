using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface IKeycloakService
{
    Task<TokenKeycloak?> ConnecterAsync(Identifiants identifiants);
}
