namespace GestionOrdinateurs.API.Services;

public interface IJournalService
{
    void EcrireReparation(string message);

    void EcrireOrdinateur(string message);
}
