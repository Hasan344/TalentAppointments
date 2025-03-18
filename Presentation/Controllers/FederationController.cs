using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class FederationController : BaseController
    {
        private readonly IFederationService _federationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;

        public FederationController(
            IFederationService federationService,
            MyDbContext context,
            UserManager<ApplicationUser> userManager
        ) : base(context, userManager)
        {
            _federationService = federationService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var federations = await _federationService.GetAllFederationsAsync(sectionId);
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(federations);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            ViewBag.ProfessionList = new SelectList(await _context.Professions.Where(p => p.SectionId == sectionId).ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Profession federation)
        {
            var sectionId = await GetCurrentSectionIdAsync();

            if (ModelState.IsValid)
            {
                await _federationService.AddFederationAsync(federation);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            ViewBag.ProfessionList = new SelectList(await _context.Professions.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(federation);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var federation = await _federationService.GetFederationByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();

            if (federation == null)
                return NotFound();

            if (!await IsSectionValidAsync<Profession>(id))
                return Forbid();

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            ViewBag.ProfessionList = new SelectList(await _context.Professions.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(federation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Profession federation)
        {
            if (id != federation.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _federationService.UpdateFederationAsync(federation);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FederationExists(federation.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var sectionId = await GetCurrentSectionIdAsync();

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            ViewBag.ProfessionList = new SelectList(await _context.Professions.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(federation);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var federation = await _federationService.GetFederationByIdAsync(id);

            if (federation == null)
                return NotFound();

            if (!await IsSectionValidAsync<Profession>(id))
                return Forbid();

            return View(federation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var federation = await _federationService.GetFederationByIdAsync(id);

            if (federation != null)
                await _federationService.DeleteFederationAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private bool FederationExists(int id)
        {
            return _federationService.GetFederationByIdAsync(id) != null;
        }

        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }
    }
}
