using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Expert;
using System.Data;
using ClosedXML.Excel;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using ForQab.Service;
using System.Globalization;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
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
                TelEl = expert.TelEl,
                TelIs = expert.TelIs,
                SelectedSubProfessions = expert.ExpertsProfessions.Select(sp => sp.SubProfessionId).ToArray(),
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
                new DataColumn("İstiqamət"),
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
                var subProfessions = expert.ExpertsProfessions != null && expert.ExpertsProfessions.Any()
                    ? string.Join(", ", expert.ExpertsProfessions.Select(sp => sp.SubProfession.Name))
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

                    foreach (var row in worksheet.RowsUsed().Skip(1)) 
                    {
                        var finCode = row.Cell(7).GetString();
                        if (string.IsNullOrEmpty(finCode) || existingFinCodes.Contains(finCode))
                        {
                            continue; 
                        }

                        var expert = new Expert
                        {
                            District = _context.Districts.FirstOrDefault(d => d.Name == row.Cell(1).GetString())?.Id,
                            Surname = row.Cell(2).GetString(),
                            Name = row.Cell(3).GetString(),
                            Fname = row.Cell(4).GetString(),
                            Gender = row.Cell(5).GetValue<byte>(),
                            Serial = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetString(),
                            FinCode = finCode,
                            Federation = _context.Professions.FirstOrDefault(p => p.Name == row.Cell(8).GetString())?.Id,
                            Profession = row.Cell(10).IsEmpty() ? null : row.Cell(10).GetString(),
                            BirthDate = row.Cell(11).IsEmpty() ? null
                                    : DateOnly.ParseExact(row.Cell(11).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            TelIs = row.Cell(12).IsEmpty() ? null : row.Cell(12).GetString(),
                            Voen = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetString(),
                            HesablashmaH = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                            Rekvizit = row.Cell(15).IsEmpty() ? null : row.Cell(15).GetString(),
                            SSN = row.Cell(16).IsEmpty() ? null : row.Cell(16).GetString(),
                            BankFilial = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                            BankFilialCode = row.Cell(18).IsEmpty() ? null : row.Cell(18).GetString(),
                            Kons = true,
                            SectionId = sectionId,
                            Status = 0,
                        };
                        var subProfessionNames = row.Cell(9).GetString().Split(',').Select(x => x.Trim()).ToList();
                        foreach (var subProfessionName in subProfessionNames)
                        {
                            var subProfession = _context.SubProfessions.FirstOrDefault(sp => sp.Name == subProfessionName);
                            if (subProfession != null)
                            {
                                expert.ExpertsProfessions.Add(new Models.ExpertsProfession 
                                { 
                                 Expert = expert,
                                    SubProfession = subProfession
                                });
                            }
                        }

                        experts.Add(expert);
                    }

                    if (experts.Any())
                    {
                        await _konsService.BulkAddAsync(experts);
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string statusReason)
        {
            var expert = await _konsService.GetByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }

            expert.Status = 1;
            expert.StatusReason = statusReason;
            await _konsService.UpdateAsync(expert);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStatus(int id)
        {
            var expert = await _konsService.GetByIdAsync(id);
            if (expert == null)
            {
                return NotFound();
            }

            expert.Status = 0;
            expert.StatusReason = null;
            await _konsService.UpdateAsync(expert);

            return RedirectToAction(nameof(Index));
        }
    }
}
