using ForQab.DataAccess.Models;
using ForQab.Service;
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

    public ExamBuildingController(IExamBuildingService examBuildingService, MyDbContext context, UserManager<ApplicationUser> userManager) : base(context,userManager)
    {
        _examBuildingService = examBuildingService;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var sectionId = await GetCurrentSectionIdAsync();
        var examBuildings = await _examBuildingService.GetAllExamBuildingsAsync(sectionId);
        ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
        return View(examBuildings);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var sectionId = await GetCurrentSectionIdAsync();
        if (sectionId==null)
        {
            ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
        }
        else
        ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ExamBuilding examBuilding)
    {
        var sectionId = await GetCurrentSectionIdAsync();
        if (ModelState.IsValid)
        {
            await _examBuildingService.AddExamBuildingAsync(examBuilding);
            return RedirectToAction(nameof(Index));
        }
        if (sectionId == null)
        {
            ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
        }
        else
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
        return View(examBuilding);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var examBuilding = await _examBuildingService.GetExamBuildingByIdAsync(id);
        var sectionId = await GetCurrentSectionIdAsync();
        if (examBuilding == null)
        {
            return NotFound();
        }
        if (sectionId == null)
        {
            ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
        }
        else
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
        return View(examBuilding);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamBuilding examBuilding)
    {
        if (id != examBuilding.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _examBuildingService.UpdateExamBuildingAsync(examBuilding);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExamBuildingExists(examBuilding.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        var sectionId = await GetCurrentSectionIdAsync(); if (sectionId == null)
        {
            ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
        }
        else
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
        return View(examBuilding);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var examBuilding = await _examBuildingService.GetExamBuildingByIdAsync(id);
        if (examBuilding == null)
        {
            return NotFound();
        }

        return View(examBuilding);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _examBuildingService.DeleteExamBuildingAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> ExamBuildingExists(int id)
    {
        return await _examBuildingService.GetExamBuildingByIdAsync(id) != null;
    }
    private async Task<int?> GetCurrentSectionIdAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        return user?.SectionId != null ? user.SectionId : null;
    }
}