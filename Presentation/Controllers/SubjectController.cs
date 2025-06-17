using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;


namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class SubjectController : BaseController
    {
        private readonly ISubjectService _subjectService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;

        public SubjectController(ISubjectService subjectService, MyDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _userManager = userManager;
            _subjectService = subjectService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var subjects = await _subjectService.GetAllSubjectsAsync(sectionId);
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(subjects);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(); 
        }
        [HttpPost]
        public async Task<IActionResult> Create(Subject subject)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (ModelState.IsValid)
            {
                await _subjectService.AddSubjectAsync(subject);
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, re-populate dropdown lists
            //var sections = await _subjectService.GetSectionByIdAsync(sectionId);

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            

            return View(subject);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _subjectService.GetSubjectByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            if (subject == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Subject>(id))
            {
                return Forbid();
            }

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(subject);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Subject subject)
        {
            if (id != subject.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _subjectService.UpdateSubjectAsync(subject);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.Id))
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
            var sectionId= await GetCurrentSectionIdAsync();
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(subject);
        }



        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var subject = await _subjectService.GetSubjectByIdAsync(id);
            if (subject == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Subject>(id))
            {
                return Forbid();
            }
            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var subject = await _subjectService.GetSubjectByIdAsync(id);
            if (subject != null)
            {
                await _subjectService.DeleteSubjectAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectExists(int id)
        {
            if (_subjectService.GetSubjectByIdAsync(id) == null)
            {
                return false;
            }
            return true;
        }
        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }
    }
}
