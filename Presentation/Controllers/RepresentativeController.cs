// =====================================================================
// RepresentativeController.cs faylı — TAM DƏYİŞİKLİK
// =====================================================================

using ForQab.DataAccess.Models;
using ForQab.Presentation.Validators;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class RepresentativeController : BaseController
    {
        private readonly IRepresentativeService _representativeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;
        private readonly DimRepresentativeValidator _validator;

        public RepresentativeController(
            UserManager<ApplicationUser> userManager,
            IRepresentativeService representativeService,
            MyDbContext context,
            DimRepresentativeValidator validator)
            : base(context, userManager)
        {
            _userManager = userManager;
            _representativeService = representativeService;
            _context = context;
            _validator = validator;
        }

        public async Task<IActionResult> Index()
        {
            var commissions = await _representativeService.GetAllRepresentativesAsync();
            return View(commissions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var representative = await _representativeService.GetRepresentativeByIdAsync(id);
            if (representative == null)
            {
                return NotFound();
            }
            return View(representative);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            // Type = 1 → DİM nümayəndəsi (View-da hidden field var, amma default veririk)
            return View(new DimRepresentative { Type = 1 });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DimRepresentative dimRepresentative)
        {
            // Type DİM nümayəndəsi üçün təmin edilir
            if (dimRepresentative.Type == 0) dimRepresentative.Type = 1;

            var result = await _validator.ValidateAsync(dimRepresentative);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(dimRepresentative);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(dimRepresentative);
            }

            try
            {
                await _representativeService.AddRepresentativeAsync(dimRepresentative);
                TempData["SuccessMessage"] = "DİM nümayəndəsi uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yaddaşa yazma zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yaddaşa yazma zamanı xəta baş verdi.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(dimRepresentative);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            var representative = await _representativeService.GetRepresentativeByIdAsync(id);
            if (representative == null)
            {
                return NotFound();
            }
            return View(representative);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DimRepresentative representative)
        {
            if (id != representative.Id)
            {
                return NotFound();
            }

            var result = await _validator.ValidateAsync(representative);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(representative);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(representative);
            }

            try
            {
                await _representativeService.UpdateRepresentativeAsync(representative);
                TempData["SuccessMessage"] = "Nümayəndə məlumatları yeniləndi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommissionExists(representative.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yeniləmə zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yeniləmə zamanı xəta baş verdi.";
                ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
                return View(representative);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var commission = await _representativeService.GetRepresentativeByIdAsync(id);
            if (commission == null)
            {
                return NotFound();
            }
            return View(commission);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var commission = await _representativeService.GetRepresentativeByIdAsync(id);
            if (commission != null)
            {
                await _representativeService.DeleteRepresentativeAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CommissionExists(int id)
        {
            if (_representativeService.GetRepresentativeByIdAsync(id) == null)
            {
                return false;
            }
            return true;
        }
    }
}
