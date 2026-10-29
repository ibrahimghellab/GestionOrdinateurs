namespace GestionOrdinateurs.Client.Services;

public interface IAuthentificationService
{
    Task<bool> ConnecterAsync(string login, string motDePasse);

    Task DeconnecterAsync();
}
