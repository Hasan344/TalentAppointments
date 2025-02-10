using ClosedXML.Excel;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class ExpertController : BaseController
    {
        private readonly IExpertService _expertService;
        private readonly ISubProfessionService _subProfessionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExpertController(IExpertService expertService, ISubProfessionService subProfessionService, MyDbContext context, UserManager<ApplicationUser> userManager) : base(context, userManager)
        {
            _expertService = expertService;
            _subProfessionService = subProfessionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear,int? subProfessionId)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = _context.Professions.ToList();
            var experts = await _expertService.GetExpertsBySectionIdAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, subProfessionId);
            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Federation = federations;
            ViewBag.Districts = districts;
            return View(experts);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var subProfessions = await _expertService.GetSubProfessionsAsync(sectionId);
            var federations = await _expertService.GetFederationsAsync(sectionId);

            var viewModel = new ExpertViewModel
            {
                SubProfessions = subProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ExpertViewModel expertViewModel)
        {
            if (ModelState.IsValid)
            {
                await _expertService.AddExpertAsync(expertViewModel);
                return RedirectToAction(nameof(Index));
            }

            var sectionId = await GetCurrentSectionIdAsync();
            // If ModelState is invalid, re-populate dropdown lists
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = await _expertService.GetFederationsAsync(sectionId);


            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
            expertViewModel.SubProfessions = subProfessions.Select(sp => new SelectListItem
            {
                Text = sp.Name,
                Value = sp.Id.ToString()
            }).ToList();

            return View(expertViewModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            var federations = await _expertService.GetFederationsAsync(sectionId);
            if (expert == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var allSubProfessions = await _expertService.GetSubProfessionsAsync(sectionId);

            // Map to ViewModel
            var viewModel = new ExpertEditViewModel
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
                Kons = false,
                FinCode = expert.FinCode,
                Profession = expert.Profession,
                Gender = expert.Gender,
                Federation = expert.Federation,
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
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpertEditViewModel expert)
        {
            if (id != expert.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _expertService.UpdateExpertAsync(expert);
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
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var federations = await _expertService.GetFederationsAsync(sectionId);

            ViewData["SectionId"] = new SelectList(sections, "Id", "Name");
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
            return View(expert);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
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
            var expert = await _expertService.GetExpertByIdAsync(id);
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
            var expert = await _expertService.GetExpertByIdAsync(id);
            if (expert != null)
            {
                await _expertService.DeleteExpertAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExpertExists(int id)
        {
            if (_expertService.GetExpertByIdAsync(id) == null)
            {
                return false;
            }
            return true;
        }
        [HttpGet]
        public JsonResult GetSubProfessions(int sectionId)
        {
            var subProfessions = _expertService.GetSubProfessionsAsync(sectionId);

            return Json(subProfessions);
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
            var experts = await _expertService.GetExpertsBySectionIdAsync(sectionId);

            // DataTable oluştur
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
            }

            // Excel dosyasını oluştur
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
                            SectionId = sectionId,
                            FinCode = row.Cell(4).GetString(),
                            Profession = row.Cell(5).GetString(),
                            Kons = true

                        };
                        experts.Add(expert);
                    }

                    await _expertService.BulkAddAsync(experts);
                }
            }

            TempData["SuccessMessage"] = "Expert-lər uğurla idxal edildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSubProfessionsByFederation(int federationId)
        {
            var subProfessions = await _context.SubProfessions
                .Where(sp => sp.ProfessionId == federationId) // Federation aslında Profession olduğu için bu ilişkiyi kullanıyoruz.
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync();

            return Json(subProfessions);
        }

    }

}