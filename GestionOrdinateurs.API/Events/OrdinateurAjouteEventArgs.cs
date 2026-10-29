using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Events;

public class OrdinateurAjouteEventArgs : EventArgs
{
    public Ordinateur Ordinateur { get; set; }
}