using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Monitor = ForQab.DataAccess.Models.Monitor;
using ForQab.Service;
using System.Data;
using ClosedXML.Excel;
using System.Globalization;

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
            if (ModelState.IsValid)
            {
                await _headMonitorService.AddAsync(monitor);
                return RedirectToAction(nameof(Index));
            }
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
        }

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
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
        }

        // POST: HeadMonitors/Edit/5
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
                    await _headMonitorService.UpdateAsync(monitor);
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
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _headMonitorService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
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
            var monitor = await _headMonitorService.GetByIdAsync(id);
            if (monitor != null)
            {
                await _headMonitorService.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
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
            var fileContent = await _headMonitorService.ExportToExcelAsync(sectionId);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "İmtahan rəhbərləri.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _headMonitorService.ImportFromExcelAsync(excelFile);
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}