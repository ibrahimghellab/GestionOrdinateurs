using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionOrdinateurs.Shared;

//Sans [Table], EF Core nommerait la table comme le DbSet ("Ordinateurs")
[Table("Ordinateur")]
public class Ordinateur
{
    public int Id { get; init; }

    public string Nom { get; set; }

    public EtatOrdinateur Etat { get; set; }

    //[DefaultValue] n'est qu'une information (Swagger) : c'est la valeur par défaut d'un int qui donne 0
    [DefaultValue(0)]
    public int Stock { get; set; }

    //decimal pour les montants : pas d'erreur d'arrondi comme avec double
    [DefaultValue(0)]
    public decimal Prix { get; set; }

    [DefaultValue(false)]
    public bool Vendu { get; set; }

    public Caracteristiques Caracteristiques { get; set; }

    public List<Reparation> Reparations { get; set; } = new();
}
