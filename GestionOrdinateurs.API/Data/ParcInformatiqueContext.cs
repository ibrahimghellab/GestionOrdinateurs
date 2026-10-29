using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Data;
public class ParcInformatiqueContext : DbContext
{
    public DbSet<Ordinateur> Ordinateurs{ get; set; }

    public DbSet<Reparation> Reparations{ get; set; }

    public DbSet<User> Users{ get; set; }

    public ParcInformatiqueContext() { }

    //Permet de fournir une configuration externe (ex : base en mémoire pour les tests)
    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    //SQLite par défaut, sauf si une configuration a déjà été fournie
    if (!optionsBuilder.IsConfigured)
        optionsBuilder.UseSqlite("Data Source=parcinfo.db");
}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Caracteristiques n'a pas de table à part : ses colonnes sont dans la table Ordinateur
        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(o => o.Caracteristiques);

        //SQLite ne sait pas trier une colonne decimal : le prix est stocké comme un nombre réel
        modelBuilder.Entity<Ordinateur>()
            .Property(o => o.Prix)
            .HasConversion<double>();

        modelBuilder.Entity<Ordinateur>()
            .HasMany(o => o.Reparations)
            .WithOne(r => r.UnOrdinateur)
            .HasForeignKey(r => r.OrdinateurId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();
    }

}