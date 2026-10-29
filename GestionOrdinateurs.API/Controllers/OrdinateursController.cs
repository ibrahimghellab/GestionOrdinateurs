using GestionOrdinateurs.API.Events;
using GestionOrdinateurs.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionOrdinateurs.Shared;
namespace GestionOrdinateurs.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdinateursController : ControllerBase
{
    private IConfiguration _configuration;
    private ILogger<OrdinateursController> _logger;

    private  IOrdinateurService _service;

    public OrdinateursController(ILogger<OrdinateursController> logger,IOrdinateurService service,IConfiguration configuration)
    {
        _logger = logger;
        _service = service;  
        _configuration = configuration;

        _service.OrdinateurAjoute += AfficherMessageAjout;
        _service.OrdinateurSupprime += AfficherMessageSuppression;
        _service.OrdinateurMisAJour += AfficherMessageMiseAJour;
    }

    private void AfficherMessageAjout(object? sender, OrdinateurAjouteEventArgs e)
    {
        Console.WriteLine($"[Évènement] Ordinateur ajouté : {e.Ordinateur.Id} - {e.Ordinateur.Nom} ({e.Ordinateur.Caracteristiques.Marque})");
    }

    private void AfficherMessageSuppression(object? sender, OrdinateurSupprimeEventArgs e)
    {
        Console.WriteLine($"[Évènement] Ordinateur supprimé : {e.Id}");
    }

    private void AfficherMessageMiseAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        Console.WriteLine($"[Évènement] Ordinateur mis à jour : {e.Ordinateur.Id} - {e.Ordinateur.Nom} ({e.Ordinateur.Caracteristiques.Marque})");
    }

    //Vérifie que le stock et le prix ne sont pas négatifs, renvoie le message d'erreur ou null si tout est valide
    private static string? VerifierStockEtPrix(Ordinateur ordinateur)
    {
        if (ordinateur.Stock < 0)
            return "Le stock ne peut pas être négatif.";

        if (ordinateur.Prix < 0)
            return "Le prix ne peut pas être négatif.";

        return null;
    }

   [HttpGet]
public async Task<IActionResult> GetAll()
{
    _logger.LogInformation("Récupération de tous les ordinateurs");
    return Ok(await _service.GetAllAsync());
}

[HttpGet("{id}")]
public async Task<IActionResult> GetComputerById(int id)
{
    var ordinateur = await _service.GetByIdAsync(id);
    if (ordinateur is null)
    {
        _logger.LogWarning("Ordinateur {Id} introuvable", id);
        return NotFound();
    }

    _logger.LogInformation("Récupération de l'ordinateur {Id}", id);
    return Ok(ordinateur);
}

[HttpGet("{id}/diagnostic")]
public async Task<IActionResult> GetComputerDiagnostic(int id)
{
    var ordinateur = await _service.GetByIdAsync(id);
    if (ordinateur is null)
    {
        _logger.LogWarning("Diagnostic impossible : ordinateur {Id} introuvable", id);
        return NotFound();
    }

    var diagnostic = _service.Diagnostiquer(ordinateur);
    _logger.LogInformation("Diagnostic de l'ordinateur {Id} : {Diagnostic}", id, diagnostic);

    //ToString() : nom de l'état ("Lent") plutôt que son numéro (4)
    return Ok(new
    {
        OrdinateurId = ordinateur.Id,
        Nom = ordinateur.Nom,
        Etat = ordinateur.Etat.ToString(),
        RamGo = ordinateur.Caracteristiques.RamGo,
        Diagnostic = diagnostic
    });
}

[HttpPost]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CreateComputer([FromBody] Ordinateur ordinateur)
{
    if (ordinateur.Id <= 0)
    {
        _logger.LogWarning("Tentative de création avec un Id invalide : {Id}", ordinateur.Id);
        return BadRequest("L'Id doit être un entier strictement positif.");
    }

    var erreur = VerifierStockEtPrix(ordinateur);
    if (erreur is not null)
    {
        _logger.LogWarning("Tentative de création avec des valeurs invalides : {Erreur}", erreur);
        return BadRequest(erreur);
    }

    if (!await _service.AddAsync(ordinateur))
    {
        _logger.LogWarning("Échec de la création : Id {Id} déjà existant", ordinateur.Id);
        return Conflict($"Un ordinateur avec l'Id {ordinateur.Id} existe déjà.");
    }

    _logger.LogInformation("Ordinateur créé : {Id} - {Nom}", ordinateur.Id, ordinateur.Nom);
    return CreatedAtAction(nameof(GetComputerById), new { id = ordinateur.Id }, ordinateur);
}

