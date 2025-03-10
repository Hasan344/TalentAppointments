using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using ForQab.DataAccess.Models;

namespace ForQab.Presentation.Controllers;


[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    //MyDbContext db = new MyDbContext();
    public HomeController(ILogger<HomeController> logger, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }
    public async Task<IActionResult> Index()
    {
        var section = await GetCurrentSectionIdAsync();
        ViewBag.Section = section;
        return View();
    }
    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    private async Task<int?> GetCurrentSectionIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.SectionId != null ? user.SectionId : null;
    }
}
