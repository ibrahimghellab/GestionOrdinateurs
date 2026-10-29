using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Services;

public interface IUserApiService
{
    Task<User?> GetUserByLoginAsync(string login);
}
