using GestionOrdinateurs.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    private readonly IUserService _userService;

    public UsersController(ILogger<UsersController> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllAsync();
        _logger.LogInformation("Récupération de tous les utilisateurs : {Count} résultat(s)", users.Count);
        return Ok(users);
    }

    [HttpGet("login/{login}")]
    public async Task<IActionResult> GetUserByLogin(string login)
    {
        var user = await _userService.GetByLoginAsync(login);
        if (user is null)
        {
            _logger.LogWarning("Utilisateur {Login} introuvable", login);
            return NotFound();
        }

        _logger.LogInformation("Récupération de l'utilisateur {Login}", login);
        return Ok(user);
    }
}
