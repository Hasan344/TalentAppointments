using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using System.Data;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Math;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ForQab.Service.Abstract;

namespace ForQab.Presentation.Controllers
{
    public class KonsController : BaseController
    {
        private readonly IKonsService _konsService;
        private readonly ISubProfessionService _subProfessionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public KonsController(IKonsService konsService, ISubProfessionService subProfessionService, MyDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _konsService = konsService;
            _subProfessionService = subProfessionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
        {
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var sectionId = await GetCurrentSectionIdAsync();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var experts = await _konsService.GetAllAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, subProfessionId);
            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(experts);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _konsService.GetSectionsAsync(sectionId);
            var subProfessions = await _konsService.GetSubProfessionsAsync(sectionId);

            var viewModel = new KonsViewModel
            {
                SubProfessions = subProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(KonsViewModel KonsViewModel)
        {
            if (ModelState.IsValid)
            {
                await _konsService.AddAsync(KonsViewModel);
                return RedirectToAction(nameof(Index));
            }

            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _konsService.GetSectionsAsync(sectionId);
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);


            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            KonsViewModel.SubProfessions = subProfessions.Select(sp => new SelectListItem
            {
                Text = sp.Name,
                Value = sp.Id.ToString()
            }).ToList();

            return View(KonsViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expert = await _konsService.GetByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            if (expert == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }
            var sections = await _konsService.GetSectionsAsync(sectionId);
            var allSubProfessions = await _konsService.GetSubProfessionsAsync(sectionId);

            var viewModel = new KonsEditViewModel
            {
                Id = expert.Id,
                Name = expert.Name,
                Surname = expert.Surname,
                Fname = expert.Fname,
                SectionId = expert.SectionId,
                BankFilial = expert.BankFilial,
                BankFilialCode = expert.BankFilialCode,
                BirthDate = expert.BirthDate,
                HesablashmaH = expert.HesablashmaH,
                Rekvizit = expert.Rekvizit,
                SSN = expert.SSN,
                Voen = expert.Voen,
                Kons = true,
                FinCode = expert.FinCode,
                Profession = expert.Profession,
                SelectedSubProfessions = expert.SubProfessions.Select(sp => sp.Id).ToArray(),
                SubProfessions = allSubProfessions
                    .Select(sp => new SelectListItem
                    {
                        Value = sp.Id.ToString(),
                        Text = sp.Name
                    })
                    .ToList()
            };

            ViewData["SectionId"] = new SelectList(sections, "Id", "Name", expert.SectionId);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KonsEditViewModel expert)
        {
            if (id != expert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _konsService.UpdateAsync(expert);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExpertExists(expert.Id))
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
            var sections = await _konsService.GetSectionsAsync(sectionId);
            ViewData["SectionId"] = new SelectList(sections, "Id", "Name");
            return View(expert);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var expert = await _konsService.GetByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }
            return View(expert);
        }
        public async Task<IActionResult> Delete(int id)
        {
            var expert = await _konsService.GetByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }
            return View(expert);
        }

        // POST: HeadMonitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expert = await _konsService.GetByIdAsync(id);
            if (expert != null)
            {
                await _konsService.GetByIdAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExpertExists(int id)
        {
            if (_konsService.GetByIdAsync(id) == null)
            {
                return false;
            }
            return true;
        }
        [HttpGet]
        public JsonResult GetSubProfessions(int sectionId)
        {
            var subProfessions = _konsService.GetSubProfessionsAsync(sectionId);

            return Json(subProfessions);
        }
        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var sectionId = await GetCurrentSectionIdAsync();

            var experts = await _konsService.GetAllAsync(sectionId);

            var dt = new DataTable("Experts");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Ad"),
                new DataColumn("Soyad"),
                new DataColumn("Ata adı"),
                new DataColumn("Bölmə"),
                new DataColumn("Fin kodu"),
                new DataColumn("Peşə"),
                new DataColumn("Doğum tarixi"),
                new DataColumn("Rekvizit"),
                new DataColumn("SSN"),
                new DataColumn("Bank filialı"),
                new DataColumn("Bank filial kodu"),
                new DataColumn("Hesablaşma hesabı"),
                new DataColumn("VÖEN"),
                new DataColumn("İxtisaslar"),
            });

            // Verileri doldur
            foreach (var expert in experts)
            {
                var subProfessions = expert.SubProfessions != null && expert.SubProfessions.Any()
                    ? string.Join(", ", expert.SubProfessions.Select(sp => sp.Name))
                    : "---";

                dt.Rows.Add(
                    expert.Name,
                    expert.Surname,
                    expert.Fname,
                    expert.Section?.Name ?? "---",
                    expert.FinCode,
                    expert.Profession,
                    expert.BirthDate,
                    expert.Rekvizit,
                    expert.SSN,
                    expert.BankFilial,
                    expert.BankFilialCode,
                    expert.HesablashmaH,
                    expert.Voen,
                    subProfessions
                );
            }  // Excel dosyasını oluştur
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Experts");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Experts.xlsx");
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
            var sectionId = await GetCurrentSectionIdAsync();
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
                    if (sectionId == null)
                    {
                        ModelState.AddModelError("", "Admin fayl yükləməsi edə bilməz.");
                        return RedirectToAction(nameof(Index));
                    }
                    var experts = new List<Expert>();
                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Başlığı atla
                    {
                        var expert = new Expert
                        {
                            Name = row.Cell(1).GetString(),
                            Surname = row.Cell(2).GetString(),
                            Fname = row.Cell(3).GetString(),
                            SectionId=sectionId,
                            FinCode = row.Cell(4).GetString(),
                            Profession=row.Cell(5).GetString(),
                            Kons=true

                        };
                        experts.Add(expert);
                    }

                    await _konsService.BulkAddAsync(experts);
                }
            }

            TempData["SuccessMessage"] = "Expert-lər uğurla idxal edildi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
