using LinkUpPro.Application.ViewModels.CreditsViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkUpPro.WebApp.Controllers;

[AllowAnonymous]
public class CreditsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new CreditsViewModel());
    }
}
