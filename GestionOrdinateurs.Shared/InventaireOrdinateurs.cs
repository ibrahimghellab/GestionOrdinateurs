namespace GestionOrdinateurs.Shared;

//Collection d'ordinateurs accessible avec des crochets :
//inventaire[3] -> ordinateur d'Id 3, inventaire["Dell XPS 15"] -> ordinateur portant ce nom
public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    public int Count => Ordinateurs.Count;

    //Recherche par Id (et non par position dans la liste), null si introuvable
    public Ordinateur? this[int ordinateurId]
    {
        get
        {
            return Ordinateurs.FirstOrDefault(o => o.Id == ordinateurId);
        }
    }

    //Recherche par nom sans tenir compte de la casse, null si introuvable
    public Ordinateur? this[string nom]
    {
        get
        {
            return Ordinateurs.FirstOrDefault(o => string.Equals(o.Nom, nom, StringComparison.OrdinalIgnoreCase));
        }
    }
}
