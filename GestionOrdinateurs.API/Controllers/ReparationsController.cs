using GestionOrdinateurs.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReparationsController : ControllerBase
{
    private readonly ILogger<ReparationsController> _logger;

    private readonly IReparationService _reparService;

    private readonly IOrdinateurService _ordinateurService;

    private readonly IStockService _stockService;

    public ReparationsController(ILogger<ReparationsController> logger, IReparationService reparService, IOrdinateurService ordinateurService, IStockService stockService)
    {
        _logger = logger;
        _reparService = reparService;
        _ordinateurService = ordinateurService;
        _stockService = stockService;
    }

    //Techniciens de l'atelier, attribués à tour de rôle
    private static readonly string[] Techniciens = { "Karim", "Julie", "Thomas" };

    //Lance une réparation en même temps sur chaque ordinateur en base
    [HttpPost("SimulerReparation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SimulerReparation()
    {
        var listOrdinateurs = await _ordinateurService.GetAllAsync();

        if (listOrdinateurs.Count == 0)
        {
            _logger.LogWarning("Simulation impossible : aucun ordinateur en base");
            return BadRequest("Il faut au moins un ordinateur en base pour lancer la simulation.");
        }

        _logger.LogInformation("Début de la simulation de {Count} réparations simultanées", listOrdinateurs.Count);

        //Une tâche par ordinateur, le modulo fait tourner les techniciens (Karim, Julie, Thomas, Karim...)
        var reparations = listOrdinateurs.Select((ordinateur, index) =>
            _reparService.SimulerReparationAsync(ordinateur, Techniciens[index % Techniciens.Length]));

        await Task.WhenAll(reparations);

        _logger.LogInformation("Fin de la simulation des réparations");
        return Ok($"{listOrdinateurs.Count} réparations simulées.");
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var reparations = await _reparService.GetAllAsync();
        _logger.LogInformation("Récupération de toutes les réparations : {Count} résultat(s)", reparations.Count);
        return Ok(reparations);
    }

    [HttpGet("historique")]
    public async Task<IActionResult> GetHistorique()
    {
        var historique = await _reparService.GetHistoriqueAsync();
        _logger.LogInformation("Récupération de l'historique des réparations : {Count} résultat(s)", historique.Count);
        return Ok(historique);
    }

    //Consomme 10 SSD en parallèle puis renvoie le stock restant (attendu : 0)
    [HttpGet("GetStockSSD")]
    public ActionResult<int> GetStockSSD()
    {
        Parallel.For(
            0,
            10,
            i => _stockService.ConsommerSSD());

        _logger.LogInformation("Stock SSD après 10 consommations en parallèle : {StockSSD}", _stockService.StockSSD);

        return (_stockService.StockSSD);
    }
}
