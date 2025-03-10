using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Models.ViewModels;
using ForQab.Presentation.ViewModels;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ForQab.Service
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly IExpertRepository _expertRepository;
        private readonly IExamExpertSubProfessionRepository _examExpertSubProfessionRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IMonitorRepository _monitorRepository;
        private readonly IRepresentativeRepository _representativeRepository;
        private readonly MyDbContext _context;

        public ExamService(IExamRepository examRepository, IExpertRepository expertRepository, ISectionRepository sectionRepository, IExamExpertSubProfessionRepository examExpertSubProfessionRepository, IMonitorRepository monitorRepository, MyDbContext context, IRepresentativeRepository representativeRepository)
        {
            _examRepository = examRepository;
            _expertRepository = expertRepository;
            _sectionRepository = sectionRepository;
            _examExpertSubProfessionRepository = examExpertSubProfessionRepository;
            _monitorRepository = monitorRepository;
            _context = context;
            _representativeRepository = representativeRepository;
        }
        public async Task<CreateExamViewModel> PrepareCreateExamViewModelAsync(int? sectionId)
        {
            var commissions = await _examRepository.GetCommissionsAsync(sectionId);
            var degrees = await _context.Degrees.ToListAsync();

            return new CreateExamViewModel
            {
                Commissions = commissions.Select(sp => new SelectListItem { Text = sp.Name, Value = sp.Id.ToString() }).ToList(),
                Degrees = degrees.Select(d => new SelectListItem { Text = d.Name, Value = d.Id.ToString() }).ToList()
            };
        }
        public async Task<EditExamViewModel> PrepareEditExamViewModelAsync(int id, int? sectionId)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null) return null;

            var commissions = await _examRepository.GetCommissionsAsync(sectionId);
            var degrees = await _context.Degrees.ToListAsync();

            return new EditExamViewModel
            {
                Id = exam.Id,
                Name = exam.Name,
                SectionId = exam.SectionId,
                DistrictId = exam.DistrictId,
                ExamBuldingId = exam.ExamBuldingId,
                ExamDate = exam.ExamDate,
                Duration = exam.Duration,
                Water = exam.Water,
                Food = exam.Food,
                StudentCount = exam.StudentCount,
                Notes = exam.Notes,
                InventoryTransport = exam.InventoryTransport,
                Shift = exam.Shift,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                AdmissionTime = exam.AdmissionTime,
                SelectedCommissions = exam.ExamCommissions?.Select(ec => ec.CommissionId).ToArray(),
                Commissions = commissions.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                SelectedDegrees = exam.ExamDegrees?.Select(ed => ed.DegreeId).ToArray(),
                Degrees = degrees.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToList()
            };
        }

        public async Task PopulateViewBagsAsync(int? sectionId, dynamic viewBag)
        {
            var sections = sectionId == null ? _context.Sections : _context.Sections.Where(s => s.Id == sectionId);
            var examBuildings = sectionId == null ? _context.ExamBuildings : _context.ExamBuildings.Where(e => e.SectionId == sectionId);
            var districts = _context.Districts ;
            var commissions = sectionId == null ? _context.Commissions : _context.Commissions.Where(c => c.SectionId == sectionId);
            var degrees = _context.Degrees;
            var subCommissions = sectionId == null ? _context.SubCommissions : _context.SubCommissions.Where(sc => sc.SectionId == sectionId);

            viewBag.SectionList = new SelectList(await sections.ToListAsync(), "Id", "Name");
            viewBag.ExamBuildingList = new SelectList(await examBuildings.ToListAsync(), "Id", "Name");
            viewBag.DistrictList = new SelectList(await districts.ToListAsync(), "Id", "Name");
            viewBag.CommissionList = new SelectList(await commissions.ToListAsync(), "Id", "Name");
            viewBag.DegreeList = new SelectList(await degrees.ToListAsync(), "Id", "Name");
            viewBag.SubCommissionList = new SelectList(await subCommissions.ToListAsync(), "Id", "Name");
        }
        public async Task<ChangeMonitorViewModel> GetChangeMonitorViewModelAsync(int examId, int monitorId, int role)
        {
            var exam = await _examRepository.GetExamWithMonitorsAsync(examId);
            if (exam == null) return null;

            var monitorAttribute = await _monitorRepository.GetMonitorAttributeByIdAsync(monitorId, role);
            var sectionId = exam.SectionId;

            var selectedMonitorList = exam.Monitors.Select(m => m.Id).ToList();
            var availableMonitors = role == 5
                                    ? await _monitorRepository.GetAvailableWorkersAsync(sectionId, role, (int)monitorAttribute, selectedMonitorList)
                                    : await _monitorRepository.GetAvailableMonitorsAsync(sectionId, role, (int)monitorAttribute, selectedMonitorList);


            return new ChangeMonitorViewModel
            {
                ExamId = examId,
                CurrentMonitorId = monitorId,
                AvailableMonitors = availableMonitors.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Name} {m.Surname} ({m.FinCode})"
                }).ToList()
            };
        }
        public async Task<ChangeRepresentativeViewModel> GetChangeRepresentativeViewModelAsync(int examId, int representativeId)
        {
            var exam = await _examRepository.GetExamWithRepresentativeAsync(examId);
            if (exam == null) return null;

            var selectedRepresentativeList = exam.Representatives.Select(m => m.Id).ToList();
            var availableRepresentatives =await _monitorRepository.GetAvailableRepresentativesAsync(selectedRepresentativeList);

            return new ChangeRepresentativeViewModel
            {
                ExamId = examId,
                CurrentRepresentativeId = representativeId,
                AvailableRepresentatives = availableRepresentatives.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.Name} {m.Surname} ({m.FinCode})"
                }).ToList()
            };
        }
        //public async Task<bool> UpdateExpertsAsync(int examId, int[] selectedExpertIds)
        //{
        //    var exam = await _examRepository.GetExamWithExpertsByIdAsync(examId);
        //    if (exam == null) return false;

        //    var selectedExperts = await _expertRepository.GetExpertsByIdsAsync(selectedExpertIds);
        //    exam.Experts = selectedExperts;

        //    await _examRepository.SaveChangesAsync();
        //    return true;
        //}
        public async Task<AssignExpertToExamViewModel> PrepareAssignExpertsViewModelAsync(Exam exam)
        {
            var availableSubProfessions = await GetSubprofessionsBySectionIdAsync(exam.SectionId);
            var federations = await _context.Professions
                                             .Where(f => f.SectionId == exam.SectionId)
                                             .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                                             .ToListAsync();

            return new AssignExpertToExamViewModel
            {
                ExamId = exam.Id,
                SectionId = exam.SectionId,
                SubProfessions = availableSubProfessions.Select(sp => new SelectListItem
                {
                    Text = sp.Name,
                    Value = sp.Id.ToString()
                }).ToList(),
                Federations = federations
            };
        }


        public async Task<bool> ChangeMonitorAsync(ChangeMonitorViewModel model)
        {
            var exam = await _examRepository.GetExamWithMonitorsAsync(model.ExamId);
            if (exam == null) return false;

            var currentMonitor = exam.Monitors.FirstOrDefault(m => m.Id == model.CurrentMonitorId);
            if (currentMonitor != null)
            {
                exam.Monitors.Remove(currentMonitor);
            }

            var newMonitor = await _monitorRepository.GetMonitorByIdAsync(model.NewMonitorId);
            if (newMonitor != null)
            {
                exam.Monitors.Add(newMonitor);
            }

            await _examRepository.SaveAsync();
            return true;
        }
        public async Task<bool> ChangeRepresentativeAsync(ChangeRepresentativeViewModel model)
        {
            var exam = await _examRepository.GetExamWithRepresentativeAsync(model.ExamId);
            if (exam == null) return false;

            var currentRepresentative = exam.Representatives.FirstOrDefault(m => m.Id == model.CurrentRepresentativeId);
            if (currentRepresentative != null)
            {
                exam.Representatives.Remove(currentRepresentative);
            }

            var newRepresentative = await _representativeRepository.GetByIdAsync(model.NewRepresentativeId);
            if (newRepresentative != null)
            {
                exam.Representatives.Add(newRepresentative);
            }

            await _examRepository.SaveAsync();
            return true;
        }
        public async Task<ChangeExpertViewModel> GetChangeExpertViewModelAsync(int examId, int expertId)
        {
            var exam = await _examRepository.GetExamWithExpertsAndSubProfessionsAsync(examId);
            if (exam == null) return null;
            var sectionId = exam.SectionId;

            var subProfession = await _examExpertSubProfessionRepository.GetSubProfessionIdByExpertAsync(examId, expertId);
            if (subProfession == null) return null;


            var selectedExpertList = exam.Experts.Select(e => e.Id).ToList();

            var expertList = await _expertRepository.GetExpertsBySectionAndSubProfessionAsync(sectionId, (int)subProfession, selectedExpertList);

            return new ChangeExpertViewModel
            {
                ExamId = examId,
                CurrentExpertId = expertId,
                AvailableExperts = expertList.Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = $"{e.Name} {e.Surname} ({e.FinCode})"
                }).ToList()
            };
        }

        public async Task<bool> ChangeExpertAsync(int examId, int currentExpertId, int newExpertId)
        {
            var exam = await _examRepository.GetExamWithExpertsAndSubProfessionsAsync(examId);
            if (exam == null) return false;

            using (var transaction = await _examRepository.BeginTransactionAsync())
            {
                try
                {
                    var currentExpert = exam.Experts.FirstOrDefault(e => e.Id == currentExpertId);
                    if (currentExpert != null)
                    {
                        exam.Experts.Remove(currentExpert);

                        //var currentSubProfessions = await _examExpertSubProfessionRepository.GetSubProfessionsByExpertAsync(examId, currentExpertId);
                        //if (currentSubProfessions.Any())
                        //{
                        //    await _examExpertSubProfessionRepository.RemoveSubProfessionsAsync(currentSubProfessions);
                        //}
                    }

                    var newExpert = await _expertRepository.GetByIdAsync(newExpertId);
                    if (newExpert != null)
                    {
                        exam.Experts.Add(newExpert);

                        var subProfession = await _examExpertSubProfessionRepository.GetSubProfessionsByExpertAsync(examId, currentExpertId);
                        if (subProfession.Any())
                        {
                            await _examExpertSubProfessionRepository.AddSubProfessionsAsync(subProfession.Select(sp => new ExamExpertSubProfession
                            {
                                ExamId = examId,
                                ExpertId = newExpertId,
                                SubProfessionId = sp.SubProfessionId,
                                FederationId = sp.FederationId
                            }).ToList());
                        }
                    }

                    await _examRepository.SaveAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
            public async Task AddExamAsync(CreateExamViewModel exam)
        {
            await _examRepository.AddAsync(exam);
        }

        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId)
        {
            await _examRepository.AssignRandomExpertsToExamAsync(examId, numberOfExperts,  selectedSubProfessions, federationId);
        }
        public async Task<bool> AssignExpertsAsync(AssignExpertToExamViewModel model)
        {
            var sectionId = await _examRepository.GetSectionIdByExamIdAsync(model.ExamId);
            if (sectionId == null)
                throw new Exception("İmtahan tapılmadı");

            foreach (var assignment in model.Assignments)
            {
                if (assignment.SelectedSubProfessions == null || !assignment.SelectedSubProfessions.Any())
                    throw new Exception("Alt ixtisas sahəsi seçilməyib!");

                var availableExpertsCount = await _examRepository.GetAvailableExpertsCountAsync(
                    sectionId.Value, assignment.SelectedSubProfessions);

                if (assignment.NumberOfExperts > availableExpertsCount)
                    throw new Exception(
                        $"{assignment.NumberOfExperts} sayda ekspert təyin etmək istədiniz, " +
                        $"lakin mövcud ekspert sayı {availableExpertsCount}-dır!"
                    );
            }

            foreach (var assignment in model.Assignments)
            {
                await _examRepository.AssignRandomExpertsToExamAsync(
                    model.ExamId, assignment.NumberOfExperts, assignment.SelectedSubProfessions, assignment.FederationId);
            }

            return true;
        }
        public async Task AddMonitorLogAsync(WriteMonitorLogViewModel model)
        {
            MonitorLog log = new MonitorLog
            {
                SupervisorId = model.MonitorId,
                Note = model.Note,
                Kind = model.Kind
            };

            await _examRepository.AddMonitorLogAsync(log);
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate)
        {
            await _examRepository.AssignRandomMonitorsToExamAsync(examId, numberOfMonitors, genderId, maxDate);
        }

        public async Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate)
        {
            await _examRepository.AssignRandomHeadMonitorsToExamAsync(examId, numberOfMonitors, genderId, maxDate);
        }
        public async Task DeleteExamAsync(int id)
        {
            await _examRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Exam>> GetAllExamsAsync()
        {
            return await _examRepository.GetAllAsync();
        }

        public async Task<Exam?> GetExamByIdAsync(int id)
        {
            return await _examRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId)
        {
            return await _examRepository.GetExamsBySectionIdAsync(sectionId);
        }

        public async Task<IEnumerable<SubProfession>> GetSubprofessionsBySectionIdAsync(int? sectionId)
        {
            return await _examRepository.GetSubProfessionsBySectionIdAsync(sectionId);
        }

        public async Task UpdateExamAsync(Exam exam)
        {
            await _examRepository.UpdateAsync(exam);
        }

        public async Task<int?> GetSectionIdByExamIdAsync(int examId)
        {
           return await _examRepository.GetSectionIdByExamIdAsync(examId);
        }

        public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds)
        {
            await _examRepository.UpdateExamAsync(exam, commissionIds,degreeIds);
        }

        public async Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId)
        {
           return await _examRepository.GetCommissionsAsync(sectionId);
        }

        public async Task AddExpertLogAsync(WriteExpertLogsViewModel model)
        {
            ExpertLog log = new ExpertLog
            {
                ExpertId = model.ExpertId,
                Note = model.Note,
                Kind = model.Kind
            };

            await _examRepository.AddExpertLogAsync(log);
        }

        public async Task<List<int>> GetMonitorsWithLogsAsync(List<int> monitorIds)
        {
            return await _examRepository.GetMonitorsWithLogsAsync(monitorIds);
        }
        public async Task<List<int>> GetExpertsWithLogsAsync(List<int> expertIds)
        {
            return await _examRepository.GetExpertsWithLogsAsync(expertIds);
        }
        public  List<Expert> GetExpertsByExam(int examId)
        {
            return  _examRepository.GetExpertsByExam(examId);
        }
        public async Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId)
        {
            return await _examRepository.GetExpertSubProfessionsByExamIdAsync(examId);
        }

        public async Task AssignWorkersToExamAsync(int examId, List<int> selectedWorkerIds)
        {
            await _examRepository.AssignWorkersToExamAsync(examId, selectedWorkerIds);
        }

        public Task<MemoryStream> ExportExamScheduleToWord()
        {
            return _examRepository.ExportExamScheduleToWord();
        }

        public Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            return _examRepository.AssignRepresentativesToExamAsync(examId, selectedRepresentativeIds);
        }

        public Task<List<DimRepresentative>> GetAvailableRepresentativesAsync()
        {
            return _examRepository.GetAvailableRepresentativesAsync();
        }

        public Task<List<DataAccess.Models.Monitor>> GetAvailableWorkersAsync(int buildingId)
        {
            return _examRepository.GetAvailableWorkersAsync(buildingId);
        }
        public async Task<ExamDetailsViewModel> GetExamDetailsAsync(int examId)
        {
            var exam = await _examRepository.GetByIdAsync(examId);

            if (exam == null)
            {
                return null;
            }

            // Fetch the monitors and experts with logs sequentially
            var monitorIds = exam.Monitors.Select(m => m.Id).ToList();
            var expertIds = exam.Experts.Select(m => m.Id).ToList();

            // Fetch logs for monitors
            var monitorLogs = await _examRepository.GetMonitorsWithLogsAsync(monitorIds);

            // Fetch logs for experts
            var expertLogs = await _examRepository.GetExpertsWithLogsAsync(expertIds);
            Console.WriteLine($"Monitor Logs Count: {monitorLogs.Count}");
            Console.WriteLine($"Expert Logs Count: {expertLogs.Count}");
            var viewModel = new ExamDetailsViewModel
            {
                Id = exam.Id,
                Name = exam.Name,
                Section = new SectionViewModel { Name = exam.Section.Name },
                District = new DistrictViewModel { Name = exam.District.Name },
                ExamBulding = new BuildingViewModel { Name = exam.ExamBuilding.Name },
                ExamDate = exam.ExamDate,
                Duration = exam.Duration,
                Water = exam.Water,
                Food = exam.Food,
                StudentCount = exam.StudentCount,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                AdmissionTime = exam.AdmissionTime,
                Notes = exam.Notes ?? string.Empty,
                InventoryTransport = exam.InventoryTransport ?? string.Empty,
                ExamCommissions = exam.ExamCommissions.Select(ec => new ExamCommissionViewModel
                {
                    Commission = new CommissionViewModel { Name = ec.Commission.Name }
                }).ToList(),
                ExamDegrees = exam.ExamDegrees.Select(ec => new ExamDegreeViewModel
                {
                    Degree = new DegreeViewModel { Name = ec.Degrees.Name }
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
                ExamRepresentatives = exam.Representatives.Select(er => new RepresentativeViewModel
                {
                    Id = er.Id,
                    Name = er.Name,
                    Surname = er.Surname,
                    Fname = er.Fname,
                    FinCode = er.FinCode,
                }).ToList(),
                ExpertsWithLogs = expertLogs ?? new List<int>(),
                MonitorsWithLogs = monitorLogs ?? new List<int>(),
            };

            return viewModel;
        }

        public Task<byte[]> ExportExamMonitorsToWordAsync(int examId)
        {
            return _examRepository.ExportExamMonitorsToWordAsync(examId);
        }
    }
}