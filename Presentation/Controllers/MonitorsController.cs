using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Monitor = ForQab.DataAccess.Models.Monitor;
using ForQab.Service;
using ClosedXML.Excel;
using System.Data;
using ForQab.Presentation.Validators;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class MonitorsController : BaseController
    {
        private readonly IMonitorService _monitorService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MonitorsController(MyDbContext context ,UserManager<ApplicationUser> userManager, IMonitorService monitorService) : base(context, userManager)
        {
            _userManager = userManager;
            _monitorService = monitorService;
        }

        // GET: Monitors
        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var currentUserSection = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var model = await _monitorService.GetAllAsync(currentUserSection,searchName,genderId,finCode,serial,district, startYear, endYear);
            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);
        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var model = await _monitorService.GetAllArchivedAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);
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

        // GET: Monitors/Create
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _monitorService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name");
            return View();
        }

        // POST: Monitors/Create
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
                await _monitorService.AddAsync(monitor);
                TempData["SuccessMessage"] = "İmtahan rəhbəri uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }

            // Model geçersizse, sayfayı tekrar doldur ve hata mesajlarını göster.
            await LoadViewData(monitor);
            return View(monitor);
        }

        // GET: Monitors/Edit/5
        public async Task<IActionResult> Edit(int id)
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
            await LoadViewData(monitor);
            return View(monitor);
        }

        // POST: Monitors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Monitor monitor)
        {
            if (id != monitor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                   await _monitorService.UpdateAsync(monitor);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonitorExists(monitor.Id))
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
            await LoadViewData(monitor);
            return View(monitor);
        }

        // GET: Monitors/Delete/5
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

        // POST: Monitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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

            // Monitor'ü arşive al
            monitor.Archive = 1;
            monitor.ArchiveReason = archiveReason;
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

            // Monitor'ü arşive al
            monitor.Archive = 0;
            monitor.ArchiveReason = null;
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
            await _monitorService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MonitorLogs()
        {
            var logs = await _monitorService.GetMonitorLogsAsync();

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
        public async Task<IActionResult> ExportToExcel()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var fileContent = await _monitorService.ExportToExcelAsync(sectionId);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "İmtahan rəhbərləri.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _monitorService.ImportFromExcelAsync(excelFile);
            TempData["SuccessMessage"] = message;
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
    }

}
