using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ForQab.Presentation.Controllers;
[Authorize]
public class DistrictController : BaseController
{

    private readonly IDistrictService _districtService;
    private readonly MyDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DistrictController( MyDbContext context, UserManager<ApplicationUser> userManager, IDistrictService districtService) : base(context,userManager)
    {
        _districtService = districtService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var districts = await _districtService.GetAllDistrictsAsync();
        return View(districts);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(District district)
    {
        if (ModelState.IsValid)
        {
            await _districtService.AddDistrictAsync(district);
            return RedirectToAction(nameof(Index));
        }
        return View(district);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var district = await _districtService.GetDistrictByIdAsync(id);
        if (district == null)
        {
            return NotFound();
        }
        return View(district);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, District district)
    {
        if (id != district.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            await _districtService.UpdateDistrictAsync(district);
            return RedirectToAction(nameof(Index));
        }
        return View(district);
    }

    public async Task<IActionResult> Delete(int id)
    {
        if (!await IsAdminValidAsync())
        {
            return Forbid();
        }
        var district = await _districtService.GetDistrictByIdAsync(id);
        if (district == null)
        {
            return NotFound();
        }
        return View(district);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!await IsAdminValidAsync())
        {
            return Forbid();
        }
        await _districtService.DeleteDistrictAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> GetCurrentSectionIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.SectionId != null ? user.SectionId : null;
    }
}
