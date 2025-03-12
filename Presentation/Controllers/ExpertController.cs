using ClosedXML.Excel;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

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

        public async Task<IActionResult> Index(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? federationId, int? subProfessionId)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = _context.Professions.ToList();
            var experts = await _expertService.GetExpertsBySectionIdAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, federationId, subProfessionId);
            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Federation = federations;
            ViewBag.Districts = districts;
            return View(experts);
        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = _context.Professions.ToList();
            var model = await _expertService.GetArchivedExpertsBySectionIdAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, subProfessionId);
            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Federation = federations;
            ViewBag.Districts = districts;

            ViewBag.Genders = genders;
            ViewBag.Districts = districts;
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.Section = sectionId;
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
            ViewBag.Section = sectionId;
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
            ViewBag.Section = sectionId;
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
                TelEl = expert.TelEl,
                TelIs = expert.TelIs,
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
            ViewBag.Section = sectionId;
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
        public async Task<IActionResult> ExpertLogs()
        {
            var logs = await _expertService.GetExpertLogsAsync();

            return View(logs);
        }
        public async Task<IActionResult> ExpertLog(int expertId)
        {
            var logs = await _expertService.GetExpertLogsByExpertIdAsync(expertId);

            var expert = await _context.Experts.FindAsync(expertId);
            if (expert == null)
            {
                return NotFound();
            }

            return View(logs);
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
            if (sectionId == null)
            {
                ModelState.AddModelError("", "Admin fayl yükləməsi edə bilməz.");
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

                    var experts = new List<Expert>();
                    var existingFinCodes = _context.Experts.Select(e => e.FinCode).ToHashSet();

                    foreach (var row in worksheet.RowsUsed().Skip(1)) // Başlığı atla
                    {
                        var finCode = row.Cell(4).GetString();
                        if (string.IsNullOrEmpty(finCode) || existingFinCodes.Contains(finCode))
                        {
                            continue; // Mövcud və ya boş FinCode-ları əlavə etmə
                        }

                        var expert = new Expert
                        {
                            Name = row.Cell(1).GetString(),
                            Surname = row.Cell(2).GetString(),
                            Fname = row.Cell(3).GetString(),
                            FinCode = finCode,
                            Profession = row.Cell(5).IsEmpty() ? null : row.Cell(5).GetString(),
                            Kons = false,
                            SectionId = sectionId,
                            Gender = _context.Genders.FirstOrDefault(g => g.Name == row.Cell(6).GetString())?.Id,
                            District = _context.Districts.FirstOrDefault(d => d.Name == row.Cell(7).GetString())?.Id,
                            Federation = _context.Professions.FirstOrDefault(p => p.Name == row.Cell(8).GetString())?.Id,
                            HesablashmaH = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                            Rekvizit = row.Cell(11).GetString(),
                            Serial = row.Cell(12).GetString(),
                            SSN = row.Cell(13).GetString(),
                            Voen = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            BirthDate = row.Cell(15).IsEmpty() ? null
                                : DateOnly.ParseExact(row.Cell(15).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            Status = 0,
                            BankFilial = row.Cell(16).GetString(),
                            BankFilialCode = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                            TelIs = row.Cell(18).IsEmpty() ? null : row.Cell(18).GetString(),
                        };

                        // SubProfessions Many-to-Many əlaqəsi üçün işlənir
                        var subProfessionNames = row.Cell(9).GetString().Split(',').Select(x => x.Trim()).ToList();
                        foreach (var subProfessionName in subProfessionNames)
                        {
                            var subProfession = _context.SubProfessions.FirstOrDefault(sp => sp.Name == subProfessionName);
                            if (subProfession != null)
                            {
                                expert.SubProfessions.Add(subProfession);
                            }
                        }

                        experts.Add(expert);
                    }

                    if (experts.Any())
                    {
                        await _expertService.BulkAddAsync(experts);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> GetSubProfessionsByFederation(int federationId)
        {
            var subProfessions = await _context.SubProfessions
                .Where(sp => sp.ProfessionId == federationId) 
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync();

            return Json(subProfessions);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLogs(int id)
        {
            var monitor = await _expertService.GetExpertByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }
            await _expertService.DeleteExpertLogs(id);
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveExpert(int id, string archiveReason)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }

            expert.Archive = 1;
            expert.ArchiveReason = archiveReason;
            await _expertService.UpdateExpertAsync(expert);

            TempData["SuccessMessage"] = "Nəzarətçi arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreExpert(int id)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }

            expert.Archive = 0;
            expert.ArchiveReason = null;
            await _expertService.UpdateExpertAsync(expert);

            TempData["SuccessMessage"] = "Nəzarətçi arxivdən çıxarıldı.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusReason)
        {
            var monitor = await _expertService.GetExpertByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 1;
            monitor.StatusReason = statusReason;
            await _expertService.UpdateExpertAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStatus(int id)
        {
            var monitor = await _expertService.GetExpertByIdAsync(id);
            if (monitor == null)
            {
                return NotFound();
            }

            monitor.Status = 0;
            monitor.StatusReason = null;
            await _expertService.UpdateExpertAsync(monitor);

            return RedirectToAction(nameof(Index));
        }

    }

}