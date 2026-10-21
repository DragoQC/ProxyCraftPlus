using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyCraftPlus.Api.Documentation;
using ProxyCraftPlus.Api.Contracts;
using ProxyCraftPlus.Services;

namespace ProxyCraftPlus.Controllers;

public sealed class HomeController(
    ApiReferenceService reference,
    SettingsStore settings,
    PublicDashboardService dashboard) : Controller
{
    [AllowAnonymous, HttpGet("/")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!settings.Current.AuthenticationEnabled || User.Identity?.IsAuthenticated == true)
        {
            return View();
        }

        PublicDashboardResponse? response = await dashboard.GetAsync(cancellationToken);
        return response is null ? Challenge() : View("Welcome", response);
    }

    [HttpGet("/dashboard")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        ViewData["Page"] = "dashboard";
        return View("Welcome", await dashboard.GetAsync(cancellationToken, preview: true));
    }

    [HttpGet("/proxies")]
    public IActionResult Proxies() => View();

    [HttpGet("/proxies/{id:int}/rcon")]
    public IActionResult Rcon(int id)
    {
        ViewData["ProxyId"] = id;
        return View();
    }

    [HttpGet("/settings/proxy")]
    public IActionResult ProxySettings() => View();

    [HttpGet("/settings/app")]
    public IActionResult AppSettings() => View();

    [HttpGet("/settings/api")]
    public IActionResult Api() => View(reference.List());

    [AllowAnonymous, Route("/error"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error(int? statusCode)
    {
        Response.StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
        ViewData["StatusCode"] = Response.StatusCode;
        return View();
    }
}
