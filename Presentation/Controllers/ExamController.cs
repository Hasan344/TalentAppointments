using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ForQab.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using ForQab.DataAccess.ViewModel.Exam;
using ClosedXML.Excel;
using System.Data;
using ForQab.Presentation.ViewModels;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Authorization;
using ForQab.Service.Concrete;

namespace ForQab.Presentation.Controllers
{
    [Authorize]
    public class ExamController : BaseController
    {
        private readonly IExamService _examService;
        private readonly MyDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBadgeExportService _badgeExportService;

        public ExamController(IExamService examService, MyDbContext context, UserManager<ApplicationUser> userManager, IBadgeExportService badgeExportService)
         : base(context, userManager)
        {
            _examService = examService;
            _context = context;
            _userManager = userManager;
            _badgeExportService = badgeExportService;
        }

        public async Task<IActionResult> Index()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var exams = await _examService.GetExamsBySectionIdAsync(sectionId);
            ViewBag.Section = sectionId;
            return View(exams);
        }

        public async Task<IActionResult> Assesments()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var exams = await _examService.GetExamsBySectionIdAsyncForAssesment(sectionId);
            return View(exams);
        }
        public async Task<IActionResult> Appeals()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var exams = await _examService.GetExamsBySectionIdAsyncForAppeal(sectionId);
            return View(exams);
        }

        public async Task<IActionResult> Details(int id)
        {
            var viewModel = await _examService.GetExamDetailsAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }

            ViewBag.MonitorsWithLogs = viewModel.MonitorsWithLogs;
            ViewBag.ExpertsWithLogs = viewModel.ExpertsWithLogs;
            ViewBag.SectionId = await GetCurrentSectionIdAsync();

            return View(viewModel);
        }
        public async Task<IActionResult> DetailsOfAssesment(int id)
        {
            var viewModel = await _examService.GetExamDetailsAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }

            ViewBag.ExpertsWithLogs = viewModel.ExpertsWithLogs;
            ViewBag.SectionId = await GetCurrentSectionIdAsync();

            return View(viewModel);
        }
        public async Task<IActionResult> DetailsOfAppeal(int id)
        {
            var viewModel = await _examService.GetExamDetailsAsync(id);

            if (viewModel == null)
            {
                return NotFound();
            }

            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }

            ViewBag.ExpertsWithLogs = viewModel.ExpertsWithLogs;
            ViewBag.SectionId = await GetCurrentSectionIdAsync();

            return View(viewModel);
        }
        //[HttpGet]
        //public async Task<IActionResult> ChangeExpert(int examId, int expertId)
        //{
        //    var viewModel = await _examService.GetChangeExpertViewModelAsync(examId, expertId);
        //    if (viewModel == null) return NotFound();

        //    return View(viewModel);
        //}

        [HttpPost]
        public async Task<IActionResult> ChangeExpert(int examId, int expertId)
        {
            var success = await _examService.ChangeExpertAsync(examId, expertId);
            if (!success) return NotFound();

            return RedirectToAction("Details", new { id = examId });
        }

        public async Task<IActionResult> ChangeMonitor(int examId, int monitorId)
        {
            var viewModel = await _examService.GetChangeMonitorViewModelAsync(examId, monitorId, 2);

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeMonitor(ChangeMonitorViewModel model)
        {
            await _examService.ChangeMonitorAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> ChangeHeadMonitor(int examId, int monitorId)
        {
            var viewModel = await _examService.GetChangeMonitorViewModelAsync(examId, monitorId, 1);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeHeadMonitor(ChangeMonitorViewModel model)
        {
            await _examService.ChangeMonitorAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> ChangeWorker(int examId, int monitorId)
        {
            var viewModel = await _examService.GetChangeMonitorViewModelAsync(examId, monitorId, 5);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeWorker(ChangeMonitorViewModel model)
        {
            await _examService.ChangeMonitorAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> ChangeRepresentative(int examId, int representativeId)
        {
            var viewModel = await _examService.GetChangeRepresentativeViewModelAsync(examId, representativeId);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRepresentative(ChangeRepresentativeViewModel model)
        {
            await _examService.ChangeRepresentativeAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> ChangeMinistryRepresentative(int examId, int representativeId)
        {
            var viewModel = await _examService.GetChangeMinistryRepresentativeViewModelAsync(examId, representativeId);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeMinistryRepresentative(ChangeRepresentativeViewModel model)
        {
            await _examService.ChangeMinistryRepresentativeAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }

        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var viewModel = await _examService.PrepareCreateExamViewModelAsync(sectionId);
            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExamViewModel exam)
        {
            if (!ModelState.IsValid)
            {
                var sectionId = await GetCurrentSectionIdAsync();
                exam = await _examService.PrepareCreateExamViewModelAsync(sectionId);
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }

            await _examService.AddExamAsync(exam);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> CreateForAssesment()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateForAssesment(CreateExamViewModelForAssesment exam)
        {
            if (!ModelState.IsValid)
            {
                var sectionId = await GetCurrentSectionIdAsync();
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }

            await _examService.AddExamAsyncForAssesment(exam);
            return RedirectToAction(nameof(Assesments));
        }
        public async Task<IActionResult> CreateForAppeal()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateForAppeal(CreateExamViewModelForAssesment exam)
        {
            if (!ModelState.IsValid)
            {
                var sectionId = await GetCurrentSectionIdAsync();
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }

            await _examService.AddExamAsyncForAssesment(exam);
            return RedirectToAction(nameof(Appeals));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var viewModel = await _examService.PrepareEditExamViewModelAsync(id, sectionId);

            if (viewModel == null) return NotFound();
            if (!await IsSectionValidAsync<Exam>(id)) return Forbid();

            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditExamViewModel exam, int[] selectedCommissions, int[] selectedDegrees)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (!ModelState.IsValid)
            {
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }
            await _examService.UpdateExamAsync(exam, selectedCommissions, selectedDegrees);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> EditForAssesment(int id)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var viewModel = await _examService.PrepareEditExamViewModelAsyncForAssesment(id, sectionId);

            if (viewModel == null) return NotFound();
            if (!await IsSectionValidAsync<Exam>(id)) return Forbid();

            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditForAssesment(EditExamViewModelForAssesment exam)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (!ModelState.IsValid)
            {
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }

            await _examService.UpdateExamAsync(exam);
            return RedirectToAction(nameof(Assesments));
        }
        public async Task<IActionResult> EditForAppeal(int id)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var viewModel = await _examService.PrepareEditExamViewModelAsyncForAssesment(id, sectionId);

            if (viewModel == null) return NotFound();
            if (!await IsSectionValidAsync<Exam>(id)) return Forbid();

            await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditForAppeal(EditExamViewModelForAssesment exam)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            if (!ModelState.IsValid)
            {
                await _examService.PopulateViewBagsAsync(sectionId, ViewBag);
                return View(exam);
            }

            await _examService.UpdateExamAsync(exam);
            return RedirectToAction(nameof(Appeals));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
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

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdminValidAsync())
            {
                return Forbid();
            }
            await _examService.DeleteExamAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> AssignExperts(int id)
        {
            var section = await GetCurrentSectionIdAsync();
            ViewBag.Section = section;
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            var viewModel = await _examService.PrepareAssignExpertsViewModelAsync(exam);
            ViewBag.ExamName = exam.Name;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignExperts(AssignExpertToExamViewModel model)
        {
            var section = await GetCurrentSectionIdAsync();
            ViewBag.Section = section;
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

            var exam = await _examService.GetExamByIdAsync(model.ExamId);
            if (exam == null)
            {
                return NotFound();
            }

            model = await _examService.PrepareAssignExpertsViewModelAsync(exam);
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AssignExpertsForMX(int examId)
        {
            var sectionId = await GetCurrentSectionIdAsync();

            var rooms = await _context.ExamRooms.Where(er => er.SectionId == sectionId)
        .Select(r => new RoomSelectListItem
        {
            Value = r.Id,
            Text = r.Name
        })
        .ToListAsync();

            var viewModel = new AssignExpertForMXToExamViewModel
            {
                ExamId = examId,
                SectionId = (int)sectionId,
                Rooms = rooms
            };

            return View(viewModel);
        }

        // POST: Ekspertleri atama işlemi
        [HttpPost]
        public async Task<IActionResult> AssignExpertsForMX(AssignExpertForMXToExamViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Bütün bölmələri doldurun.";
                return RedirectToAction(nameof(AssignExpertsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }

            try
            {
                await _examService.AssignExpertsForMXToExamAsync(viewModel);
                return RedirectToAction("Details", new { id = viewModel.ExamId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ekspert təyinatı zamanı bir xəta baş verdi.: {ex.Message}";
                return RedirectToAction(nameof(AssignExpertsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AssignMonitorsForMX(int examId)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var rooms = await _context.ExamRooms.Where(er => er.SectionId == sectionId)
                .Select(r => new RoomSelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
                .ToListAsync();

            var roles = await _context.Roles.Where(r => r.Id < 3)
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name // Eğer Name kolonu varsa
                })
                .ToListAsync();

            var viewModel = new AssignMonitorForMXToExamViewModel
            {
                ExamId = examId,
                SectionId = (int)sectionId,
                Rooms = rooms,
                Roles = roles
            };

            return View(viewModel);
        }


        // POST: Ekspertleri atama işlemi
        [HttpPost]
        public async Task<IActionResult> AssignMonitorsForMX(AssignMonitorForMXToExamViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Bütün bölmələri doldurun.";
                return RedirectToAction(nameof(AssignMonitorsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }

            try
            {
                await _examService.AssignMonitorsForMXToExamAsync(viewModel);
                return RedirectToAction("Details", new { id = viewModel.ExamId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Təyinat zamanı bir xəta baş verdi.";
                return RedirectToAction(nameof(AssignMonitorsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> AssignWorkersForMX(int examId)
        {

            var workerTypes = await _context.WorkerTypes // WorkerType tablonuz varsa kullanın
                .Select(w => new SelectListItem
                {
                    Value = w.Id.ToString(),
                    Text = w.Name // Eğer Name kolonu varsa
                })
                .ToListAsync();

            var sectionId = await GetCurrentSectionIdAsync();

            var viewModel = new AssignWorkerForMXToExamViewModel
            {
                ExamId = examId,
                SectionId = (int)sectionId,
                WorkerTypes = workerTypes
            };

            return View(viewModel);
        }


        // POST: Ekspertleri atama işlemi
        [HttpPost]
        public async Task<IActionResult> AssignWorkersForMX(AssignWorkerForMXToExamViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Bütün bölmələri doldurun.";
                return RedirectToAction(nameof(AssignMonitorsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }

            try
            {
                await _examService.AssignWorkersForMXToExamAsync(viewModel);
                return RedirectToAction("Details", new { id = viewModel.ExamId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Təyinat zamanı bir xəta baş verdi.";
                return RedirectToAction(nameof(AssignMonitorsForMX), new { examId = viewModel.ExamId, sectionId = viewModel.SectionId });
            }
        }

        private List<SelectListItem> GetKindOptions()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "Gəlməyən haqqında qeyd" },
                new SelectListItem { Value = "1", Text = "Xaric olan haqqında qeyd" },
                new SelectListItem { Value = "2", Text = "Digər səbəblərlə bağlı qeyd" }
            };
        }
        public async Task<IActionResult> WriteMonitorLog(int examId, int monitorId, byte kind)
        {
            var user = await _userManager.GetUserAsync(User); // Giriş yapan kullanıcıyı al
            var viewModel = new WriteMonitorLogViewModel
            {
                ExamId = examId,
                MonitorId = monitorId,
                Kind = kind,
                UserName = user?.UserName,
                KindOptions = GetKindOptions()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WriteMonitorLog(WriteMonitorLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.KindOptions = GetKindOptions();
                return View(model);
            }

            model.UserName = (await _userManager.GetUserAsync(User))?.FirstName;
            await _examService.AddMonitorLogAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }

        public async Task<IActionResult> WriteExpertLog(int examId, int expertId, byte kind)
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = new WriteExpertLogsViewModel
            {
                ExamId = examId,
                ExpertId = expertId,
                Kind = kind,
                UserName = user?.UserName,
                KindOptions = GetKindOptions()
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WriteExpertLog(WriteExpertLogsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.KindOptions = GetKindOptions();
                return View(model);
            }


            model.UserName = (await _userManager.GetUserAsync(User))?.FirstName;
            await _examService.AddExpertLogAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }

        [HttpGet]
        public async Task<IActionResult> ExportToWord()
        {
            var memoryStream = await _examService.ExportExamScheduleToWord();
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "Təqvim qabiliyyət.docx");
        }


        private async Task<int?> GetCurrentSectionIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.SectionId != null ? user.SectionId : null;
        }

        [HttpGet]
        public async Task<IActionResult> AssignMonitors(int id)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            ViewBag.Section = sectionId;
            ViewBag.ExamName = exam.Name;
            var model = new AssignMonitorsToExamViewModel
            {
                ExamId = exam.Id,
                SectionId = exam.SectionId,
                Rooms = _context.ExamRooms.Where(er => er.SectionId == sectionId).Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList(),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMonitors(AssignMonitorsToExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var assignment in model.Assignments)
                    {

                        int genderId = assignment.GenderId.HasValue ? assignment.GenderId.Value : 0;
                        int roomId = assignment.RoomId.HasValue ? assignment.RoomId.Value : 0;
                        DateOnly maxDate = assignment.MaxDate.HasValue ? assignment.MaxDate.Value : new DateOnly();
                        await _examService.AssignRandomMonitorsToExamAsync(
                            model.ExamId,
                            assignment.NumberOfMonitors,
                            genderId,
                            maxDate,
                            roomId
                        );
                        Console.WriteLine($"Monitor: {assignment.NumberOfMonitors}, IsReserve: {assignment.IsReserve}");
                    }

                    TempData["SuccessMessage"] = "Nəzarətçilər uğurla təyin edildi!";
                    return RedirectToAction(nameof(Details), new { id = model.ExamId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    ViewBag.ErrorMessage = ex.Message;
                }
            }

            var exam = await _examService.GetExamByIdAsync(model.ExamId);
            if (exam != null)
            {
                ViewBag.ExamName = exam.Name;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AssignHeadMonitors(int id)
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
        public async Task<IActionResult> AssignHeadMonitors(AssignMonitorsToExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var assignment in model.Assignments)
                    {
                        await _examService.AssignRandomHeadMonitorsToExamAsync(
                            model.ExamId,
                            assignment.NumberOfMonitors,
                            assignment.GenderId,
                            assignment.MaxDate
                        );
                    }

                    TempData["SuccessMessage"] = "İmtahan rəhbərləri uğurla təyin edildi!";
                    return RedirectToAction(nameof(Details), new { id = model.ExamId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    ViewBag.ErrorMessage = ex.Message;
                }
            }

            var exam = await _examService.GetExamByIdAsync(model.ExamId);
            if (exam != null)
            {
                ViewBag.ExamName = exam.Name;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignWorkers(int id)
        {

            await _examService.AssignWorkersToExamAsync(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AssignVolunteers(int id)
        {

            await _examService.AssignVolunteersToExamAsync(id);

            return RedirectToAction(nameof(Details), new { id });
        }

        public IActionResult ExpertDetails(int id)
        {
            var expert = _context.Experts.Find(id);
            if (expert == null)
                return NotFound();
            return RedirectToAction("Details", "Expert", new { id });
        }

        public IActionResult MonitorDetails(int id)
        {
            var monitor = _context.Monitors.Find(id);
            if (monitor == null)
                return NotFound();
            return RedirectToAction("Details", "Monitors", new { id });
        }

        public IActionResult HeadMonitorDetails(int id)
        {
            var monitor = _context.Monitors.Find(id);
            if (monitor == null) return NotFound();
            return RedirectToAction("Details", "HeadMonitors", new { id });
        }

        public IActionResult WorkerDetails(int id)
        {
            var monitor = _context.Monitors.Find(id);
            if (monitor == null)
                return NotFound();
            return RedirectToAction("Details", "Worker", new { id });
        }
        public IActionResult DimRepresentativeDetails(int id)
        {
            var monitor = _context.Monitors.Find(id);
            if (monitor == null)
                return NotFound();
            return RedirectToAction("Details", "Worker", new { id });
        }
        [HttpPost]
        public async Task<IActionResult> ExportContingentRegister(int examId)
        {
            var fileContents = await _examService.ExportExamToWordAsync(examId);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"İşçi heyət qeydiyyat vərəqi (İmtahan Id:{examId}).docx");
        }
        [HttpPost]
        public async Task<IActionResult> ExportExpertRegister(int examId)
        {
            var fileContents = await _examService.ExportExamToWordAsyncForMX(examId);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"İşçi heyət qeydiyyat vərəqi (İmtahan Id:{examId}).docx");
        }
        [HttpPost]
        public async Task<IActionResult> ExportVolunteerRegister(int examId)
        {
            var fileContents = await _examService.ExportExamToWordAsyncForV(examId);
            return File(fileContents, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Könüllü qeydiyyat vərəqi (İmtahan Id:{examId}).docx");
        }

        public async Task<IActionResult> ExportToExcel()
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var exams = await _examService.GetExamsBySectionIdAsync(sectionId);

            var dt = new DataTable("Exams");
            dt.Columns.AddRange(new DataColumn[]
            {
                    new DataColumn("Name"),
                    new DataColumn("Exam Date"),
                    new DataColumn("Duration"),
                    new DataColumn("Water Provided"),
                    new DataColumn("Food Provided"),
                    //new DataColumn("Commission"),
                    new DataColumn("Exam Building"),
                    new DataColumn("Section"),
            });

            foreach (var exam in exams)
            {
                dt.Rows.Add(
                    exam.Name,
                    exam.ExamDate.ToString("yyyy-MM-dd"),
                    exam.Duration,
                    exam.Water,
                    exam.Food,
                    //exam.Commission?.Name ?? "---",
                    exam.ExamBuilding?.Name ?? "---",
                    exam.Section?.Name ?? "---"
                );
            }

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

        [HttpGet]
        public async Task<IActionResult> GetSubProfessionsByFederation(int federationId)
        {
            var subProfessions = await _context.SubProfessions
                .Where(sp => sp.ProfessionId == federationId)
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync();

            return Json(subProfessions);
        }
        public async Task<IActionResult> AssignRepresentatives(int examId)
        {
            var representatives = await _examService.GetAvailableRepresentativesAsync();

            var viewModel = new AssignRepresentativesToExamViewModel
            {
                ExamId = examId,
                Representatives = representatives.Select(r => new RepresentativeViewModelForAssign
                {
                    Id = r.Id,
                    Name = r.Name,
                    Surname = r.Surname,
                    FinCode = r.FinCode
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRepresentatives(AssignRepresentativesToExamViewModel model)
        {
            if (model.SelectedRepresentativeIds == null || !model.SelectedRepresentativeIds.Any())
            {
                ModelState.AddModelError("", "Ən azı bir DİM Nümayəndəsi seçilməlidir.");
                return View(model);
            }

            await _examService.AssignRepresentativesToExamAsync(model.ExamId, model.SelectedRepresentativeIds);

            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> AssignMinistryRepresentatives(int examId)
        {
            var representatives = await _examService.GetAvailableMinistryRepresentativesAsync();

            var viewModel = new AssignRepresentativesToExamViewModel
            {
                ExamId = examId,
                Representatives = representatives.Select(r => new RepresentativeViewModelForAssign
                {
                    Id = r.Id,
                    Name = r.Name,
                    Surname = r.Surname,
                    FinCode = r.FinCode
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignMinistryRepresentatives(AssignRepresentativesToExamViewModel model)
        {
            if (model.SelectedRepresentativeIds == null || !model.SelectedRepresentativeIds.Any())
            {
                ModelState.AddModelError("", "Ən azı bir Nazirlik Nümayəndəsi seçilməlidir.");
                return View(model);
            }

            await _examService.AssignMinistryRepresentativesToExamAsync(model.ExamId, model.SelectedRepresentativeIds);

            return RedirectToAction("Details", new { id = model.ExamId });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteExperts(int examId, List<int> expertIds)
        {
            try
            {
                await _examService.RemoveExpertsFromExamAsync(examId, expertIds);
                return RedirectToAction("Details", new { id = examId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ManageMonitors(int examId, string action, List<AttendanceUpdateModel> updates, int[] monitorIds)
        {
            if (action == "delete" && monitorIds != null && monitorIds.Any())
            {
                try
                {
                    await _examService.RemoveMonitorsFromExamAsync(examId, monitorIds.ToList());
                    return RedirectToAction("Details", new { id = examId });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else if (action == "update" && updates != null && updates.Any())
            {
                foreach (var item in updates)
                {
                    var entity = _context.ExamMonitors
                        .FirstOrDefault(e => e.ExamId == examId && e.MonitorId == item.MonitorId);
                    if (entity != null)
                    {
                        entity.IsAttended = item.IsAttended;
                    }
                }
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SaveChanges hatası: {ex.Message}");
                    return BadRequest("Veritabanı güncelleme hatası: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Geçersiz action veya veri eksik!");
            }

            return RedirectToAction("Details", new { id = examId });
        }
        //[HttpPost]
        //public async Task<IActionResult> DeleteMonitors(int examId, List<int> monitorIds)
        //{
        //    try
        //    {
        //        await _examService.RemoveMonitorsFromExamAsync(examId, monitorIds);
        //        return RedirectToAction("Details", new { id = examId });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> DeleteHeadMonitors(int examId, List<int> monitorIds)
        {
            try
            {
                await _examService.RemoveMonitorsFromExamAsync(examId, monitorIds);
                return RedirectToAction("Details", new { id = examId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetExpertsByKons(bool kons)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var experts = await _context.Experts
                .Where(e => e.Kons == kons && e.SectionId == sectionId).OrderBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name + " " + e.Surname + " " + e.Fname
                })
                .ToListAsync();

            return Json(experts);
        }
        [HttpGet]
        public async Task<IActionResult> GetMonitorsByType(byte workerType)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var monitors = await _context.Monitors
                .Where(e => e.WorkerType == workerType && e.SectionId == sectionId).OrderBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name + " " + e.Surname + " " + e.Fname
                })
                .ToListAsync();

            return Json(monitors);
        }
        [HttpGet]
        public async Task<IActionResult> GetMonitorsByRole(int role)
        {
            var sectionId = await GetCurrentSectionIdAsync();
            var monitors = await _context.Monitors
                .Where(e => e.Role == role && e.SectionId == sectionId).OrderBy(e => e.Name)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.Name + " " + e.Surname + " " + e.Fname
                })
                .ToListAsync();

            return Json(monitors);
        }
        public IActionResult ExportExamData()
        {
            return View(); 
        }

        [HttpPost]
        
        public async Task<IActionResult> ExportExamData(DateOnly selectedDate)
        {
            var fileContent = await _examService.GetExamDataForExport(selectedDate);
            return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ExamData_{selectedDate}.xlsx");
        }

        [HttpPost]
        public IActionResult ExportBadges(int examId)
        {
            try
            {
                var fileBytes = _badgeExportService.GenerateBadges(examId);
                var fileName = $"Imtahan_Badgeleri_{examId}.docx";

                return File(fileBytes,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    fileName);
            }
            catch (Exception ex)
            {
                // Gerekirse logla
                return BadRequest("Xəta baş verdi: " + ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> ManageExperts(int examId, string action, List<AttendanceUpdateModel> updates, int[] expertIds)
        {
            if (action == "delete" && expertIds != null && expertIds.Any())
            {
                try
                {
                    await _examService.RemoveExpertsFromExamAsync(examId, expertIds.ToList());
                    return RedirectToAction("Details", new { id = examId });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else if (action == "update" && updates != null && updates.Any())
            {
                foreach (var item in updates)
                {
                    var entity = _context.ExamExpertSubProfessions
                        .FirstOrDefault(e => e.ExamId == examId && e.ExpertId == item.ExpertId);
                    if (entity != null)
                    {
                        entity.IsAttended = item.IsAttended; 
                    }
                    else
                    {
                        Console.WriteLine($"Entity not found for ExpertId: {item.ExpertId}");
                    }
                }
                try
                {
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SaveChanges error: {ex.Message}");
                    return BadRequest("Veritabanı güncelleme hatası: " + ex.Message);
                }
            }

            return RedirectToAction("Details", new { id = examId });
        }

    }
}