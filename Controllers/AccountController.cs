using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyCraftPlus.Services;

namespace ProxyCraftPlus.Controllers;

[AllowAnonymous]
public sealed class AccountController(SettingsStore settings) : Controller
{
    [HttpGet("/account/login"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Login()
    {
        if (!settings.Current.AuthenticationEnabled || User.Identity?.IsAuthenticated == true)
        {
            return Redirect("/");
        }

        return View();
    }
}
