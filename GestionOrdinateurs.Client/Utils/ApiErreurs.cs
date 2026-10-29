using System.Net;

namespace GestionOrdinateurs.Client.Utils;

//Traduit une erreur d'appel API en message lisible pour l'utilisateur
public static class ApiErreurs
{
    public static string Message(HttpRequestException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Votre session a expiré, veuillez vous reconnecter.",
            HttpStatusCode.Forbidden => "Vous n'avez pas les droits pour voir cette page.",
            null => "Le serveur est indisponible.",
            _ => $"Erreur lors de l'appel au serveur ({(int)ex.StatusCode})."
        };
    }
}
