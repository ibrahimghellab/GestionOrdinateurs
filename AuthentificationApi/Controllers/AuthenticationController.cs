using AuthentificationApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    private readonly IUserApiService _userApiService;

    private readonly ITokenService _tokenService;

    public AuthenticationController(ILogger<AuthenticationController> logger, IUserApiService userApiService, ITokenService tokenService)
    {
        _logger = logger;
        _userApiService = userApiService;
        _tokenService = tokenService;
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
}
