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
            var monitors = await _headMonitorService.GetAllAsync(sectionId); // await ekleniyor

            // DataTable oluştur
            var dt = new DataTable("Monitors");
            dt.Columns.AddRange(new DataColumn[]
            {
        new DataColumn("Ad"),
        new DataColumn("Soyad"),
        new DataColumn("Ata adı"),
        new DataColumn("Arxiv"),
        new DataColumn("Cins"),
        new DataColumn("Rolu"),
        new DataColumn("Vəzifə nömrəsi"),
        new DataColumn("Peşə"),
        new DataColumn("İş yeri"),
        new DataColumn("Mövqe"),
        new DataColumn("Yaş"),
        new DataColumn("Ev telefonu"),
        new DataColumn("İş telefonu"),
        new DataColumn("FİN kod"),
        new DataColumn("Seriya"),
        new DataColumn("Bölmə"),
        new DataColumn("Rayon"),
            });

            // Verileri doldur
            foreach (var monitor in monitors)
            {
                dt.Rows.Add(
                    monitor.Name,
                    monitor.Surname,
                    monitor.Fname,
                    monitor.Archive,
                    monitor.GenderNavigation?.Name,
                    monitor.RoleNavigation?.Name,
                    monitor.VNum,
                    monitor.Profession,
                    monitor.Workplace,
                    monitor.Position,
                    monitor.Age,
                    monitor.TelEv,
                    monitor.TelIs,
                    monitor.FinCode,
                    monitor.Serial,
                    monitor.Section?.Name,
                    monitor.DistrictNavigation?.Name
                );
            }

            // Excel oluştur ve dosyayı döndür
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "İmtahan rəhbərləri");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "İmtahan rəhbərləri.xlsx");
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

                    await _headMonitorService.BulkAddAsync(monitors);
                }
            }

            TempData["SuccessMessage"] = "HeadMonitor-lər uğurla idxal edildi.";
            return RedirectToAction(nameof(Index));
        }

    }
}
