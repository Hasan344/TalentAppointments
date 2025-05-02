using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Monitor = ForQab.DataAccess.Models.Monitor;
using System.Data;
using ClosedXML.Excel;
using System.Globalization;
using ForQab.Presentation.Validators;
using ForQab.DataAccess.ViewModel.HeadMonitor;
using ForQab.Service.Abstract;
using ForQab.Service;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class HeadMonitorsController : BaseController
    {
        private readonly MyDbContext _context;
        private readonly IHeadMonitorService _headMonitorService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HeadMonitorsController(MyDbContext context, UserManager<ApplicationUser> userManager, IHeadMonitorService headMonitorService) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
            _headMonitorService = headMonitorService;
        }

        // GET: HeadMonitors
        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var model = await _headMonitorService.GetAllAsync(sectionId,searchName, genderId, finCode, serial, district, startYear, endYear);

            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();

            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);

        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var model = await _headMonitorService.GetAllArchivedAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);

            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();

            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);
        }

        // GET: HeadMonitors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monitor = await _headMonitorService.GetByIdAsync(id);
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

        // GET: HeadMonitors/Create
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name");
            return View();
        }

        // POST: HeadMonitors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Monitor monitor)
        {
            var validator = new HeadMonitorValidator();
            var result = validator.Validate(monitor);

            if (!result.IsValid)
            {
                // FluentValidation hatalarını ModelState’e ekleyelim ki View içinde gösterilebilsin.
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                // Hatalarla birlikte tekrar View’a döneceğiz.
                await LoadViewData(monitor);
                return View(monitor);
            }

            if (ModelState.IsValid)
            {
                await _headMonitorService.AddAsync(monitor);
                TempData["SuccessMessage"] = "İmtahan rəhbəri uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }

            // Model geçersizse, sayfayı tekrar doldur ve hata mesajlarını göster.
            await LoadViewData(monitor);
            return View(monitor);
        }

        // ViewData için tekrar tekrar kod yazmamak adına ayrı bir metot oluşturalım.
        


        // GET: HeadMonitors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }
            await LoadViewData(monitor);
            return View(monitor);
        }

        // POST: HeadMonitors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HeadMonitorEditViewModel headMonitor)
        {
            if (id != headMonitor.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(headMonitor);
            }

            try
            {
                await _headMonitorService.UpdateModelAsync(headMonitor);
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(headMonitor);
            }
        }

        // GET: HeadMonitors/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            if (id == null)
            {
                return NotFound();
            }

            var monitor = await _headMonitorService.GetByIdAsync(id);
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

        // POST: HeadMonitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor != null)
            {
                await _headMonitorService.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> HeadMonitorLogs()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var logs = await _headMonitorService.GetMonitorLogsAsync(sectionId);

            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMonitor(int id, string archiveReason)
        {
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            // Monitor'ü arşive al
            monitor.Archive = 1;
            monitor.ArchiveReason = archiveReason;
            await _headMonitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "İmtahan rəhbəri arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMonitor(int id)
        {
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            // Monitor'ü arşive al
            monitor.Archive = 0;
            monitor.ArchiveReason = null;
            await _headMonitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "İmtahan rəhbəri arxivdən çıxarıldı.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusReason)
        {
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 1;
            monitor.StatusReason = statusReason;
            monitor.Photo = null;
            await _headMonitorService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStatus(int id)
        {
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 0;
            monitor.StatusReason = null;
            monitor.Photo = null;
            await _headMonitorService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> HeadMonitorLog(int monitorId)
        {
            var logs = await _headMonitorService.GetMonitorLogsBySupervisorIdAsync(monitorId);

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
        public async Task<IActionResult> ExportToExcel(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var fileContent = await _headMonitorService.ExportToExcelAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "İmtahan rəhbərləri.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _headMonitorService.ImportFromExcelAsync(excelFile);
            TempData["Message"] = message;
            return RedirectToAction(nameof(Index));
        }
        private async Task LoadViewData(Monitor monitor)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);

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
            await _headMonitorService.DeleteMonitorLogs(id);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ExportContracts(List<int> selectedMonitorIds, DateTime contractDate)
        {
            if (selectedMonitorIds == null || !selectedMonitorIds.Any())
                return RedirectToAction(nameof(Index));

            var bytes = await _headMonitorService
                .ExportContractsToWordAsync(selectedMonitorIds, contractDate);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "Mugavileler.docx");
        }
    }
}