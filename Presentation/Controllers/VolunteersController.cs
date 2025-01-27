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
            // Mevcut section ID'yi alın (Gerekirse kaldırın veya değiştirin)
            var sectionId = await GetCurrentSectionIdAsync();

            // Verileri alın
            var volunteers = await _volunteerService.GetAllAsync(sectionId);

            // DataTable oluştur
            var dt = new DataTable("Volunteers");
            dt.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Ad"),
        new DataColumn("Soyad"),
        new DataColumn("Ata adı"),
        new DataColumn("İş Telefonu"),
        new DataColumn("Cins"),
        new DataColumn("Rolu"),
        new DataColumn("Bölmə"),
            });

            // Verileri doldur
            foreach (var volunteer in volunteers)
            {
                dt.Rows.Add(
                    volunteer.Name,
                    volunteer.Surname,
                    volunteer.Fname,
                    volunteer.TelIs ?? "---",
                    volunteer.GenderNavigation?.Name ?? "---",
                    volunteer.RoleNavigation?.Name ?? "---",
                    volunteer.Section?.Name ?? "---"
                );
            }

            // Excel dosyasını oluştur
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Volunteers");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Volunteers.xlsx");
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ModelState.AddModelError("", "Excel faylı yüklənməmişdir.");
                return RedirectToAction(nameof(Index));
            }

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        ModelState.AddModelError("", "Excel faylı düzgün deyil.");
                        return RedirectToAction(nameof(Index));
                    }

                    var monitors = new List<Monitor>();
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Başlığı atla
                    {
                        var monitor = new Monitor
                        {
                            Name = row.Cell(1).GetString(),
                            Surname = row.Cell(2).GetString(),
                            Fname = row.Cell(3).GetString(),
                            Archive = Convert.ToByte(row.Cell(4).GetValue<int>()),
                            Gender = row.Cell(5).GetValue<byte>(),
                            Role = row.Cell(6).GetValue<byte>(),
                            VNum = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetValue<string?>(),
                            Profession = row.Cell(8).IsEmpty() ? null : row.Cell(8).GetString(),
                            Workplace = row.Cell(9).IsEmpty() ? null : row.Cell(9).GetString(),
                            Position = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                            Age = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetValue<int?>(),
                            TelEv = row.Cell(12).IsEmpty() ? null : row.Cell(12).GetString(),
                            TelIs = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetString(),
                            FinCode = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            Serial = row.Cell(15).IsEmpty() ? null : row.Cell(15).GetString(),
                            SectionId = row.Cell(16).GetValue<int>(),
                            District = row.Cell(17).GetValue<int>()
                        };
                        monitors.Add(monitor);
                    }

                    await _volunteerService.BulkAddAsync(monitors);
                }
            }

            TempData["SuccessMessage"] = "HeadMonitor-lər uğurla idxal edildi.";
            return RedirectToAction(nameof(Index));
        }

    }
}
