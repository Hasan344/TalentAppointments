
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Worker;
using ForQab.Presentation.Validators;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class WorkerController : BaseController
    {
        private readonly IWorkerService _workerService;
        private readonly UserManager<ApplicationUser> _userManager;
        public WorkerController(MyDbContext context, UserManager<ApplicationUser> userManager, IWorkerService workerService) : base(context, userManager)
        {
            _userManager = userManager;
            _workerService = workerService;
        }

        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var currentUserSection = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var model = await _workerService.GetAllAsync(currentUserSection, searchName, genderId, finCode, serial, district, startYear, endYear);
            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            ViewBag.WorkerTypes = await _context.WorkerTypes
                                    .Where(w => _context.Monitors.Any(m => m.WorkerType == w.Id && m.Status == 0 && m.Archive == 0))
                                    .Select(w => new SelectListItem
                                    {
                                        Value = w.Id.ToString(),
                                        Text = w.Name
                                    })
                                    .ToListAsync();


            return View(model);
        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var model = await _workerService.GetAllArchivedAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();

            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var monitor = await _workerService.GetByIdAsync(id);
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
            var sections = await _workerService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            ViewBag.WorkerType = new SelectList(_context.WorkerTypes, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name");
            if (sectionId == null)
            {
                ViewBag.Building = new SelectList(_context.ExamBuildings, "Id", "Name");
            }
            else
                ViewBag.Building = new SelectList(_context.ExamBuildings.Where(eb => eb.SectionId == sectionId), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Monitor monitor)
        {
            var validator = new HeadMonitorValidator();
            var result = validator.Validate(monitor);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }

                await LoadViewData(monitor);
                return View(monitor);
            }

            if (ModelState.IsValid)
            {
                await _workerService.AddAsync(monitor);
                TempData["SuccessMessage"] = "İmtahan rəhbəri uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData(monitor);
            return View(monitor);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var worker = _context.Monitors
        .Where(w => w.Id == id)
        .Select(w => new WorkerEditViewModel
        {
            Id = w.Id,
            Name = w.Name,
            Surname = w.Surname,
            Fname = w.Fname,
            Region = w.Region,
            SectionId = w.SectionId,
            Sections = _context.Sections.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList(),
            WorkerType = w.WorkerType,
            WorkerTypes = _context.WorkerTypes.Select(wt => new SelectListItem
            {
                Value = wt.Id.ToString(),
                Text = wt.Name
            }).ToList(),
            ExamBuilding = w.ExamBuildingId,
            ExamBuildings = _context.ExamBuildings.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList(),
            Gender = w.Gender,
            BirthDate = w.BirthDate,
            SSN = w.SSN,
            Rekvizit = w.Rekvizit,
            Voen = w.Voen,
            BankFilial = w.BankFilial,
            BankFilialCode = w.BankFilialCode,
            District = w.District,
            Districts = _context.Districts.Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.Name
            }).ToList()
        })
        .FirstOrDefault();
            if (worker == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _workerService.GetSectionsAsync(sectionId);

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", worker.Gender);
            ViewBag.WorkerType = new SelectList(_context.WorkerTypes, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", worker.District);
            if (sectionId == null)
            {
                ViewBag.Building = new SelectList(_context.ExamBuildings, "Id", "Name");
            }
            else
                ViewBag.Building = new SelectList(_context.ExamBuildings.Where(eb => eb.SectionId == sectionId), "Id", "Name");
            return View(worker);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, WorkerEditViewModel worker)
        {
            if (id != worker.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(worker);
            }

            try
            {
                await _workerService.UpdateModelAsync(worker);
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(worker);
            }
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var monitor = await _workerService.GetByIdAsync(id);
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
            var monitor = await _workerService.GetByIdAsync(id);
            if (monitor != null)
            {
                await _workerService.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMonitor(int id, string archiveReason)
        {
            var monitor = await _workerService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            monitor.Archive = 1;
            monitor.ArchiveReason = archiveReason;
            await _workerService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "İşçi arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMonitor(int id)
        {
            var monitor = await _workerService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Archive = 0;
            monitor.ArchiveReason = null;
            await _workerService.UpdateAsync(monitor);

            TempData["SuccessMessage"] = "İşçi arxivdən çıxarıldı.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusReason)
        {
            var monitor = await _workerService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 1;
            monitor.StatusReason = statusReason;
            monitor.Photo = null;
            await _workerService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStatus(int id)
        {
            var monitor = await _workerService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 0;
            monitor.StatusReason = null;
            monitor.Photo = null;
            await _workerService.UpdateAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MonitorLogs()
        {
            var logs = await _workerService.GetMonitorLogsAsync();

            return View(logs);
        }
        public async Task<IActionResult> MonitorLog(int monitorId)
        {
            var logs = await _workerService.GetMonitorLogsBySupervisorIdAsync(monitorId);

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
            var fileContent = await _workerService.ExportToExcelAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Digər işçilər.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _workerService.ImportFromExcelAsync(excelFile);
            TempData["Message"] = message;
            return RedirectToAction(nameof(Index));
        }
        private async Task LoadViewData(Monitor monitor)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _workerService.GetSectionsAsync(sectionId);

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewBag.WorkerType = new SelectList(_context.WorkerTypes, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            if (sectionId == null)
            {
                ViewBag.Building = new SelectList(_context.ExamBuildings, "Id", "Name");
            }
            else
                ViewBag.Building = new SelectList(_context.ExamBuildings.Where(eb => eb.SectionId == sectionId), "Id", "Name");
        }

        [HttpPost]
        public async Task<IActionResult> ExportContracts(List<int> selectedMonitorIds, DateTime contractDate, int workerType)
        {
            if (selectedMonitorIds == null || !selectedMonitorIds.Any())
            {
                TempData["ErrorMessage"] = "Seçim edilməmişdir.";
                return RedirectToAction(nameof(Index));
            }

            var fileContent = await _workerService.ExportContractsToWordAsync(selectedMonitorIds, contractDate, workerType);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "İşçi müqavilələri.docx");
        }

    }
}