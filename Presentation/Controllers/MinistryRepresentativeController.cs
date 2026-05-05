using ClosedXML.Excel;
using ForQab.DataAccess.Models;
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

        public MinistryRepresentativeController(UserManager<ApplicationUser> userManager, IMinistryRepresentativeService representativeService, MyDbContext context) : base(context, userManager)
        {
            _userManager = userManager;
            _representativeService = representativeService;
            _context = context;
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
            return View(new DimRepresentative());
        }
        [HttpPost]
        public async Task<IActionResult> Create(DimRepresentative dimRepresentative)
        {
            if (ModelState.IsValid)
            {
                await _representativeService.AddRepresentativeAsync(dimRepresentative);
                return RedirectToAction(nameof(Index));
            }
            return View(dimRepresentative);
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

            if (ModelState.IsValid)
            {
                try
                {
                    await _representativeService.UpdateRepresentativeAsync(representative);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommissionExists(representative.Id))
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
            return View(representative);
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

        public async Task<IActionResult> ExportToExcel()
        {
            var representatives = await _representativeService.GetAllRepresentativesAsync();

            var dt = new DataTable("MinistryRepresentatives");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Ad"),
                new DataColumn("Soyad"),
                new DataColumn("Ata adı"),
                new DataColumn("Fin Kod"),
                new DataColumn("Telefon"),
                new DataColumn("Seriya prefiksi"),
                new DataColumn("Seriya nömrəsi"),
            });

            foreach (var rep in representatives)
            {
                dt.Rows.Add(
                    rep.Name,
                    rep.Surname,
                    rep.Fname,
                    rep.FinCode,
                    rep.Tel,
                    rep.SerialPrefix ?? "",
                    rep.Serial
                );
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Nazirlik Nümayəndələri");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Nazirlik nümayəndələri.xlsx"
                    );
                }
            }
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