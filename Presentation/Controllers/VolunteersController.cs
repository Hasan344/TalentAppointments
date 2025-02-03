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

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class VolunteersController : BaseController
    {
        private readonly IVolunteerService _volunteerService;
        private readonly MyDbContext _context;
        private readonly AuthDbContext _authDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public VolunteersController(MyDbContext context, UserManager<ApplicationUser> userManager, IVolunteerService volunteerService) : base(context, userManager)
        {
            _context = context;
            _userManager = userManager;
            _volunteerService = volunteerService;
        }

        // GET: Volunteer
        public async Task<IActionResult> Index()
        {
            var currentUserSection = await GetCurrentSectionIdAsync();
            var model = await _volunteerService.GetAllAsync(currentUserSection);
            return View(model);
        }

        // GET: Volunteer/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var monitor = await _volunteerService.GetByIdAsync(id);
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

        // GET: Volunteer/Create
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _volunteerService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name");
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name");
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name");
            return View();
        }

        // POST: Volunteer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Surname,Fname,Region,SectionId,0,Gender,Role,VNum,Profession,Workplace,Position,Age,Foto,TelEv,TelIs,FinCode,Serial,District")] DataAccess.Models.Monitor monitor)
        {
            if (ModelState.IsValid)
            {
                await _volunteerService.AddAsync(monitor);
                return RedirectToAction(nameof(Index));
            }
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _volunteerService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
        }

        // GET: Volunteer/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var monitor = await _volunteerService.GetByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _volunteerService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
        }

        // POST: Volunteer/Edit/5
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
                    await _volunteerService.UpdateAsync(monitor);
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
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _volunteerService.GetSectionsAsync(sectionId);
            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewData["Gender"] = new SelectList(_context.Genders, "Id", "Name", monitor.Gender);
            ViewData["Role"] = new SelectList(_context.Roles, "Id", "Name", monitor.Role);
            ViewData["District"] = new SelectList(_context.Districts, "Id", "Name", monitor.District);
            return View(monitor);
        }

        // GET: Volunteer/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var volunteer = await _volunteerService.GetByIdAsync(id);
            if (volunteer == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Monitor>(id))
            {
                return Forbid();
            }

            return View(volunteer);
        }

        // POST: Volunteer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monitor = await _volunteerService.GetByIdAsync(id);
            if (monitor != null)
            {
                await _volunteerService.DeleteAsync(id);
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
            var fileContent = await _volunteerService.ExportToExcelAsync(sectionId);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "İmtahan rəhbərləri.xlsx");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            var message = await _volunteerService.ImportFromExcelAsync(excelFile);
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

    }
}
