namespace GestionOrdinateurs.Shared;

public class Reparation
{
    public int Id { get; set; }

    //Clé étrangère vers l'ordinateur réparé
    public int OrdinateurId { get; set; }

    //Propriété de navigation correspondant à OrdinateurId
    public Ordinateur UnOrdinateur { get; set; }

    //Ex : "Changement SSD", "Nettoyage"
    public string TypeReparation { get; set; }

    public DateTime DateDebut { get; set; }

    //"En cours" ou "Terminée"
    public string Statut { get; set; }

    public string Technicien { get; set; }
}
