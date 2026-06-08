using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Monitor = ForQab.DataAccess.Models.Monitor;
using ClosedXML.Excel;
using System.Data;
using ForQab.Presentation.Validators;
using ForQab.DataAccess.ViewModel.Monitor;
using ForQab.Service.Abstract;
using ForQab.Data_Access.ViewModel;
using ForQab.Service;
using ForQab.Migrations;
using System.Threading;
using ForQab.Service.Concrete;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class MonitorsController : BaseController
    {
        private readonly IMonitorService _monitorService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAmasPhotoService _amasPhotoService;
        private readonly MonitorValidator _monitorValidator;
        private readonly MonitorEditValidator _monitorEditValidator;

        public MonitorsController(MyDbContext context, UserManager<ApplicationUser> userManager, IMonitorService monitorService, IAmasPhotoService amasPhotoService, MonitorEditValidator monitorEditValidator, MonitorValidator monitorValidator) : base(context, userManager)
        {
            _userManager = userManager;
            _monitorService = monitorService;
            _amasPhotoService = amasPhotoService;
            _monitorEditValidator = monitorEditValidator;
            _monitorValidator = monitorValidator;
        }

        public async Task<IActionResult> Index(
    string searchName,
    int? genderId,
    string? finCode,
    string serial,
    int? district,
    int? startYear,
    int? endYear,
    DateTime? createdStartDate,    
    DateTime? createdEndDate,
    int page = 1,
    int pageSize = 25)
        {
            var currentUserSection = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();

            var all = await _monitorService.GetAllAsync(
                currentUserSection, searchName, genderId, finCode, serial, district, startYear, endYear, createdStartDate, createdEndDate);

            
            var (paged, totalCount) = await _monitorService.GetPagedAsync(
    currentUserSection, searchName, genderId, finCode, serial, district,
    startYear, endYear, createdStartDate, createdEndDate, page, pageSize);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));


            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(paged);
        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear,
    DateTime? createdStartDate,
    DateTime? createdEndDate)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var model = await _monitorService.GetAllArchivedAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, createdStartDate, createdEndDate);
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();

            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }
            return View(monitor);
        }

        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _monitorService.GetSectionsAsync(sectionId);
            var subProfessions = await _monitorService.GetSubProfessionsAsync(sectionId);

            var viewModel = new MonitorViewModel
            {
                SubProfessions = subProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };
            ViewBag.Section = sectionId;
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonitorViewModel monitor)
        {
            // FluentValidation async olduğu üçün ValidateAsync çağırırıq
            var result = await _monitorValidator.ValidateAsync(monitor);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                // Konkret xəta mesajı ilə user-i məlumatlandırırıq ki, sadəcə yenilənmə təəssüratı yaranmasın
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";

                await LoadViewData(monitor);
                return View(monitor);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await LoadViewData(monitor);
                return View(monitor);
            }

            try
            {
                await _monitorService.AddAsync(monitor);
                TempData["SuccessMessage"] = "Nəzarətçi uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Database-tərəfli gözlənilməz xətaları (məs. concurrent insert ilə yaranan FinCode duplicate) tutaq
                ModelState.AddModelError(string.Empty, $"Yaddaşa yazma zamanı xəta baş verdi: {ex.Message}");
                TempData["ErrorMessage"] = "Yaddaşa yazma zamanı xəta baş verdi.";
                await LoadViewData(monitor);
                return View(monitor);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var sectionId = await GetCurrentSectionIdAsync();
                var monitor = await _monitorService.GetByIdAsync(id);
                await LoadViewData(monitor);
                var viewModel = await _monitorService.GetMonitorForEditAsync(id);
                ViewBag.Section = sectionId;
                return View(viewModel);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonitorEditViewModel monitor)
        {
            if (id != monitor.Id)
            {
                return NotFound();
            }

            var result = await _monitorEditValidator.ValidateAsync(monitor);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                return View(monitor);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                return View(monitor);
            }

            try
            {
                await _monitorService.UpdateAsync(monitor);
                TempData["SuccessMessage"] = "Nəzarətçi məlumatları yeniləndi.";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                return View(monitor);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yeniləmə zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yeniləmə zamanı xəta baş verdi.";
                return View(monitor);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }

            return View(monitor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor != null)
            {
                await _monitorService.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMonitor(int id, string archiveReason)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Archive = 1;
            monitor.ArchiveReason = archiveReason;
            monitor.Photo = null;
            await _monitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "Nəzarətçi arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMonitor(int id)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Archive = 0;
            monitor.ArchiveReason = null;
            monitor.Photo = null;
            await _monitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "Nəzarətçi arxivdən çıxarıldı.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusReason)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 1;
            monitor.StatusReason = statusReason;
            monitor.Photo = null;
            await _monitorService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStatus(int id)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 0;
            monitor.StatusReason = null;
            monitor.Photo = null;
            await _monitorService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MonitorLogs()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var logs = await _monitorService.GetMonitorLogsAsync(sectionId);

            return View(logs);
        }
        public async Task<IActionResult> MonitorLog(int monitorId)
        {
            var logs = await _monitorService.GetMonitorLogsBySupervisorIdAsync(monitorId);

            var monitor = await _context.Monitors.FindAsync(monitorId);
            if (monitor == null)
            {
                return NotFound();
            }

            return View(logs);
        }
        private bool MonitorExists(int id)
        {
            return _context.Monitors.Any(e => e.Id == id);
        }
        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }
        public async Task<IActionResult> ExportToExcel(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, DateTime? createdStartDate, DateTime? createdEndDate)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var fileContent = await _monitorService.ExportToExcelAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, createdStartDate, createdEndDate);

            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Nəzarətçilər.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _monitorService.ImportFromExcelAsync(excelFile);
            TempData["Message"] = message;
            return RedirectToAction(nameof(Index));
        }
        private async Task LoadViewData(Monitor monitor)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _monitorService.GetSectionsAsync(sectionId);

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
        }
        private async Task LoadViewData(MonitorViewModel monitor)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _monitorService.GetSectionsAsync(sectionId);

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLogs(int id)
        {
            var log = await _context.MonitorLogs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }
            await _monitorService.DeleteMonitorLogs(id);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ExportContracts(List<int> selectedMonitorIds, DateTime contractDate, string searchName, int? districtId)
        {
            if (selectedMonitorIds == null || !selectedMonitorIds.Any())
                return RedirectToAction(nameof(Index));

            // Seçilmişləri filtrele
            var filteredIds = await _monitorService
                .FilterSelectedMonitorsAsync(selectedMonitorIds, searchName, districtId);

            var bytes = await _monitorService
                .ExportContractsToWordAsync(filteredIds, contractDate);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "Mugavileler.docx");
        }
        [HttpPost]
        public async Task<IActionResult> FilterMonitorsAjax(string searchName, int? districtId)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var monitors = _context.Monitors.Where(m => m.SectionId == sectionId && m.Role == 2).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchName))
            {
                var keyword = searchName.Trim().ToLower();
                monitors = monitors.Where(m =>
                    (m.Name + " " + m.Surname).ToLower().Contains(keyword) ||
                    (m.Surname + " " + m.Name).ToLower().Contains(keyword));
            }

            if (districtId.HasValue)
                monitors = monitors.Where(m => m.District == districtId);

            var list = await monitors
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.Surname,
                    m.FinCode
                }).ToListAsync();

            return PartialView("_MonitorCheckboxListPartial", list);
        }
        [HttpPost]
        public async Task<IActionResult> ExportContract(int monitorId)
        {
            if (monitorId <= 0)
                return RedirectToAction(nameof(Index));

            try
            {
                var bytes = await _monitorService.ExportContractToWordAsync(monitorId);
                return File(
                    bytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    $"Mugavile_{monitorId}.docx");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id = monitorId });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchPhoto(int id)
        {
            var monitor = await _monitorService.GetByIdAsync(id);
            if (monitor == null) return NotFound();

            if (string.IsNullOrWhiteSpace(monitor.FinCode) ||
                string.IsNullOrWhiteSpace(monitor.Serial))
            {
                TempData["ErrorMessage"] = "FİN Kod və ya Seriya nömrəsi boşdur. Şəkil çəkilə bilmədi.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var photo = await _amasPhotoService.FetchPhotoAsBase64Async(
                monitor.FinCode, monitor.SerialPrefix ?? "", monitor.Serial);

            if (photo == null)
            {
                TempData["ErrorMessage"] = "AMAS sistemindən şəkil tapılmadı.";
                return RedirectToAction(nameof(Details), new { id });
            }

            monitor.Photo = photo;
            await _monitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "Şəkil AMAS-dan uğurla yükləndi.";
            return RedirectToAction(nameof(Details), new { id });
        }


    }

}
