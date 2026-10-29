namespace GestionOrdinateurs.API.Services;

//Écrit les journaux de l'atelier dans Logs\Reparations\reparations.log et Logs\Ordinateurs\ordinateurs.log
public class JournalService : IJournalService
{
    //Chemins relatifs au dossier depuis lequel l'API est lancée
    private static readonly string DossierReparations = Path.Combine("Logs", "Reparations");
    private static readonly string DossierOrdinateurs = Path.Combine("Logs", "Ordinateurs");

    //Plusieurs réparations simultanées écrivent dans le même fichier :
    //sans verrou, deux StreamWriter ouverts en même temps sur un fichier lèvent une IOException
    private static readonly object _fichierLock = new object();

    public JournalService()
    {
        //Ne fait rien si les dossiers existent déjà
        Directory.CreateDirectory(DossierReparations);
        Directory.CreateDirectory(DossierOrdinateurs);
    }

    public void EcrireReparation(string message)
    {
        Ecrire(Path.Combine(DossierReparations, "reparations.log"), message);
    }

    public void EcrireOrdinateur(string message)
    {
        Ecrire(Path.Combine(DossierOrdinateurs, "ordinateurs.log"), message);
    }

    //Ajoute une ligne précédée de la date du jour à la fin du fichier, ex : "15/10/2026 - Dell XPS réparé"
    private void Ecrire(string cheminFichier, string message)
    {
        lock (_fichierLock)
        {
            //using appelle Dispose à la fin du bloc (même en cas d'exception) : le fichier est fermé
            //tout de suite, sans attendre le Garbage Collector
            using var writer = new StreamWriter(cheminFichier, append: true);

            writer.WriteLine($"{DateTime.Now:dd/MM/yyyy} - {message}");
        }
    }
}
