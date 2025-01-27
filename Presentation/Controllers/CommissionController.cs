using ForQab.Data_Access.ViewModel.Expert;
using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ForQab.Migrations;

namespace ForQab.Presentation.Controllers
{
    public class CommissionController : BaseController
    {
        private readonly ICommissionService _commissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;

        public CommissionController(ICommissionService commissionService, MyDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _userManager = userManager;
            _commissionService = commissionService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var commissions = await _commissionService.GetAllCommissionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(commissions);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(); 
        }
        [HttpPost]
        public async Task<IActionResult> Create(Commission commission)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (ModelState.IsValid)
            {
                await _commissionService.AddCommissionAsync(commission);
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is invalid, re-populate dropdown lists
            //var sections = await _commissionService.GetSectionByIdAsync(sectionId);

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            

            return View(commission);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var commission = await _commissionService.GetCommissionByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            if (commission == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Commission>(id))
            {
                return Forbid();
            }

            ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
            return View(commission);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Commission commission)
        {
            if (id != commission.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _commissionService.UpdateCommissionAsync(commission);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommissionExists(commission.Id))
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
            return View(commission);
        }



        public async Task<IActionResult> Delete(int id)
        {
            var commission = await _commissionService.GetCommissionByIdAsync(id);
            if (commission == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Commission>(id))
            {
                return Forbid();
            }
            return View(commission);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var commission = await _commissionService.GetCommissionByIdAsync(id);
            if (commission != null)
            {
                await _commissionService.DeleteCommissionAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CommissionExists(int id)
        {
            if (_commissionService.GetCommissionByIdAsync(id) == null)
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
