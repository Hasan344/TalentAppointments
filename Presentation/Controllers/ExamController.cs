using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ForQab.DataAccess.Models;
using ForQab.Service;
using Microsoft.AspNetCore.Identity;
using ForQab.DataAccess.ViewModel.Exam;
using ClosedXML.Excel;
using System.Data;
using ForQab.Presentation.ViewModels;
using ForQab.Models.ViewModels;

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

            if (exam == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }

            var monitorIds = exam.Monitors.Select(m => m.Id).ToList();
            var expertIds = exam.Experts.Select(m => m.Id).ToList();
            var monitorLogs = await _examService.GetMonitorsWithLogsAsync(monitorIds);
            var expertLogs = await _examService.GetExpertsWithLogsAsync(expertIds);

            ViewBag.MonitorsWithLogs = monitorLogs ?? new List<int>();
            ViewBag.ExpertsWithLogs = expertLogs ?? new List<int>();

            // Entity Model'den ViewModel'e Dönüşüm
            var viewModel = new ExamDetailsViewModel
            {
                Id = exam.Id,
                Name = exam.Name,
                Section = new SectionViewModel { Name = exam.Section.Name },
                ExamBulding = new BuildingViewModel { Name = exam.ExamBuilding.Name },
                ExamDate = exam.ExamDate,
                Duration = exam.Duration,
                Water = exam.Water,
                Food = exam.Food,
                Notes = exam.Notes ?? string.Empty,
                InventoryTransport = exam.InventoryTransport ?? string.Empty,
                ExamCommissions = exam.ExamCommissions.Select(ec => new ExamCommissionViewModel
                {
                    Commission = new CommissionViewModel { Name = ec.Commission.Name }
                }).ToList(),
                Experts = exam.Experts.Select(e => new ExpertViewModelForExam
                {
                    Id = e.Id,
                    Name = e.Name,
                    Surname = e.Surname,
                    Fname = e.Fname,
                    FinCode = e.FinCode,
                    ExamExpertSubProfessions = e.ExamExpertSubProfessions
                        .Where(eesp => eesp.SubProfession != null && eesp.Federation != null)
                        .Select(eesp => new SubProfessionViewModelForExam
                        {
                            Name = eesp.SubProfession.Name,
                            FederationName = eesp.Federation.Name
                        }).ToList()
                }).ToList(),
                Monitors = exam.Monitors.Select(m => new MonitorViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Surname = m.Surname,
                    Fname = m.Fname,
                    FinCode = m.FinCode,
                    Role = m.Role
                }).ToList(),
                ExpertsWithLogs = expertLogs ?? new List<int>(),
            };

            return View(viewModel);
        }



        public  async Task<IActionResult> ChangeExpert(int examId, int expertId)
        {
           
            var exam = _context.Exams
                .Include(e => e.Experts)
                .FirstOrDefault(e => e.Id == examId);
            var sectionId = await GetCurrentSectionIdAsync();
            var subProfession = _context.ExamExpertSubProfessions
                                        .Include(e => e.SubProfession)
                                        .Where(e => e.ExamId == examId && e.ExpertId == expertId)
                                        .Select(e => e.SubProfessionId)
                                        .FirstOrDefault();
            if (exam == null) return NotFound();

            var selectedExpertList = exam.Experts.Select(e => e.Id).ToList();

            var expertList = _context.Experts
                                     .Where(e => e.SectionId == sectionId)
                                     .Where(e => e.SubProfessions.Any(sp => sp.Id == subProfession) && !selectedExpertList.Contains(e.Id))// 🔥 Yalnız uyğun subprofession olanlar
                                     .ToList();

            var viewModel = new ChangeExpertViewModel
            {
                ExamId = examId,
                CurrentExpertId = expertId,
                AvailableExperts = expertList.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} {e.Surname} ({e.FinCode})"
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeExpert(ChangeExpertViewModel model)
        {
            var exam = _context.Exams
                .Include(e => e.Experts)
                .Include(e => e.ExamExpertSubProfessions) // 🔥 SubProfession ilişkisini de çekiyoruz
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null) return NotFound();

            using (var transaction = _context.Database.BeginTransaction()) // 🔥 İşlem sırasında hata olursa geri al
            {
                try
                {
                    var currentExpert = exam.Experts.FirstOrDefault(e => e.Id == model.CurrentExpertId);
                    if (currentExpert != null)
                    {
                        exam.Experts.Remove(currentExpert);

                        var currentSubProfessions = _context.ExamExpertSubProfessions
                            .Where(eesp => eesp.ExamId == model.ExamId && eesp.ExpertId == model.CurrentExpertId)
                            .ToList();

                        if (currentSubProfessions.Any())
                        {
                            _context.ExamExpertSubProfessions.RemoveRange(currentSubProfessions);
                        }
                    }

                    var newExpert = _context.Experts.FirstOrDefault(e => e.Id == model.NewExpertId);
                    if (newExpert != null)
                    {
                        exam.Experts.Add(newExpert);

                        var subProfession = _context.ExamExpertSubProfessions
                            .Where(eesp => eesp.ExamId == model.ExamId && eesp.ExpertId == model.CurrentExpertId)
                            .Select(eesp => new ExamExpertSubProfession
                            {
                                ExamId = model.ExamId,
                                ExpertId = model.NewExpertId,
                                SubProfessionId = eesp.SubProfessionId,
                                FederationId = eesp.FederationId
                            })
                            .ToList();

                        if (subProfession.Any())
                        {
                            _context.ExamExpertSubProfessions.AddRange(subProfession);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); 
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return RedirectToAction("Details", new { id = model.ExamId });
        }

        public async Task<IActionResult> ChangeMonitor(int examId, int monitorId)
        {
            var exam = _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefault(e => e.Id == examId);

            var monitorGender = _context.Monitors.Where(m => m.Id == monitorId).Select(m => m.Gender).FirstOrDefault();
            var sectionId = await GetCurrentSectionIdAsync();
            if (exam == null) return NotFound();

            var selectedMonitorList = exam.Monitors.Select(m => m.Id).ToList();

            var monitorList = _context.Monitors.Where(m => m.Role == 2 && m.SectionId == sectionId && m.Gender == monitorGender && !selectedMonitorList.Contains(m.Id)).ToList(); 

            var viewModel = new ChangeMonitorViewModel
            {
                ExamId = examId,
                CurrentMonitorId = monitorId,
                AvailableMonitors = monitorList.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} {e.Surname} ({e.FinCode})"
                }).ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult ChangeMonitor(ChangeMonitorViewModel model)
        {
            var exam = _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null) return NotFound();

            var currentMonitor = exam.Monitors.FirstOrDefault(e => e.Id == model.CurrentMonitorId);
            if (currentMonitor != null)
            {
                exam.Monitors.Remove(currentMonitor);
            }

            var newMonitor = _context.Monitors.FirstOrDefault(e => e.Id == model.NewMonitorId);
            if (newMonitor != null)
            {
                exam.Monitors.Add(newMonitor);
            }

            _context.SaveChanges();
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> ChangeHeadMonitor(int examId, int monitorId)
        {
            var exam = _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefault(e => e.Id == examId);

            var sectionId = await GetCurrentSectionIdAsync();
            var headMonitorGender = _context.Monitors.Where(m => m.Id == monitorId).Select(m => m.Gender).FirstOrDefault();
            if (exam == null) return NotFound();

            var monitorList = _context.Monitors.Where(m => m.Role == 1 && m.SectionId == sectionId && m.Gender == headMonitorGender).ToList();

            var viewModel = new ChangeMonitorViewModel
            {
                ExamId = examId,
                CurrentMonitorId = monitorId,
                AvailableMonitors = monitorList.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} {e.Surname} ({e.FinCode})"
                }).ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult ChangeHeadMonitor(ChangeMonitorViewModel model)
        {
            var exam = _context.Exams
                .Include(e => e.Monitors)
                .FirstOrDefault(e => e.Id == model.ExamId);

            if (exam == null) return NotFound();

            var currentMonitor = exam.Monitors.FirstOrDefault(e => e.Id == model.CurrentMonitorId);
            if (currentMonitor != null)
            {
                exam.Monitors.Remove(currentMonitor);
            }

            var newMonitor = _context.Monitors.FirstOrDefault(e => e.Id == model.NewMonitorId);
            if (newMonitor != null)
            {
                exam.Monitors.Add(newMonitor);
            }

            _context.SaveChanges();
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public async Task<IActionResult> Create()
        {
            var sectionId = await GetCurrentSectionIdAsync(); 
            var commissions = await _examService.GetCommissionsAsync(sectionId);

            var viewModel = new CreateExamViewModel
            {
                Commissions = commissions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList()
            };
            if (sectionId == null)
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.ToListAsync(), "Id", "Name");
            }
            else
            {
                ViewBag.SectionList = new SelectList(await _context.Sections.Where(s => s.Id == sectionId).ToListAsync(), "Id", "Name");
                ViewBag.ExamBuildingList = new SelectList(await _context.ExamBuildings.Where(e => e.SectionId == sectionId).ToListAsync(), "Id", "Name");

            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateExamViewModel exam)
        {
            if (!ModelState.IsValid)
            {
                var sectionId = await GetCurrentSectionIdAsync();
                var commissions = await _examService.GetCommissionsAsync(sectionId);

                var viewModel = new CreateExamViewModel
                {
                    Commissions = commissions.Select(sp => new SelectListItem
                    {
                        Text = sp.Name,
                        Value = sp.Id.ToString()
                    }).ToList()
                };
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

            // Fetch the exam entity
            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }
            if (!await IsSectionValidAsync<Exam>(id))
            {
                return Forbid();
            }

            // Convert Exam model to EditExamViewModel
            var viewModel = new EditExamViewModel
            {
                Id = exam.Id,
                Name = exam.Name,
                SectionId = exam.SectionId,
                ExamBuldingId = exam.ExamBuldingId,
                ExamDate = exam.ExamDate,
                Duration = exam.Duration,
                Water = exam.Water,
                Food = exam.Food,
                Notes = exam.Notes,
                InventoryTransport = exam.InventoryTransport,
                Shift = exam.Shift,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                SelectedCommissions = exam.ExamCommissions?.Select(ec => ec.CommissionId).ToArray(), // CommissionId'leri al
                Commissions = (await _examService.GetCommissionsAsync(sectionId))
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToList()
            };

            return View(viewModel); // Pass the correct view model
        }


        [HttpPost]
        public async Task<IActionResult> Edit(EditExamViewModel exam, int[] selectedCommissions)
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
                await _examService.UpdateExamAsync(exam,selectedCommissions);
                return RedirectToAction(nameof(Index));
            }
            
            return View(exam);
        }

        private async Task PopulateDropdowns(int? sectionId)
        {
            ViewBag.CommissionList = new MultiSelectList(
                await _context.Commissions
                    .Where(c => c.SectionId == sectionId)
                    .ToListAsync(),
                "Id",
                "Name"
            );

            ViewBag.SubCommissionList = new MultiSelectList(
                await _context.SubCommissions
                    .Where(sc => sc.SectionId == sectionId)
                    .ToListAsync(),
                "Id",
                "Name"
            );
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

        [HttpPost]
        public async Task<IActionResult> UpdateExperts(int ExamId, int[] SelectedExpertIds)
        {
            var exam = await _context.Exams
                .Include(e => e.Experts)
                .FirstOrDefaultAsync(e => e.Id == ExamId);

            if (exam == null)
            {
                return NotFound();
            }

            // Mövcud ekspertləri yeniləyək
            var selectedExperts = await _context.Experts
                .Where(e => SelectedExpertIds.Contains(e.Id))
                .ToListAsync();

            exam.Experts = selectedExperts;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ekspertlər uğurla yeniləndi!";
            return RedirectToAction(nameof(Details), new { id = ExamId });
        }
        public async Task<IActionResult> AssignExperts(int id)
        {
            int sectionId = _context.Exams
                                     .Where(e => e.Id == id)
                                     .Select(i => i.SectionId)
                                     .FirstOrDefault();
            var availableSubProfessions = await _examService.GetSubprofessionsBySectionIdAsync(sectionId);
            var federations = await _context.Professions
                                             .Where(f => f.SectionId == sectionId)
                                             .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                                             .ToListAsync();

            var exam = await _examService.GetExamByIdAsync(id);
            if (exam == null)
            {
                return NotFound();
            }

            var viewModel = new AssignExpertToExamViewModel
            {
                ExamId = exam.Id,
                SectionId = exam.SectionId,
                SubProfessions = availableSubProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList(),
                Federations = federations // Yeni eklendi
            };

            ViewBag.ExamName = exam.Name;
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AssignExperts(AssignExpertToExamViewModel model)
        {
            try
            {
                var federationId = model.Assignments[0].FederationId;
                Console.WriteLine($"FederationId received: {federationId}");
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
            var sectionId = await _examService.GetSectionIdByExamIdAsync(model.ExamId);
            model.SubProfessions = (await _examService.GetSubprofessionsBySectionIdAsync(sectionId))
                .Select(sp => new SelectListItem { Value = sp.Id.ToString(), Text = sp.Name })
                .ToList();
            model.Federations = await _context.Professions
                .Where(f => f.SectionId == sectionId)
                .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                .ToListAsync();

            return View(model);
        }

        public IActionResult WriteMonitorLog(int examId, int monitorId, byte kind)
        {
            var viewModel = new WriteMonitorLogViewModel
            {
                ExamId = examId,
                MonitorId = monitorId,
                Kind = 0, 
                KindOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Gəlməyən haqqında qeyd" },
                    new SelectListItem { Value = "1", Text = "Xaric olan haqqında qeyd" },
                    new SelectListItem { Value = "2", Text = "Digər səbəblərlə bağlı qeyd" }
                }
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WriteMonitorLog(WriteMonitorLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hata durumunda KindOptions listesini yeniden eklemeyi unutmayın.
                model.KindOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Gəlməyən haqqında qeyd" },
                    new SelectListItem { Value = "1", Text = "Xaric olan haqqında qeyd" },
                    new SelectListItem { Value = "2", Text = "Digər səbəblərlə bağlı qeyd" }
                };
                return View(model);
            }

            await _examService.AddMonitorLogAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
        }
        public IActionResult WriteExpertLog(int examId, int expertId, byte kind)
        {
            var viewModel = new WriteExpertLogsViewModel
            {
                ExamId = examId,
                ExpertId = expertId,
                Kind = 0,
                KindOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Gəlməyən haqqında qeyd" },
                    new SelectListItem { Value = "1", Text = "Xaric olan haqqında qeyd" },
                    new SelectListItem { Value = "2", Text = "Digər səbəblərlə bağlı qeyd" }
                }
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WriteExpertLog(WriteExpertLogsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hata durumunda KindOptions listesini yeniden eklemeyi unutmayın.
                model.KindOptions = new List<SelectListItem>
                {
                    new SelectListItem { Value = "0", Text = "Gəlməyən haqqında qeyd" },
                    new SelectListItem { Value = "1", Text = "Xaric olan haqqında qeyd" },
                    new SelectListItem { Value = "2", Text = "Digər səbəblərlə bağlı qeyd" }
                };
                return View(model);
            }

            await _examService.AddExpertLogAsync(model);
            return RedirectToAction("Details", new { id = model.ExamId });
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
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var assignment in model.Assignments)
                    {
                        await _examService.AssignRandomMonitorsToExamAsync(
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
                    ViewBag.ErrorMessage = ex.Message; // Alternatif olarak
                }
            }

            var exam = await _examService.GetExamByIdAsync(model.ExamId);
            if (exam != null)
            {
                ViewBag.ExamName = exam.Name;
            }

            return View(model);
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
                    //new DataColumn("Commission"),
                    new DataColumn("Exam Building"),
                    new DataColumn("Section"),
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
                    //exam.Commission?.Name ?? "---",
                    exam.ExamBuilding?.Name ?? "---",
                    exam.Section?.Name ?? "---"
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

        [HttpGet]
        public async Task<IActionResult> GetSubProfessionsByFederation(int federationId)
        {
            var subProfessions = await _context.SubProfessions
                .Where(sp => sp.ProfessionId == federationId)
                .Select(sp => new { sp.Id, sp.Name })
                .ToListAsync();

            return Json(subProfessions);
        }

    }
}