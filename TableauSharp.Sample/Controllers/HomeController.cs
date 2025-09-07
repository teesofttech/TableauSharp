using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TableauSharp.Auth.Service;
using TableauSharp.Sample.Models;

namespace TableauSharp.Sample.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAuthService _authService;
    public HomeController(ILogger<HomeController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var token = await _authService.SignInWithJWTAsync("tableau_admin", CancellationToken.None);
        return View(token);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
