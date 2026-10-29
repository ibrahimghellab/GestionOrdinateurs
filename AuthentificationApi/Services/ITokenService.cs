using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface ITokenService
{
    string GenererToken(User user);
}
