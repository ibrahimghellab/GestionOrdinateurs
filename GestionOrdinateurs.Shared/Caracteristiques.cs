namespace GestionOrdinateurs.Shared;

//Caractéristiques techniques d'un ordinateur, sans Id : stockées dans la table Ordinateur (OwnsOne)
public class Caracteristiques
{
    public int RamGo { get; set; }

    public TypeOrdinateur TypeOrdi { get; set; }

    public int StockageGo { get; set; }

    public string Marque { get; set; }

    public string Processeur { get; set; }

    //Les noms des paramètres reprennent ceux des propriétés :
    //EF Core et le JSON de l'API peuvent ainsi utiliser ce constructeur pour recréer l'objet
    public Caracteristiques(int ramGo, int stockageGo, TypeOrdinateur typeOrdi, string marque, string processeur)
    {
        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
        Marque = marque;
        Processeur = processeur;
    }
}
