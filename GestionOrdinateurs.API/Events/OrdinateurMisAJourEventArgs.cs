using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Events;

public class OrdinateurMisAJourEventArgs : EventArgs
{
    public Ordinateur Ordinateur { get; set; }
}