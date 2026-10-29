using AuthentificationApi.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    private readonly IUserApiService _userApiService;

    private readonly ITokenService _tokenService;

    private readonly IKeycloakService _keycloakService;

    public AuthenticationController(ILogger<AuthenticationController> logger, IUserApiService userApiService, ITokenService tokenService, IKeycloakService keycloakService)
    {
        _logger = logger;
        _userApiService = userApiService;
        _tokenService = tokenService;
        _keycloakService = keycloakService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string login)
    {
        var user = await _userApiService.GetUserByLoginAsync(login);
        if (user is null)
        {
            _logger.LogWarning("Connexion refusée : utilisateur {Login} inexistant", login);
            return NotFound($"L'utilisateur {login} n'existe pas.");
        }

        _logger.LogInformation("Token généré pour {Login} ({Role})", user.Login, user.Role);
        return Ok(_tokenService.GenererToken(user));
    }

    [HttpPost("login-keycloak")]
    public async Task<IActionResult> LoginKeycloak(Identifiants identifiants)
    {
        var token = await _keycloakService.ConnecterAsync(identifiants);
        if (token is null)
        {
            _logger.LogWarning("Connexion Keycloak refusée pour {Login}", identifiants.Login);
            return Unauthorized("Utilisateur ou mot de passe incorrect.");
        }

        _logger.LogInformation("Token Keycloak obtenu pour {Login}", identifiants.Login);
        return Ok(token);
    }
}
