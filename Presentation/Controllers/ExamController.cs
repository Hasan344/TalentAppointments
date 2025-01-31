using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ForQab.DataAccess.Models;
using ForQab.Service;
using Microsoft.AspNetCore.Identity;
using ForQab.DataAccess.ViewModel.Exam;
using ClosedXML.Excel;
using System.Data;

namespace ForQab.Presentation.Controllers
{
    public class ExamController : BaseController
    {
        private readonly IExamService _examService;
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(IExamService examService,  MyDbContext context, UserManager<ApplicationUser> userManager)
         : base(context, userManager)
        {
            _examService = examService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId= await GetCurrentSectionIdAsync();
            var exams = await _examService.GetExamsBySectionIdAsync(sectionId);
            return View(exams);
        }

        public async Task<IActionResult> Details(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            var sectionId = await GetCurrentSectionIdAsync();
            var examWithExperts = await _context.Exams
                                        .Include(e => e.Experts)  // Include the related experts
                                        .FirstOrDefaultAsync(e => e.Id == id);
            if (exam == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }
            return View(exam);
        }

        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (sectionId==null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.Where(e => e.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.Where(c => c.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.Where(sc => sc.SectionId == sectionId).ToListAsync(), "Id", "Name");

            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                var sectionId = await GetCurrentSectionIdAsync();
                if (sectionId == null)
                {
                    ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
                    ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.ToListAsync(), "Id", "Name");
                    ViewBag.CommissionList = new SelectList(await _context.Commissions.ToListAsync(), "Id", "Name");
                    ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.ToListAsync(), "Id", "Name");
                }
                else
                {
                    ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
                    ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.Where(e => e.SectionId == sectionId).ToListAsync(), "Id", "Name");
                    ViewBag.CommissionList = new SelectList(await _context.Commissions.Where(c => c.SectionId == sectionId).ToListAsync(), "Id", "Name");
                    ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.Where(sc => sc.SectionId == sectionId).ToListAsync(), "Id", "Name");

                }
                return View(exam); // Ensure drop-downs are reloaded on error
            }

            await _examService.AddExamAsync(exam);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.Where(e => e.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.Where(c => c.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.Where(sc => sc.SectionId == sectionId).ToListAsync(), "Id", "Name");

            }
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }
            
            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Exam exam)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.Where(e => e.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.CommissionList = new SelectList(await _context.Commissions.Where(c => c.SectionId == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.SubCommissionList = new SelectList(await _context.SubCommissions.Where(sc => sc.SectionId == sectionId).ToListAsync(), "Id", "Name");

            }
            if (ModelState.IsValid)
            {
                await _examService.UpdateExamAsync(exam);
                return RedirectToAction(nameof(Index));
            }
            
            return View(exam);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }
            return View(exam);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _examService.DeleteExamAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> AssignExperts(int id)
        {
            int sectionId = _context.Exams
                                     .Where(e => e.Id == id)
                                     .Select(i => i.SectionId)
                                     .FirstOrDefault();
            var availableSubProfessions = await _examService.GetSubprofessionsBySectionIdAsync(sectionId); // SubProfessions listesini al

            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            // SubProfessions listesini ve diğer gerekli bilgileri ViewModel'e aktar
            var viewModel = new AssignExpertToExamViewModel
            {
                ExamId = exam.Id,
                SectionId = exam.SectionId,
                SubProfessions = availableSubProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };

            ViewBag.ExamName = exam.Name; // Sınav adı
            return View(viewModel); // View'a model aktar
        }



        [HttpPost]
        public async Task<IActionResult> AssignExperts(AssignExpertToExamViewModel model)
        {
            try
            {
                bool success = await _examService.AssignExpertsAsync(model);
                if (success)
                {
                    TempData["SuccessMessage"] = "Ekspertlər uğurla təyin edildi!";
                    return RedirectToAction(nameof(Details), new { id = model.ExamId });
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            // Hata durumunda, SubProfessions verisini tekrar yükleyelim
            var sectionId = await _examService.GetSectionIdByExamIdAsync(model.ExamId);
            model.SubProfessions = (await _examService.GetSubprofessionsBySectionIdAsync(sectionId))
                .Select(sp => new SelectListItem { Value = sp.Id.ToString(), Text = sp.Name })
                .ToList();

            return View(model);
        }



        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }

        [HttpGet]
        public async Task<IActionResult> AssignMonitors(int id)
        {
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            ViewBag.ExamName = exam.Name;
            var model = new AssignMonitorsToExamViewModel
            {
                ExamId = exam.Id,
                SectionId = exam.SectionId,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMonitors(AssignMonitorsToExamViewModel model)
        {
            var availableMonitors = await _context.Monitors.Where(e => e.SectionId == model.SectionId).ToListAsync();
            if (model.NumberOfMonitors > availableMonitors.Count)
            {
                ViewBag.ErrorMessage = "Təyin etmək istədiyiniz Nəzarətçi sayı mövcud nəzarətçi sayını aşır!";
                return RedirectToAction("AssignMonitors", new { id = model.ExamId });
            }
           
            if (ModelState.IsValid)
            {
                try
                {
                    await _examService.AssignRandomMonitorsToExamAsync(model.ExamId, model.NumberOfMonitors);
                    TempData["SuccessMessage"] = "Ekspert uğurla təyin edildi!";
                    return RedirectToAction(nameof(Details), new { id = model.ExamId });
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Xəta baş verdi: " + ex.Message;
                }
            }

            var exam = await _examService.GetExamByIdAsync(model.ExamId);
            if (exam != null)
            {
                ViewBag.ExamName = exam.Name;
            }

            return View(model);
        }
        public async Task<IActionResult> ExportToExcel()
        {
            var sectionId=await GetCurrentSectionIdAsync();
            // Mevcut verileri alın
            var exams = await _examService.GetExamsBySectionIdAsync(sectionId);

            // DataTable oluştur
            var dt = new DataTable("Exams");
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Name"),
                    new DataColumn("Exam Date"),
                    new DataColumn("Duration"),
                    new DataColumn("Water Provided"),
                    new DataColumn("Food Provided"),
                    new DataColumn("Commission"),
                    new DataColumn("Exam Building"),
                    new DataColumn("Section"),
                    new DataColumn("Sub Commission"),
                });

            // Verileri doldur
            foreach (var exam in exams)
            {
                dt.Rows.Add(
                    exam.Name,
                    exam.ExamDate.ToString("yyyy-MM-dd"), // Tarih formatı
                    exam.Duration,
                    exam.Water ,
                    exam.Food,
                    exam.Commission?.Name ?? "---",
                    exam.ExamBulding?.Name ?? "---",
                    exam.Section?.Name ?? "---",
                    exam.SubCommission?.Name ?? "---"
                );
            }

            // Excel dosyasını oluştur
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt, "Exams");
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Exams.xlsx");
                }
            }
        }

    }
}

