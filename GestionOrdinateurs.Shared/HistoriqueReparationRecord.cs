namespace GestionOrdinateurs.Shared;

//Ligne de l'historique des réparations renvoyée par l'API (DTO sans référence circulaire vers l'ordinateur)
public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention
);
