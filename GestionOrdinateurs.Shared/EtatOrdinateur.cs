namespace GestionOrdinateurs.Shared;

//Stockée en base sous forme d'entier : ne pas changer l'ordre des valeurs,
//sinon les états déjà enregistrés changeraient de sens
public enum EtatOrdinateur
{
    EnService,
    EnPanne,
    EnMaintenance,
    HorsService,
    Lent,
    Reparable
}
