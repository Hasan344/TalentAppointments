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
        private readonly IAmasPhotoService _amasPhotoService;
        private readonly HeadMonitorValidator _headMonitorValidator;
        private readonly HeadMonitorEditValidator _headMonitorEditValidator;

        public HeadMonitorsController(MyDbContext context, UserManager<ApplicationUser> userManager, IHeadMonitorService headMonitorService, IAmasPhotoService amasPhotoService, HeadMonitorEditValidator headMonitorEditValidator, HeadMonitorValidator headMonitorValidator) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
            _headMonitorService = headMonitorService;
            _amasPhotoService = amasPhotoService;
            _headMonitorEditValidator = headMonitorEditValidator;
            _headMonitorValidator = headMonitorValidator;
        }

        // GET: HeadMonitors
        public async Task<IActionResult> Index(
    string searchName,
    int? genderId,
    string? finCode,
    string serial,
    int? district,
    int? startYear,
    int? endYear,
    int page = 1,
    int pageSize = 25)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var all = await _headMonitorService.GetAllAsync(
                sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);

            var totalCount = all.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.Genders = _context.Genders.ToList();
            ViewBag.Districts = _context.Districts.ToList();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(paged);
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
            return View(new Monitor());
        }

        // POST: HeadMonitors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Monitor monitor)
        {
            // Yeni rekord olduğu üçün Role və default-lar burada təyin olunur
            monitor.Role = 1;     // HeadMonitor
            monitor.Status = 0;
            monitor.Archive = 0;

            // FluentValidation async (FinCode unique yoxlaması üçün)
            var result = await _headMonitorValidator.ValidateAsync(monitor);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
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
                await _headMonitorService.AddAsync(monitor);
                TempData["SuccessMessage"] = "İmtahan rəhbəri uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yaddaşa yazma zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yaddaşa yazma zamanı xəta baş verdi.";
                await LoadViewData(monitor);
                return View(monitor);
            }
        }
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
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

            // Section list-i hazırlayırıq
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);

            // Monitor entity-dən ViewModel-ə mapping
            var vm = new HeadMonitorEditViewModel
            {
                Id = monitor.Id,
                Name = monitor.Name,
                Surname = monitor.Surname,
                Fname = monitor.Fname,
                Region = monitor.Region,
                SectionId = monitor.SectionId ?? 0,
                Gender = monitor.Gender,
                BirthDate = monitor.BirthDate,
                ContractNo = monitor.ContractNo,
                ContractDate = monitor.ContractDate,
                Uni = monitor.Uni,
                Position = monitor.Position,
                Profession = monitor.Profession,
                SSN = monitor.SSN,
                FinCode = monitor.FinCode,
                SerialPrefix = monitor.SerialPrefix,
                Serial = monitor.Serial,
                Rekvizit = monitor.Rekvizit,
                Voen = monitor.Voen,
                TelIs = monitor.TelIs,
                BankFilial = monitor.BankFilial,
                BankFilialCode = monitor.BankFilialCode,
                District = (byte)(monitor.District ?? 0),

                // View bunları Model.Sections / Model.Districts kimi oxuyur
                Sections = sections
                    .Select(s => new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Name
                    }).ToList(),

                Districts = _context.Districts
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    }).ToList()
            };

            return View(vm);
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

            var result = await _headMonitorEditValidator.ValidateAsync(headMonitor);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadEditViewBags(headMonitor);
                return View(headMonitor);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadEditViewBags(headMonitor);
                return View(headMonitor);
            }

            try
            {
                await _headMonitorService.UpdateModelAsync(headMonitor);
                TempData["SuccessMessage"] = "İmtahan rəhbəri məlumatları yeniləndi.";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                await ReloadEditViewBags(headMonitor);
                return View(headMonitor);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yeniləmə zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yeniləmə zamanı xəta baş verdi.";
                await ReloadEditViewBags(headMonitor);
                return View(headMonitor);
            }
        }

        // ⭐ YENİ köməkçi: Edit səhifəsi xətalı qayıtdıqda dropdown-ları yenidən doldurur
        private async Task ReloadEditViewBags(HeadMonitorEditViewModel vm)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);

            vm.Sections = sections.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();

            vm.Districts = _context.Districts.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList();
        }

        // GET: HeadMonitors/Delete/5
        public async Task<IActionResult> Delete(int id)
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
            monitor.Photo = null;
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
            monitor.Photo = null;
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
        public async Task<IActionResult> ExportContracts(List<int> selectedExpertIds, DateTime contractDate)
        {
            if (selectedExpertIds == null || !selectedExpertIds.Any())
                return RedirectToAction(nameof(Index));

            var bytes = await _headMonitorService
                .ExportContractsToWordAsync(selectedExpertIds, contractDate);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "Mugavileler.docx");
        }
        [HttpPost]
        public async Task<IActionResult> ExportContract(int monitorId)
        {
            if (monitorId <= 0)
                return RedirectToAction(nameof(Index));

            try
            {
                var bytes = await _headMonitorService.ExportContractToWordAsync(monitorId);
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
        public async Task<IActionResult> BulkArchive(List<int> selectedIds, string archiveReason)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Heç bir rəhbər seçilməyib.";
                return RedirectToAction(nameof(Index));
            }

            await _headMonitorService.BulkArchiveAsync(selectedIds, archiveReason ?? "");
            TempData["SuccessMessage"] = $"{selectedIds.Count} rəhbər arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchPhoto(int id)
        {
            var monitor = await _headMonitorService.GetByIdAsync(id);
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
            await _headMonitorService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "Şəkil AMAS-dan uğurla yükləndi.";
            return RedirectToAction(nameof(Details), new { id });
        }

    }
}