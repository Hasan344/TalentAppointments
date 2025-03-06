using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Presentation.Controllers
{
    public class SubProfessionController : BaseController
    {
        private readonly ISubProfessionService _subProfessionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;

        public SubProfessionController(
            ISubProfessionService subProfessionService,
            MyDbContext context,
            UserManager<ApplicationUser> userManager
        ) : base(context, userManager)
        {
            _subProfessionService = subProfessionService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(subProfessions);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            }
            ViewBag.ProfessionList = new SelectList(await _context.Professions.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubProfession subProfession)
        {
            var sectionId = await GetCurrentSectionIdAsync();

            if (ModelState.IsValid)
            {
                await _subProfessionService.AddSubProfessionAsync(subProfession);
                return RedirectToAction(nameof(Index));
            }

            // Re-populate dropdown lists if ModelState is invalid if (sectionId == null)
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            }
            ViewBag.ProfessionList = new SelectList(await _context.Professions.ToListAsync(), "Id", "Name");
            return View(subProfession);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subProfession = await _subProfessionService.GetSubProfessionByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();

            if (subProfession == null)
                return NotFound();

            if (!await IsSectionValidAsync<SubProfession>(id))
                return Forbid();
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            }
            ViewBag.ProfessionList = new SelectList(await _context.Professions.ToListAsync(), "Id", "Name");
            return View(subProfession);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubProfession subProfession)
        {
            if (id != subProfession.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _subProfessionService.UpdateSubProfessionAsync(subProfession);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubProfessionExists(subProfession.Id))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            var sectionId = await GetCurrentSectionIdAsync(); 
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            }
            ViewBag.ProfessionList = new SelectList(await _context.Professions.ToListAsync(), "Id", "Name");
            return View(subProfession);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var subProfession = await _subProfessionService.GetSubProfessionByIdAsync(id);

            if (subProfession == null)
                return NotFound();

            if (!await IsSectionValidAsync<SubProfession>(id))
                return Forbid();

            return View(subProfession);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subProfession = await _subProfessionService.GetSubProfessionByIdAsync(id);

            if (subProfession != null)
                await _subProfessionService.DeleteSubProfessionAsync(id);

            return RedirectToAction(nameof(Index));
        }

        private bool SubProfessionExists(int id)
        {
            return _subProfessionService.GetSubProfessionByIdAsync(id) != null;
        }

        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }
    }
}
