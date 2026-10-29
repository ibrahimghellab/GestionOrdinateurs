namespace GestionOrdinateurs.Client.Services;

public interface IAuthentificationService
{
    Task<bool> ConnecterAsync(string login);

    Task DeconnecterAsync();
}
