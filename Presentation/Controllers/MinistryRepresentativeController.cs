// =====================================================================
// MinistryRepresentativeController.cs faylı — TAM DƏYİŞİKLİK (FetchPhoto əlavə edildi)
// =====================================================================

using ClosedXML.Excel;
using ForQab.DataAccess.Models;
using ForQab.Presentation.Validators;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class MinistryRepresentativeController : BaseController
    {
        private readonly IMinistryRepresentativeService _representativeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MyDbContext _context;
        private readonly DimRepresentativeValidator _validator;
        private readonly IAmasPhotoService _amasPhotoService;

        public MinistryRepresentativeController(
            UserManager<ApplicationUser> userManager,
            IMinistryRepresentativeService representativeService,
            MyDbContext context,
            DimRepresentativeValidator validator,
            IAmasPhotoService amasPhotoService)
            : base(context, userManager)
        {
            _userManager = userManager;
            _representativeService = representativeService;
            _context = context;
            _validator = validator;
            _amasPhotoService = amasPhotoService;
        }

        public async Task<IActionResult> Index()
        {
            var commissions = await _representativeService.GetAllRepresentativesAsync();
            return View(commissions);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var representative = await _representativeService.GetRepresentativeByIdAsync(id);
            if (representative == null)
            {
                return NotFound();
            }
            return View(representative);
        }

        public ActionResult Create()
        {
            // Type = 2 → Nazirlik nümayəndəsi
            return View(new DimRepresentative { Type = 2 });
        }

        [HttpPost]
        public async Task<IActionResult> Create(DimRepresentative dimRepresentative)
        {
            if (dimRepresentative.Type == 0) dimRepresentative.Type = 2;

            var result = await _validator.ValidateAsync(dimRepresentative);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                return View(dimRepresentative);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                return View(dimRepresentative);
            }

            try
            {
                await _representativeService.AddRepresentativeAsync(dimRepresentative);
                TempData["SuccessMessage"] = "Nazirlik nümayəndəsi uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yaddaşa yazma zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yaddaşa yazma zamanı xəta baş verdi.";
                return View(dimRepresentative);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
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
                return View(representative);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
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

        // ─────────────────────────────────────────────────────────────────
        // AMAS-dan şəkil çəkmək üçün endpoint
        // ─────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchPhoto(int id)
        {
            var representative = await _representativeService.GetRepresentativeByIdAsync(id);
            if (representative == null) return NotFound();

            if (string.IsNullOrWhiteSpace(representative.FinCode) ||
                string.IsNullOrWhiteSpace(representative.Serial))
            {
                TempData["ErrorMessage"] = "FİN Kod və ya Seriya nömrəsi boşdur. Şəkil çəkilə bilmədi.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var photo = await _amasPhotoService.FetchPhotoAsBase64Async(
                representative.FinCode, representative.SerialPrefix ?? "", representative.Serial);

            if (photo == null)
            {
                TempData["ErrorMessage"] = "AMAS sistemindən şəkil tapılmadı.";
                return RedirectToAction(nameof(Details), new { id });
            }

            representative.Photo = photo;
            await _representativeService.UpdateRepresentativeAsync(representative);

            TempData["SuccessMessage"] = "Şəkil AMAS-dan uğurla yükləndi.";
            return RedirectToAction(nameof(Details), new { id });
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
