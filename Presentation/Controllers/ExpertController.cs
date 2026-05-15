using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ForQab.Data_Access.ViewModel;
using ForQab.Data_Access.ViewModel.Expert;
using ForQab.DataAccess.Models;
using ForQab.Models;
using ForQab.Presentation.Validators;
using ForQab.Service;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Threading;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class ExpertController : BaseController
    {
        private readonly IExpertService _expertService;
        private readonly ISubProfessionService _subProfessionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAmasPhotoService _amasPhotoService;
        private readonly ExpertValidator _expertValidator;
        private readonly ExpertEditValidator _expertEditValidator;


        public ExpertController(IExpertService expertService, ISubProfessionService subProfessionService, MyDbContext context, UserManager<ApplicationUser> userManager, IAmasPhotoService amasPhotoService, ExpertValidator expertValidator, ExpertEditValidator expertEditValidator) : base(context, userManager)
        {
            _expertService = expertService;
            _subProfessionService = subProfessionService;
            _userManager = userManager;
            _amasPhotoService = amasPhotoService;
            _expertValidator = expertValidator;
            _expertEditValidator = expertEditValidator;
        }

        public async Task<IActionResult> Index(
    string searchName,
    int? genderId,
    string? finCode,
    string serial,
    int? district,
    int? startYear,
    int? endYear,
    int? federationId,
    int? subProfessionId,
    DateTime? createdStartDate,
    DateTime? createdEndDate,
    int page = 1,
    int pageSize = 25)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = _context.Professions.Where(f => f.SectionId == sectionId).ToList();

            var all = await _expertService.GetExpertsBySectionIdAsync(
                sectionId, searchName, genderId, finCode, serial,
                district, startYear, endYear, federationId, subProfessionId, createdStartDate, createdEndDate);

            var totalCount = all.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Federation = federations;
            ViewBag.Districts = districts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(paged);
        }
        public async Task<IActionResult> Archived(string searchName, int? genderId, string? finCode, string serial, int? district, int? startYear, int? endYear, int? subProfessionId,
    DateTime? createdStartDate,
    DateTime? createdEndDate)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var genders = _context.Genders.ToList();
            var districts = _context.Districts.ToList();
            var subProfessions = await _subProfessionService.GetAllSubProfessionsAsync(sectionId);
            var federations = _context.Professions.Where(f => f.SectionId == sectionId).ToList();
            var model = await _expertService.GetArchivedExpertsBySectionIdAsync(sectionId, searchName, genderId, finCode, serial, district, startYear, endYear, subProfessionId, createdStartDate, createdEndDate);
            ViewBag.SubProfessions = subProfessions;
            ViewBag.Genders = genders;
            ViewBag.Federation = federations;
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpertViewModel expertViewModel)
        {
            var result = await _expertValidator.ValidateAsync(expertViewModel);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadCreateViewBags(expertViewModel);
                return View(expertViewModel);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadCreateViewBags(expertViewModel);
                return View(expertViewModel);
            }

            try
            {
                await _expertService.AddExpertAsync(expertViewModel);
                TempData["SuccessMessage"] = "Ekspert uğurla əlavə edildi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yaddaşa yazma zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yaddaşa yazma zamanı xəta baş verdi.";
                await ReloadCreateViewBags(expertViewModel);
                return View(expertViewModel);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.Section = sectionId;

            if (expert == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Expert>(id))
            {
                return Forbid();
            }

            var federations = await _expertService.GetFederationsAsync(sectionId);
            var sections = await _expertService.GetSectionsAsync(sectionId);

            // Federation'a bağlı SubProfession'ları yükle
            var allSubProfessions = sectionId == 1
                ? await _expertService.GetSubProfessionsByFederationAsync((int)expert.Federation)
                : await _expertService.GetSubProfessionsAsync(sectionId);

            var selectedIds = expert.ExpertsProfessions.Select(sp => sp.SubProfessionId).ToList();

            var subProfessions = allSubProfessions
                .Select(sp => new SelectListItem
                {
                    Value = sp.Id.ToString(),
                    Text = sp.Name,
                    Selected = selectedIds.Contains(sp.Id)
                })
                .ToList();

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
                SerialPrefix = expert.SerialPrefix,
                Serial = expert.Serial,
                Profession = expert.Profession,
                Gender = expert.Gender,
                Federation = expert.Federation,
                TelEl = expert.TelEl,
                TelIs = expert.TelIs,
                SelectedSubProfessions = expert.ExpertsProfessions.Select(sp => sp.SubProfessionId).ToArray(),
                SubProfessions = subProfessions
            };

            ViewData["SectionId"] = new SelectList(sections, "Id", "Name", expert.SectionId);
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name", expert.Federation);

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

            var result = await _expertEditValidator.ValidateAsync(expert);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                }
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadEditViewBags(expert);
                return View(expert);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Formada xətalar var. Qırmızı ilə işarələnmiş sahələri yoxlayın.";
                await ReloadEditViewBags(expert);
                return View(expert);
            }

            try
            {
                await _expertService.UpdateExpertAsync(expert);
                TempData["SuccessMessage"] = "Ekspert məlumatları yeniləndi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpertExists(expert.Id)) return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Yeniləmə zamanı xəta: {ex.Message}");
                TempData["ErrorMessage"] = "Yeniləmə zamanı xəta baş verdi.";
                await ReloadEditViewBags(expert);
                return View(expert);
            }
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            var expert = await _expertService.GetExpertByIdAsync(id);
            if (expert != null)
            {
                await _expertService.DeleteExpertAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> ExpertLogs()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var logs = await _expertService.GetExpertLogsAsync(sectionId);

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
            experts = experts.Where(m => m.Archive == 0 && m.Status == 0).ToList();

            // DataTable oluştur
            var dt = new DataTable("Ekspertlər");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Ad"),
                new DataColumn("Soyad"),
                new DataColumn("Ata adı"),
                new DataColumn("İstiqamət"),
                new DataColumn("Fin kodu"),
                new DataColumn("Seriya nömrəsi"),
                new DataColumn("Seriyası"),
                new DataColumn("Cinsi"),
                new DataColumn("Vəzifəsi"),
                new DataColumn("Doğum tarixi"),
                new DataColumn("Rekvizit"),
                new DataColumn("SSN"),
                new DataColumn("Bank filialı"),
                new DataColumn("Bank filial kodu"),
                new DataColumn("Hesablaşma hesabı"),
                new DataColumn("VÖEN"),
                new DataColumn("Müəssisə - Təhsil səviyyəsi"),
                new DataColumn("İxtisaslar"),
                new DataColumn("Müqavilə tarixi"),
                new DataColumn("Müqavilə nömrəsi"),
                new DataColumn("İştirak sayı 1 növbə"),
                new DataColumn("İştirak sayı 2 növbə"),
            });

            // Verileri doldur
            foreach (var expert in experts)
            {
                var subProfessions = expert.ExpertsProfessions != null && expert.ExpertsProfessions.Any()
                    ? string.Join(", ", expert.ExpertsProfessions.Select(sp => sp.SubProfession?.Name))
                    : "---";
                var latestContract = expert.Contracts?
                                           .OrderByDescending(c => c.Date)
                                           .FirstOrDefault();

                dt.Rows.Add(
                    expert.Name,
                    expert.Surname,
                    expert.Fname,
                    expert.Section?.Name ?? "---",
                    expert.FinCode,
                    expert.SerialPrefix,
                    expert.Serial,
                    expert.GenderNavigation?.Name,
                    expert.Profession,
                    expert.BirthDate,
                    expert.Rekvizit,
                    expert.SSN,
                    expert.BankFilial,
                    expert.BankFilialCode,
                    expert.HesablashmaH,
                    expert.Voen,
                    expert.FederationNavigation?.Name,
                    subProfessions,
                    latestContract?.Date.ToString("dd.MM.yyyy") ?? "", 
                    latestContract?.Number ?? "",
                    expert.ComputedAssignmentCountShift1,
                    expert.ComputedAssignmentCountShift2
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
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ekspertlər.xlsx");
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["ImportError"] = "Excel faylı yüklənməmişdir.";
                return RedirectToAction(nameof(Index));
            }
           
            var sectionId = await GetCurrentSectionIdAsync();
            if (sectionId == null)
            {
                ModelState.AddModelError("", "Admin fayl yükləməsi edə bilməz.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
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
                            var finCode = row.Cell(8).GetString();
                            //if (string.IsNullOrEmpty(finCode) || existingFinCodes.Contains(finCode))
                            //{
                            //    TempData["ImportError"] = $"Eyni fin kodda ekspert daxil edilməyə çalışıldı: {finCode}";
                            //    return RedirectToAction(nameof(Index));
                            //}

                            var expert = new Expert
                            {
                                District = _context.Districts.FirstOrDefault(d => d.Name == row.Cell(1).GetString())?.Id,
                                Surname = row.Cell(2).GetString(),
                                Name = row.Cell(3).GetString(),
                                Fname = row.Cell(4).GetString(),
                                Gender = row.Cell(5).GetValue<byte>(),
                                SerialPrefix = row.Cell(6).IsEmpty() ? null : row.Cell(6).GetString(),
                                Serial = row.Cell(7).IsEmpty() ? null : row.Cell(7).GetString(),
                                FinCode = finCode,
                                Federation = _context.Professions.FirstOrDefault(p => p.Name == row.Cell(9).GetString())?.Id,
                                Profession = row.Cell(11).IsEmpty() ? null : row.Cell(11).GetString(),
                                BirthDate = row.Cell(12).IsEmpty() ? null
                                    : DateOnly.ParseExact(row.Cell(12).GetString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                TelIs = row.Cell(13).IsEmpty() ? null : row.Cell(13).GetString(),
                                Voen = row.Cell(14).IsEmpty() ? null : row.Cell(14).GetString(),
                                HesablashmaH = row.Cell(15).IsEmpty() ? null : row.Cell(15).GetString(),
                                Rekvizit = row.Cell(16).IsEmpty() ? null : row.Cell(16).GetString(),
                                SSN = row.Cell(17).IsEmpty() ? null : row.Cell(17).GetString(),
                                BankFilial = row.Cell(18).IsEmpty() ? null : row.Cell(18).GetString(),
                                BankFilialCode = row.Cell(19).IsEmpty() ? null : row.Cell(19).GetString(),
                                Kons = false,
                                SectionId = sectionId,
                                Status = 0,
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            var subProfessionNames = row.Cell(10).GetString().Split(',').Select(x => x.Trim()).ToList();
                            foreach (var subProfessionName in subProfessionNames)
                            {
                                var subProfession = _context.SubProfessions.FirstOrDefault(sp => sp.Name == subProfessionName);
                                if (subProfession != null)
                                {
                                    expert.ExpertsProfessions.Add(new ExpertsProfession
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
                            await _expertService.BulkAddAsync(experts);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ImportError"] = $"Xəta baş verdi: {ex.Message}";
                return RedirectToAction(nameof(Index));
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
            //var expert = await _expertService.GetExpertByIdAsync(id);
            //if (expert == null)
            //{
            //    return NotFound();
            //}
            //if (!await IsSectionValidAsync<Expert>(id))
            //{
            //    return Forbid();
            //}
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
            expert.Photo = null;
            await _expertService.UpdateExpertAsync(expert);

            TempData["SuccessMessage"] = "Ekspert arxivə göndərildi.";
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
            expert.Photo = null;
            await _expertService.UpdateExpertAsync(expert);

            TempData["SuccessMessage"] = "Ekspert arxivdən çıxarıldı.";
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
            monitor.Photo = null;
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
            monitor.Photo = null;
            await _expertService.UpdateExpertAsync(monitor);

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ExportContracts(List<int> selectedExpertIds, DateTime contractDate)
        {
            if (selectedExpertIds == null || !selectedExpertIds.Any())
                return RedirectToAction(nameof(Index));

            var bytes = await _expertService
                .ExportContractsToWordAsync(selectedExpertIds, contractDate);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "Mugavileler.docx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkArchive(List<int> selectedIds, string archiveReason)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "Heç bir ekspert seçilməyib.";
                return RedirectToAction(nameof(Index));
            }

            await _expertService.BulkArchiveAsync(selectedIds, archiveReason ?? "");
            TempData["SuccessMessage"] = $"{selectedIds.Count} ekspert arxivə göndərildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FetchPhoto(int id)
        {
            var expert = await _expertService.GetExpertByIdAsync(id);
            if (expert == null) return NotFound();

            if (string.IsNullOrWhiteSpace(expert.FinCode) ||
                string.IsNullOrWhiteSpace(expert.Serial))
            {
                TempData["ErrorMessage"] = "FİN Kod və ya Seriya nömrəsi boşdur. Şəkil çəkilə bilmədi.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var photo = await _amasPhotoService.FetchPhotoAsBase64Async(
                expert.FinCode, expert.SerialPrefix ?? "", expert.Serial);

            if (photo == null)
            {
                TempData["ErrorMessage"] = "AMAS sistemindən şəkil tapılmadı.";
                return RedirectToAction(nameof(Details), new { id });
            }

            expert.Photo = photo;
            await _expertService.UpdateExpertAsync(expert);

            TempData["SuccessMessage"] = "Şəkil AMAS-dan uğurla yükləndi.";
            return RedirectToAction(nameof(Details), new { id });
        }
        private async Task ReloadCreateViewBags(ExpertViewModel vm)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.Section = sectionId;
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var subProfessions = await _expertService.GetSubProfessionsAsync(sectionId);
            var federations = await _expertService.GetFederationsAsync(sectionId);

            vm.SubProfessions = subProfessions.Select(sp => new SelectListItem
            {
                Text = sp.Name,
                Value = sp.Id.ToString()
            }).ToList();

            ViewBag.SectionList = new SelectList(sections, "Id", "Name");
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
        }

        private async Task ReloadEditViewBags(ExpertEditViewModel vm)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            ViewBag.Section = sectionId;
            var sections = await _expertService.GetSectionsAsync(sectionId);
            var federations = await _expertService.GetFederationsAsync(sectionId);

            ViewData["SectionId"] = new SelectList(sections, "Id", "Name");
            ViewBag.GenderList = new SelectList(_context.Genders.ToList(), "Id", "Name");
            ViewBag.FederationList = new SelectList(federations, "Id", "Name");
        }
    }

}