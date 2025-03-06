using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Presentation.Controllers;

public class ExamBuildingController : BaseController
{
    private readonly IExamBuildingService _examBuildingService;
    private readonly MyDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamBuildingController(
        IExamBuildingService examBuildingService,
        MyDbContext context,
        UserManager<ApplicationUser> userManager
    ) : base(context, userManager) // Pass both parameters to BaseController
    {
        _examBuildingService = examBuildingService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var sectionId = await GetCurrentSectionIdAsync();
        var examBuildings = await _examBuildingService.GetAllExamBuildingsAsync(sectionId);
        ViewBag.SectionList = await GetSectionSelectListAsync(sectionId);
        return View(examBuildings);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.SectionList = await GetSectionSelectListAsync(await GetCurrentSectionIdAsync());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamBuilding examBuilding)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.SectionList = await GetSectionSelectListAsync(await GetCurrentSectionIdAsync());
            return View(examBuilding);
        }

        await _examBuildingService.AddExamBuildingAsync(examBuilding);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var examBuilding = await _examBuildingService.GetExamBuildingByIdAsync(id);
        if (examBuilding == null) return NotFound();

        ViewBag.SectionList = await GetSectionSelectListAsync(await GetCurrentSectionIdAsync());
        return View(examBuilding);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamBuilding examBuilding)
    {
        if (id != examBuilding.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.SectionList = await GetSectionSelectListAsync(await GetCurrentSectionIdAsync());
            return View(examBuilding);
        }

        try
        {
            await _examBuildingService.UpdateExamBuildingAsync(examBuilding);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ExamBuildingExists(examBuilding.Id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var examBuilding = await _examBuildingService.GetExamBuildingByIdAsync(id);
        return examBuilding == null ? NotFound() : View(examBuilding);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _examBuildingService.DeleteExamBuildingAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ExamBuildingExists(int id)
        => await _examBuildingService.GetExamBuildingByIdAsync(id) != null;

    private async Task<int?> GetCurrentSectionIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.SectionId;
    }

    private async Task<SelectList> GetSectionSelectListAsync(int? sectionId)
    {
        var sections = sectionId == null
            ? await _examBuildingService.GetAllSectionsAsync()
            : await _examBuildingService.GetSectionsByIdAsync(sectionId.Value);

        return new SelectList(sections, "Id", "Name");
    }
}