[HttpPut("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> UpdateComputer(int id, [FromBody] Ordinateur ordinateur)
{
    var erreur = VerifierStockEtPrix(ordinateur);
    if (erreur is not null)
    {
        _logger.LogWarning("Tentative de mise à jour de l'ordinateur {Id} avec des valeurs invalides : {Erreur}", id, erreur);
        return BadRequest(erreur);
    }

    if (!await _service.UpdateAsync(id, ordinateur))
    {
        _logger.LogWarning("Échec de la mise à jour : ordinateur {Id} introuvable", id);
        return NotFound();
    }

    _logger.LogInformation("Ordinateur {Id} mis à jour", id);
    return Ok(await _service.GetByIdAsync(id));
}

[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteComputer(int id)
{
    if (!await _service.DeleteAsync(id))
    {
        _logger.LogWarning("Échec de la suppression : ordinateur {Id} introuvable", id);
        return NotFound();
    }

    _logger.LogInformation("Ordinateur {Id} supprimé", id);
    return NoContent();
}

[HttpGet("marque/{marque}")]
public async Task<IActionResult> GetComputersByMarque(string marque)
{
    var resultats = await _service.GetByMarqueAsync(marque);
    _logger.LogInformation("Recherche par marque {Marque} : {Count} résultat(s)", marque, resultats.Count);
    return Ok(resultats);
}

[HttpGet("disponibles")]
public async Task<IActionResult> GetComputersDisponibles()
{
    var resultats = await _service.GetDisponiblesAsync();
    _logger.LogInformation("Recherche des ordinateurs disponibles : {Count} résultat(s)", resultats.Count);
    return Ok(resultats);
}

[HttpGet("prix-decroissant")]
public async Task<IActionResult> GetComputersParPrixDecroissant()
{
    var resultats = await _service.GetParPrixDecroissantAsync();
    _logger.LogInformation("Récupération des ordinateurs triés par prix décroissant : {Count} résultat(s)", resultats.Count);
    return Ok(resultats);
}

[HttpGet("inventaire/{nom}")]
public async Task<IActionResult> GetComputerFromInventaire(string nom)
{
    var inventaire = new InventaireOrdinateurs();
    inventaire.Ordinateurs = await _service.GetAllAsync();

    //Indexeur this[string nom]
    var ordinateur = inventaire[nom];
    if (ordinateur is null)
    {
        _logger.LogWarning("Ordinateur {Nom} introuvable dans l'inventaire", nom);
        return NotFound();
    }

    _logger.LogInformation("Ordinateur {Nom} trouvé dans l'inventaire : Id {Id}", nom, ordinateur.Id);
    return Ok(ordinateur);
}

[HttpGet("statistiques")]
public IActionResult GetStatistiques()
{
    //Déconstruction du tuple nommé
    var (total, enStock, valeurStock, nbReparations)
        = _service.CalculerStatistiques();

    _logger.LogInformation("Consultation des statistiques : {Total} ordinateur(s), {NbReparations} réparation(s)", total, nbReparations);

    //Un tuple renvoyé tel quel donnerait {} en JSON (ses valeurs sont des champs, pas des propriétés)
    return Ok(new
    {
        TotalOrdinateurs = total,
        OrdinateursDisponibles = enStock,
        ValeurStock = valeurStock,
        NombreReparations = nbReparations
    });
}

[HttpGet("info")]
public IActionResult GetInfos()
{
        var nom = _configuration["Application:Name"];
        var version = _configuration["Application:Version"];

        _logger.LogInformation("Consultation des informations de l'application");

        return Ok(new
        {
            Nom = nom,
            Version = version
        });
}
}
    
