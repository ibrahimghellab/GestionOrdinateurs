namespace GestionOrdinateurs.API.Services;

//Service gérant le stock de pièces de l'atelier (gardé en mémoire, pas en base)
public class StockService : IStockService
{
    public int StockSSD { get; private set; } = 10;

    //Verrou protégeant StockSSD des accès simultanés
    private readonly object _stockLock = new object();

    public async void ConsommerSSD()
    {
        //Simule le temps de récupération de la pièce, hors du verrou pour que les attentes restent parallèles
        Task.Delay(10).Wait();

        //StockSSD-- (lire, calculer, écrire) ne doit pas être interrompu par un autre thread
        lock (_stockLock)
        {
            StockSSD--;
        }
    }
}
