using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRChatTest.Context;
using SignalRChatTest.Contracts;
using SignalRChatTest.Migrations;
using SignalRChatTest.Service;

namespace SignalRChatTest.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly JwtService _jwtService;
    // private readonly SignalRDbContext _dbContext;
    private readonly IAuthService _authService;
    

    public AuthController(ILogger<AuthController> logger, JwtService jwtService, IAuthService authService)
    {
        _logger = logger;
        _jwtService = jwtService;
        // _dbContext = dbContext;
        _authService = authService;
    }


    [HttpGet("Register")]
    public async Task<IActionResult> Register()
    {
        return View("Register");
    }
    
    [HttpPost("Register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        try
        {
            var success = await _authService.RegisterAsync(registerRequest);
            if (!success)
            {
                // return BadRequest("Username or email is incorrect.");
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Register", registerRequest);
            }
            
            if (!ModelState.IsValid)
            {
                return View("Register", registerRequest);
            }
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while registering");
            return BadRequest(500);
        }
    }

    [HttpGet("Login")]
    public async Task<IActionResult> Login()
    {
        return View("Login");
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        try
        {
            var user = await _authService.LoginAsync(loginRequest);
            if (user is null)
            {
                // return BadRequest("Email or password is incorrect.");
                ModelState.AddModelError("", "Invalid username or password.");
                return View("Login", loginRequest);
            }

            if (!ModelState.IsValid)
            {
                return View("Login", loginRequest);
            }
            
            var jwt = _jwtService.GenerateToken(user.Id, user.Username, loginRequest.Roles);
            
            Response.Cookies.Append("jwt", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                // SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            
            _logger.LogInformation($"User {user.Id} logged in. Token: {jwt}");
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while login");
            return BadRequest(500);
        }
    }
    
    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");

        return RedirectToAction("Index", "Home");
    }
}