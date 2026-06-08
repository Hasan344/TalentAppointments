using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Models.ViewModels;
using ForQab.Presentation.ViewModels;
using ForQab.Repository.Abstract;
using ForQab.Service.Abstract;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using ClosedXML.Excel;
using Monitor = ForQab.DataAccess.Models.Monitor;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Justification = DocumentFormat.OpenXml.Wordprocessing.Justification;
using DocumentFormat.OpenXml.Bibliography;
using ForQab.Extensions;

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
        private readonly IMinistryRepresentativeRepository _ministryRepresentativeRepository;
        private readonly IExamMonitorRepository _examMonitorRepository;
        private readonly MyDbContext _context;

        public ExamService(IExamRepository examRepository, IExpertRepository expertRepository, ISectionRepository sectionRepository, IExamExpertSubProfessionRepository examExpertSubProfessionRepository, IMonitorRepository monitorRepository, MyDbContext context, IRepresentativeRepository representativeRepository, IExamMonitorRepository examMonitorRepository, IMinistryRepresentativeRepository ministryRepresentativeRepository)
        {
            _examRepository = examRepository;
            _expertRepository = expertRepository;
            _sectionRepository = sectionRepository;
            _examExpertSubProfessionRepository = examExpertSubProfessionRepository;
            _monitorRepository = monitorRepository;
            _context = context;
            _representativeRepository = representativeRepository;
            _examMonitorRepository = examMonitorRepository;
            _ministryRepresentativeRepository = ministryRepresentativeRepository;
        }
        public async Task<CreateExamViewModel> PrepareCreateExamViewModelAsync(int? sectionId)
        {
            var commissions = await _examRepository.GetCommissionsAsync(sectionId);
            var degrees = await _context.Degrees.ToListAsync();
            var subjects = await _context.Subjects.Where(s => s.SectionId == sectionId).ToListAsync();

            return new CreateExamViewModel
            {
                Commissions = commissions.Select(sp => new SelectListItem { Text = sp.Name, Value = sp.Id.ToString() }).ToList(),
                Degrees = degrees.Select(d => new SelectListItem { Text = d.Name, Value = d.Id.ToString() }).ToList(),
                Subjects = subjects.Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList()
            };
        }
        //public async Task<CreateExamViewModelForAssesment> PrepareCreateExamViewModelAsyncForAssesment(int? sectionId)
        //{
        //    var commissions = await _examRepository.GetCommissionsAsync(sectionId);
        //    var degrees = await _context.Degrees.ToListAsync();

        //    return new CreateExamViewModelForAssesment
        //    {
        //        Commissions = commissions.Select(sp => new SelectListItem { Text = sp.Name, Value = sp.Id.ToString() }).ToList(),
        //        Degrees = degrees.Select(d => new SelectListItem { Text = d.Name, Value = d.Id.ToString() }).ToList()
        //    };
        //}
        public async Task<EditExamViewModel> PrepareEditExamViewModelAsync(int id, int? sectionId)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null) return null;

            var commissions = await _examRepository.GetCommissionsAsync(sectionId);
            var degrees = await _context.Degrees.ToListAsync();
            var subjects = await _context.Subjects.Where(s => s.SectionId == sectionId).ToListAsync();

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
                burQ = exam.burQ,
                burK = exam.burK,
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
                Degrees = degrees.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToList(),
                SelectedSubjects = exam.ExamSubjects?.Select(ed => ed.SubjectId).ToArray(),
                Subjects = subjects.Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name }).ToList()
            };
        }
        public async Task<EditExamViewModelForAssesment> PrepareEditExamViewModelAsyncForAssesment(int id, int? sectionId)
        {
            var exam = await _examRepository.GetByIdAsync(id);
            if (exam == null) return null;

            return new EditExamViewModelForAssesment
            {
                Id = exam.Id,
                Name = exam.Name,
                SectionId = exam.SectionId,
                DistrictId = exam.DistrictId,
                ExamBuldingId = exam.ExamBuldingId,
                ExamDate = exam.ExamDate,
                Shift = exam.Shift,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
            };
        }

        public async Task PopulateViewBagsAsync(int? sectionId, dynamic viewBag)
        {
            var sections = sectionId == null ? _context.Sections : _context.Sections.Where(s => s.Id == sectionId);
            var examBuildings = sectionId == null ? _context.ExamBuildings : _context.ExamBuildings.Where(e => e.SectionId == sectionId);
            var districts = _context.Districts;
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
            var monitor = await _monitorRepository.GetByIdAsync(monitorId);

            var monitorAttribute = await _monitorRepository.GetMonitorAttributeByIdAsync(monitorId, role);
            var sectionId = exam.SectionId;

            var selectedMonitorList = exam.Monitors.Select(m => m.Id).ToList();
            var availableMonitors = role == 5
                                    ? await _monitorRepository.GetAvailableWorkersAsync(sectionId, role, (int)monitorAttribute, selectedMonitorList)
                                    : await _monitorRepository.GetAvailableMonitorsAsync(sectionId, role, (int)monitorAttribute, selectedMonitorList, monitor.District);


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
            var availableRepresentatives = await _representativeRepository.GetAvailableRepresentativeAsync(selectedRepresentativeList);

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
        public async Task<ChangeRepresentativeViewModel> GetChangeMinistryRepresentativeViewModelAsync(int examId, int representativeId)
        {
            var exam = await _examRepository.GetExamWithRepresentativeAsync(examId);
            if (exam == null) return null;

            var selectedRepresentativeList = exam.Representatives.Select(m => m.Id).ToList();
            var availableRepresentatives = await _ministryRepresentativeRepository.GetAvailableRepresentativeAsync(selectedRepresentativeList);

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
        public async Task<AssignExpertToExamViewModel> PrepareAssignExpertsViewModelAsync(Exam exam)
        {
            var availableSubProfessions = await GetSubprofessionsBySectionIdAsync(exam.SectionId);
            var federations = await _context.Professions
                                             .Where(f => f.SectionId == exam.SectionId)
                                             .Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name })
                                             .ToListAsync();
            var rooms = await _context.ExamRooms
                                     .Where(r => r.SectionId == exam.SectionId)
                                     .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
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
                Federations = federations,
                Rooms = rooms
            };
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
        public async Task<bool> ChangeMinistryRepresentativeAsync(ChangeRepresentativeViewModel model)
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
        public async Task<bool> ChangeMonitorAsync(ChangeMonitorViewModel model)
        {
            var exam = await _examRepository.GetExamWithMonitorsAsync(model.ExamId);
            if (exam == null) return false;

            var currentExamMonitor = await _examMonitorRepository.GetByExamAndMonitorAsync(model.ExamId, model.CurrentMonitorId);
            int? roomId = currentExamMonitor?.RoomId;

            if (currentExamMonitor != null)
            {
                _examMonitorRepository.Remove(currentExamMonitor);
            }

            var currentMonitor = await _monitorRepository.GetByIdAsync(model.CurrentMonitorId);
            var newMonitor = await _monitorRepository.GetByIdAsync(model.NewMonitorId);

            var newExamMonitor = new ExamMonitor
            {
                ExamId = model.ExamId,
                MonitorId = model.NewMonitorId,
                RoomId = roomId
            };

            await _examMonitorRepository.AddAsync(newExamMonitor);
            await _examRepository.SaveAsync();
            return true;
        }
        public async Task<bool> ChangeExpertAsync(int examId, int currentExpertId)
        {
            var exam = await _examRepository.GetExamWithExpertsAndSubProfessionsAsync(examId);
            if (exam == null) return false;

            using (var transaction = await _examRepository.BeginTransactionAsync())
            {
                try
                {
                    var currentExamExpertSubProfessions = await _examExpertSubProfessionRepository
                        .GetSubProfessionsByExpertAsync(examId, currentExpertId);

                    var subProfessions = currentExamExpertSubProfessions.ToList();
                    if (!subProfessions.Any()) return false;

                    var currentExpert = await _expertRepository.GetByIdAsync(currentExpertId);
                    if (currentExpert == null) return false;

                    var subprofessionId = subProfessions.FirstOrDefault()?.SubProfessionId ?? 0;

                    var newExpert = await _expertRepository
                        .FindSuitableExpertAsync(subprofessionId, currentExpert.Federation, currentExpertId, examId, exam.ExamDate);
                    if (newExpert == null) return false;

                    var isNewExpertAssigned = await _examExpertSubProfessionRepository
                        .IsExpertAssignedToExamAsync(examId, newExpert.Id);
                    if (isNewExpertAssigned)
                    {
                        return false;
                    }

                    var newExamExpertSubProfessions = subProfessions.Select(sp => new ExamExpertSubProfession
                    {
                        ExamId = examId,
                        ExpertId = newExpert.Id,
                        SubProfessionId = sp.SubProfessionId,
                        FederationId = sp.FederationId,
                        RoomId = sp.RoomId
                    }).ToList();

                    exam.Experts.Remove(currentExpert);
                    await _examExpertSubProfessionRepository.RemoveByExpertAsync(examId, currentExpertId);

                    exam.Experts.Add(newExpert);
                    await _examExpertSubProfessionRepository.AddSubProfessionsAsync(newExamExpertSubProfessions);


                    await _examRepository.SaveAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Hata: {ex.Message}");
                    throw;
                }
            }
        }
        //public async Task<bool> ChangeExpertAsync(int examId, int currentExpertId)
        //{
        //    var exam = await _examRepository.GetExamWithExpertsAndSubProfessionsAsync(examId);
        //    if (exam == null) return false;

        //    using (var transaction = await _examRepository.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            var currentExamExpertSubProfessions = await _examExpertSubProfessionRepository
        //                .GetSubProfessionsByExpertAsync(examId, currentExpertId);

        //            var subProfessions = currentExamExpertSubProfessions.ToList();
        //            if (!subProfessions.Any()) return false;

        //            var currentExpert = await _expertRepository.GetByIdAsync(currentExpertId);
        //            if (currentExpert == null) return false;

        //            var subprofessionId = subProfessions.FirstOrDefault()?.SubProfessionId ?? 0;
        //            var newExpert = await _expertRepository
        //                .FindSuitableExpertAsync(subprofessionId, currentExpert.Federation, currentExpertId);
        //            if (newExpert == null) return false;

        //            var newExamExpertSubProfessions = subProfessions.Select(sp => new ExamExpertSubProfession
        //            {
        //                ExamId = examId,
        //                ExpertId = newExpert.Id,
        //                SubProfessionId = sp.SubProfessionId,
        //                FederationId = sp.FederationId,
        //                RoomId = sp.RoomId
        //            }).ToList();

        //            exam.Experts.Remove(currentExpert);

        //            exam.Experts.Add(newExpert);

        //            // Mevcut uzmanı kaldır
        //            await _examExpertSubProfessionRepository.RemoveByExpertAsync(examId, currentExpertId);
        //            await _examExpertSubProfessionRepository.AddSubProfessionsAsync(newExamExpertSubProfessions);

        //            _context.SaveChanges();
        //            // Atama sayısını güncelle
        //            currentExpert.AssignmentCount--;
        //            newExpert.AssignmentCount++;

        //            await _examRepository.SaveAsync();
        //            await transaction.CommitAsync();
        //            return true;
        //        }
        //        catch
        //        {
        //            await transaction.RollbackAsync();
        //            throw;
        //        }
        //    }
        //}


        public async Task AddExamAsync(CreateExamViewModel exam)
        {
            await _examRepository.AddAsync(exam);
        }

        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId, int? roomId)
        {
            await _examRepository.AssignRandomExpertsToExamAsync(examId, numberOfExperts, selectedSubProfessions, federationId, roomId);
        }
        public async Task<bool> AssignExpertsAsync(AssignExpertToExamViewModel model)
        {
            var sectionId = await _examRepository.GetSectionIdByExamIdAsync(model.ExamId);
            if (sectionId == null)
                throw new Exception("İmtahan tapılmadı");

            foreach (var assignment in model.Assignments)
            {
                if (assignment.SelectedSubProfessions == null || !assignment.SelectedSubProfessions.Any())
                    throw new Exception("İxtisas seçimi doğru deyil!");

                var availableExpertsCount = await _examRepository.GetAvailableExpertsCountAsync(
                    sectionId.Value, assignment.SelectedSubProfessions);

                //if (assignment.NumberOfExperts > availableExpertsCount)
                //    throw new Exception(
                //        $"{assignment.NumberOfExperts} sayda ekspert təyin etmək istədiniz, " +
                //        $"lakin mövcud ekspert sayı {availableExpertsCount}-dır!"
                //    );
            }

            foreach (var assignment in model.Assignments)
            {
                await _examRepository.AssignRandomExpertsToExamAsync(
                    model.ExamId, assignment.NumberOfExperts, assignment.SelectedSubProfessions, assignment.FederationId, assignment.RoomId);
            }

            return true;
        }
        public async Task AddMonitorLogAsync(WriteMonitorLogViewModel model)
        {
            MonitorLog log = new MonitorLog
            {
                SupervisorId = model.MonitorId,
                Note = model.Note,
                Kind = model.Kind,
                ExamId = model.ExamId,
                UserName = model.UserName,
                Time = DateTime.Now,
            };
            await _examRepository.AddMonitorLogAsync(log);
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate, int? roomId)
        {
            await _examRepository.AssignRandomMonitorsToExamAsync(examId, numberOfMonitors, genderId, maxDate, roomId);
        }
        public async Task AssignExpertsForMXToExamAsync(AssignExpertForMXToExamViewModel viewModel)
        {
            await _examRepository.AssignExpertsForMXToExamAsync(viewModel);
        }
        public async Task AssignMonitorsForMXToExamAsync(AssignMonitorForMXToExamViewModel viewModel)
        {
            await _examRepository.AssignMonitorsForMXToExamAsync(viewModel);
        }
        public async Task AssignWorkersForMXToExamAsync(AssignWorkerForMXToExamViewModel viewModel)
        {
            await _examRepository.AssignWorkersForMXToExamAsync(viewModel);
        }
        public async Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate)
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

        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId, int? examBuildingId, int? year)
        {
            return await _examRepository.GetExamsBySectionIdAsync(sectionId, 1, year, examBuildingId);
        }

        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsyncForAssesment(int? sectionId, int? examBuildingId, int? year)
        {
            return await _examRepository.GetExamsBySectionIdAsync(sectionId, 2, year, examBuildingId);
        }

        public async Task<IEnumerable<Exam>> GetExamsBySectionIdAsyncForAppeal(int? sectionId, int? examBuildingId, int? year)
        {
            return await _examRepository.GetExamsBySectionIdAsync(sectionId, 3, year, examBuildingId);
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

        public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds, int[] selectedSubjects)
        {
            await _examRepository.UpdateExamAsync(exam, commissionIds, degreeIds, selectedSubjects);
        }
        public async Task UpdateExamAsync(EditExamViewModelForAssesment exam)
        {
            await _examRepository.UpdateExamAsync(exam);
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
                UserName = model.UserName,
                Time = DateTime.Now,
                ExamId = model.ExamId,
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
        public List<Expert> GetExpertsByExam(int examId)
        {
            return _examRepository.GetExpertsByExam(examId);
        }
        public async Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId)
        {
            return await _examRepository.GetExpertSubProfessionsByExamIdAsync(examId);
        }

        public async Task AssignWorkersToExamAsync(int examId)
        {
            await _examRepository.AssignWorkersToExamAsync(examId);
        }

        public async Task AssignVolunteersToExamAsync(int examId)
        {
            await _examRepository.AssignVolunteersToExamAsync(examId);
        }

        public Task<MemoryStream> ExportExamScheduleToWord(int? year)
        {
            return _examRepository.ExportExamScheduleToWord(year);
        }
        public Task<MemoryStream> ExportExamScheduleToWordForSeason(int? year)
        {
            return _examRepository.ExportExamScheduleToWordForSeason(year);
        }
        public Task<MemoryStream> ExportExamCalendarToWord(int? year)
        {
            return _examRepository.ExportExamCalendarToWord(year);
        }
        public Task<MemoryStream> ExportExamScheduleToWordForLetter(int? year)
        {
            return _examRepository.ExportExamScheduleToWordForLetter(year);
        }

        public Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            return _examRepository.AssignRepresentativesToExamAsync(examId, selectedRepresentativeIds);
        }

        public Task<List<DimRepresentative>> GetAvailableRepresentativesAsync()
        {
            return _examRepository.GetAvailableRepresentativesAsync();
        }
        public Task<List<Monitor>> GetAvailableVolunteersAsync(int? sectionId)
        {
            return _examRepository.GetAvailableVolunteersAsync(sectionId);
        }
        public Task AssignMinistryRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds)
        {
            return _examRepository.AssignMinistryRepresentativesToExamAsync(examId, selectedRepresentativeIds);
        }
        public Task AssignVolunteersToExamAsync(int examId, List<int> selectedVolunteersIds)
        {
            return _examRepository.AssignVolunteersToExamAsync(examId, selectedVolunteersIds);
        }

        public Task<List<DimRepresentative>> GetAvailableMinistryRepresentativesAsync()
        {
            return _examRepository.GetAvailableMinistryRepresentativesAsync();
        }

        public Task<List<DataAccess.Models.Monitor>> GetAvailableWorkersAsync(int buildingId)
        {
            return _examRepository.GetAvailableWorkersAsync(buildingId);
        }

        public async Task<byte[]> ExportExamToWordAsync(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamMonitors).ThenInclude(em => em.Monitors).ThenInclude(em => em.WorkerTypeNavigation)
                .Include(e => e.ExamMonitors).ThenInclude(em => em.ExamRooms)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.Expert)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.SubProfession)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.ExamRoom)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) throw new Exception("İmtahan tapılmadı");

            using var ms = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(CreateCenteredBoldParagraph("İMTAHANDAKI İŞÇİ HEYƏTİN QEYDİYYAT VƏRƏQİ", 16));
                body.AppendChild(CreateMixedBoldParagraph("İmtahanın adı: ", "\u00A0" + exam.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan binası: ", "\u00A0" + exam.ExamBuilding?.Code + " " + exam.ExamBuilding?.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan tarixi: ", "\u00A0" + exam.ExamDate + " Saat " + exam.StartTime, 14));


                Table table = new Table();

                TableProperties tblProp = new TableProperties(
                    new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                    )
                );
                table.AppendChild(tblProp);

                var roomHeader = exam.SectionId == 1 ? "Məntəqə kodu" : "Zalın kodu";

                var headerRow = new TableRow();
                headerRow.Append(CreateTableCell("S/s", true, 1000));
                headerRow.Append(CreateTableCell(roomHeader, true, 1500));
                headerRow.Append(CreateTableCell("Vəzifə", true, 3000));
                headerRow.Append(CreateTableCell("Soyadı, adı, ata adı", true, 6000));
                AppendSignatureHeader(headerRow, exam.Shift, exam.SectionId);        
                table.Append(headerRow);

                int rowIndex = 1;
                if (exam.SectionId != 3 && exam.SectionId != 4 && exam.SectionId != 6)
                {
                    foreach (var expert in exam.ExamExpertSubProfessions)
                    {
                        var row = new TableRow();
                        row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                        row.Append(CreateTableCell(expert.ExamRoom?.Name, false, 1500));
                        row.Append(CreateTableCell($"Ekspert-{NormalizeSubProfessionName(expert.SubProfession?.Name)} ", false, 3000));
                        row.Append(CreateTableCell(expert.Expert.Surname + " " + expert.Expert.Name + " " + expert.Expert.Fname, false, 6000));
                        AppendSignatureCells(row, exam.Shift, exam.SectionId);          
                        table.Append(row);
                        rowIndex++;
                    }
                }

                foreach (var monitor in exam.ExamMonitors.Where(em => em.Monitors.Role == 2))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell(monitor.ExamRooms?.Name, false, 1500));
                    row.Append(CreateTableCell("Nəzarətçi ", false, 3000));
                    row.Append(CreateTableCell(monitor.Monitors.Surname + " " + monitor.Monitors.Name + " " + monitor.Monitors.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);
                    table.Append(row);
                    rowIndex++;
                }

                foreach (var monitor in exam.ExamMonitors.Where(em => em.Monitors.Role == 5))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell("", false, 1500));
                    row.Append(CreateTableCell(monitor.Monitors.WorkerTypeNavigation?.Name, false, 3000));
                    row.Append(CreateTableCell(monitor.Monitors.Surname + " " + monitor.Monitors.Name + " " + monitor.Monitors.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);
                    table.Append(row);
                    rowIndex++;
                }

                foreach (var representative in exam.Representatives.Where(dr => dr.Type == 1))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell(" ", false, 1500));
                    row.Append(CreateTableCell("DİM Nümayəndəsi", false, 3000));
                    row.Append(CreateTableCell(representative.Surname + " " + representative.Name + " " + representative.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);
                    table.Append(row);
                    rowIndex++;
                }

                foreach (var representative in exam.Representatives.Where(dr => dr.Type == 2))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell(" ", false, 1500));
                    row.Append(CreateTableCell("Nazirlik Nümayəndəsi", false, 3000));
                    row.Append(CreateTableCell(representative.Surname + " " + representative.Name + " " + representative.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);
                    table.Append(row);
                    rowIndex++;
                }

                foreach (var monitor in exam.ExamMonitors.Where(em => em.Monitors.Role == 1))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell("", false, 1500));
                    row.Append(CreateTableCell("İmtahan rəhbəri ", false, 3000));
                    row.Append(CreateTableCell(monitor.Monitors.Surname + " " + monitor.Monitors.Name + " " + monitor.Monitors.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);
                    table.Append(row);
                    rowIndex++;
                }

                body.AppendChild(table);
                body.AppendChild(CreateItalicParagraph("Qeyd. İmtahana gəlməyənlərin qarşısında (imza bölməsində) iştirakçıların imtahan binasına buraxılışı başlandıqdan sonra “gəlmədi” yazılır.", 10));

                body.AppendChild(CreateCenteredBoldParagraph("\nÜmumi imtahan rəhbəri: _________ / _______________________ / ", 12));



                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
        public async Task<byte[]> ExportExamToWordAsyncForMX(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamMonitors).ThenInclude(em => em.Monitors).ThenInclude(em => em.WorkerTypeNavigation)
                .Include(e => e.ExamMonitors).ThenInclude(em => em.ExamRooms)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.Expert)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.SubProfession)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(eesp => eesp.ExamRoom)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Representatives)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) throw new Exception("İmtahan tapılmadı");

            using var ms = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());
                if (exam.SectionId == 2)
                {

                    body.AppendChild(CreateCenteredBoldParagraph("QABİLİYYƏT İMTAHANI KOMİSSİYASININ QEYDİYYAT VƏRƏQİ", 16));
                    body.AppendChild(CreateMixedBoldParagraph("İmtahanın adı: ", "\u00A0" + exam.Name, 14));
                    body.AppendChild(CreateMixedBoldParagraph("Qiymətləndirmə binası: ", "\u00A0" + exam.ExamBuilding?.Code + " " + exam.ExamBuilding?.Name, 14));
                    body.AppendChild(CreateMixedBoldParagraph("Tarix: ", "\u00A0" + exam.ExamDate, 14));
                }
                else
                    body.AppendChild(CreateCenteredBoldParagraph("İMTAHANDAKI EKSPERTLƏRİN QEYDİYYAT VƏRƏQİ", 16));
                body.AppendChild(CreateMixedBoldParagraph("İmtahanın adı: ", "\u00A0" + exam.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan binası: ", "\u00A0" + exam.ExamBuilding?.Code + " " + exam.ExamBuilding?.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan tarixi: ", "\u00A0" + exam.ExamDate + " Saat " + exam.StartTime, 14));


                Table table = new Table();

                TableProperties tblProp = new TableProperties(
                    new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                    )
                );
                table.AppendChild(tblProp);

                var headerRow = new TableRow();
                headerRow.Append(CreateTableCell("S/s", true, 1000));
                headerRow.Append(CreateTableCell("Zal (Məntəqə kodu)", true, 1500));
                headerRow.Append(CreateTableCell("Vəzifə", true, 3000));
                headerRow.Append(CreateTableCell("Soyadı, adı, ata adı", true, 6000));
                AppendSignatureHeader(headerRow, exam.Shift, exam.SectionId);          // <-- İmza başlığı
                table.Append(headerRow);

                int rowIndex = 1;
                foreach (var expert in exam.ExamExpertSubProfessions)
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell(expert.Expert.Kons == false ? expert.ExamRoom?.Name : "", false, 1500));
                    var vez = expert.Expert.Kons == false ? "Ekspert" : "Konsertmeyster";
                    row.Append(CreateTableCell($"{vez}-{expert.SubProfession?.Name} ", false, 3000));
                    row.Append(CreateTableCell(expert.Expert.Name + " " + expert.Expert.Surname + " " + expert.Expert.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);              // <-- İmza xanası/xanaları
                    table.Append(row);
                    rowIndex++;
                }

                body.AppendChild(table);
                body.AppendChild(CreateItalicParagraph("Qeyd. İmtahana gəlməyən ekspertlərin qarşısında (imza bölməsində) iştirakçıların imtahan binasına buraxılışı başlandıqdan sonra “gəlmədi” yazılır.", 10));


                mainPart.Document.Save();
            }

            return ms.ToArray();
        }
        public async Task<byte[]> ExportExamToWordAsyncForV(int examId)
        {
            var exam = await _context.Exams
                .Include(e => e.ExamMonitors).ThenInclude(em => em.Monitors)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) throw new Exception("İmtahan tapılmadı");

            using var ms = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                body.AppendChild(CreateCenteredBoldParagraph("İMTAHANDAKI KÖNÜLLÜLƏRİN QEYDİYYAT VƏRƏQİ", 16));
                body.AppendChild(CreateMixedBoldParagraph("İmtahanın adı: ", "\u00A0" + exam.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan binası: ", "\u00A0" + exam.ExamBuilding?.Code + " " + exam.ExamBuilding?.Name, 14));
                body.AppendChild(CreateMixedBoldParagraph("İmtahan tarixi: ", "\u00A0" + exam.ExamDate + " Saat " + exam.StartTime, 14));


                Table table = new Table();

                TableProperties tblProp = new TableProperties(
                    new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }
                    )
                );
                table.AppendChild(tblProp);

                var headerRow = new TableRow();
                headerRow.Append(CreateTableCell("S/s", true, 1000));
                headerRow.Append(CreateTableCell("Vəzifə", true, 3000));
                headerRow.Append(CreateTableCell("Soyadı, adı, ata adı", true, 6000));
                AppendSignatureHeader(headerRow, exam.Shift, exam.SectionId);          // <-- İmza başlığı
                table.Append(headerRow);

                int rowIndex = 1;
                foreach (var monitor in exam.ExamMonitors.Where(em => em.Monitors.Role == 4))
                {
                    var row = new TableRow();
                    row.Append(CreateTableCell(rowIndex.ToString(), false, 1000));
                    row.Append(CreateTableCell("Könüllü ", false, 3000));
                    row.Append(CreateTableCell(monitor.Monitors.Surname + " " + monitor.Monitors.Name + " " + monitor.Monitors.Fname, false, 6000));
                    AppendSignatureCells(row, exam.Shift, exam.SectionId);              // <-- İmza xanası/xanaları
                    table.Append(row);
                    rowIndex++;
                }

                body.AppendChild(table);
                body.AppendChild(CreateItalicParagraph("Qeyd. İmtahana gəlməyənlərin qarşısında (imza bölməsində) iştirakçıların imtahan binasına buraxılışı başlandıqdan sonra “gəlmədi” yazılır.", 10));

                if (exam.SectionId == 1)
                {
                    body.AppendChild(CreateCenteredBoldParagraph("\nÜmumi imtahan rəhbəri: _________ / _______________________ / ", 12));
                }


                mainPart.Document.Save();
            }

            return ms.ToArray();
        }

        private static Paragraph CreateItalicParagraph(string text, int fontSize)
        {
            return new Paragraph(
                new Run(
                    new RunProperties(new Italic(), new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(text)
                )
            );
        }

        private static Paragraph CreateMixedBoldParagraph(string boldText, string normalText, int fontSize)
        {
            return new Paragraph(
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(boldText)
                ),
                new Run(
                    new RunProperties(new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(normalText)
                )
            );
        }



        private static Paragraph CreateBoldParagraph(string text, int fontSize)
        {
            return new Paragraph(
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(text)
                )
            );
        }

        private static Paragraph CreateParagraph(string text, int fontSize)
        {
            return new Paragraph(
                new Run(
                    new RunProperties(new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(text)
                )
            );
        }

        private static TableCell CreateTableCell(string text, bool bold, int width)
        {
            return new TableCell(
                new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Dxa, Width = width.ToString() }),
                new Paragraph(
                    new Run(
                        new RunProperties(new Bold { Val = bold }),
                        new Text(text)
                    )
                )
            );
        }
        private static Paragraph CreateCenteredBoldParagraph(string text, int fontSize)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification { Val = JustificationValues.Center } // Mərkəzə uyğunlaşdırma
                ),
                new Run(
                    new RunProperties(new Bold(), new FontSize { Val = (fontSize * 2).ToString() }),
                    new Text(text)
                )
            );
        }
        public async Task<ExamDetailsViewModel> GetExamDetailsAsync(int examId)
        {
            var exam = await _examRepository.GetByIdAsync(examId);
            if (exam == null) return null;

            var monitorIds = exam.Monitors.Select(m => m.Id).ToList();
            var expertIds = exam.Experts.Select(m => m.Id).ToList();

            // Ardıcıl icra — EF Core parallel sorğuları dəstəkləmir
            var monitorLogs = await _examRepository.GetMonitorsWithLogsAsync(monitorIds);
            var expertLogs = await _examRepository.GetExpertsWithLogsAsync(expertIds);

            // ExamMonitors-ı bir dəfə dict-ə çevir — hər monitor üçün
            // ayrıca _context.ExamMonitors sorğusu əvəzinə O(1) lookup
            var examMonitorDict = exam.Monitors
                .SelectMany(m => m.ExamMonitors.Where(em => em.ExamId == examId))
                .ToDictionary(em => em.MonitorId);

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
                burK = exam.burK,
                burQ = exam.burQ,
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

                ExamSubjects = exam.ExamSubjects.Select(ec => new ExamSubjectViewModel
                {
                    Subject = new SubjectViewModel { Name = ec.Subjects.Name }
                }).ToList(),

                Experts = exam.Experts.Select(e => new ExpertViewModelForExam
                {
                    Id = e.Id,
                    Name = e.Name,
                    Surname = e.Surname,
                    Fname = e.Fname,
                    FinCode = e.FinCode,
                    Kons = e.Kons,
                    Tel = e.TelIs,
                    ExamExpertSubProfessions = e.ExamExpertSubProfessions
                        .Select(eesp => new ExamExpertSubProfessionViewModelForExam
                        {
                            Name = eesp.SubProfession?.Name,
                            FederationName = eesp.Federation?.Name,
                            RoomName = eesp.ExamRoom?.Name,
                            IsAttended = eesp.IsAttended
                        }).ToList(),
                    IsAttended = e.ExamExpertSubProfessions.Any(eesp => eesp.IsAttended == 1) ? 1 : 0
                }).ToList(),

                Monitors = exam.Monitors.Select(m => new MonitorViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Surname = m.Surname,
                    Fname = m.Fname,
                    FinCode = m.FinCode,
                    Role = m.Role,
                    Tel = m.TelIs,
                    // N+1 fix: WorkerTypeNavigation artıq GetByIdAsync-da include edilib
                    WorkerType = m.WorkerTypeNavigation?.Name,
                    Rooms = m.ExamMonitors
                        .Where(em => em.ExamId == examId && em.ExamRooms != null)
                        .Select(em => new RoomViewModelForExam { RoomName = em.ExamRooms?.Name })
                        .ToList(),
                    // N+1 fix: dict-dən oxunur, ayrıca DB sorğusu yoxdur
                    IsAttended = examMonitorDict.TryGetValue(m.Id, out var emEntry)
                        ? emEntry.IsAttended ?? 0
                        : 0
                }).ToList(),

                ExamRepresentatives = exam.Representatives.Select(er => new RepresentativeViewModel
                {
                    Id = er.Id,
                    Name = er.Name,
                    Surname = er.Surname,
                    Fname = er.Fname,
                    FinCode = er.FinCode,
                    Role = (byte?)er.Type,
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
        public async Task RemoveExpertsFromExamAsync(int examId, List<int> expertIds)
        {
            var exam = await _examRepository.GetTrackedByIdAsync(examId);
            if (exam == null) throw new ArgumentException("İmtahan tapılmadı");

            // İmtahan günü keçibsə silmə əməliyyatına icazə verilmir
            if (exam.ExamDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException(
                    "İmtahan günü keçdiyi üçün ekspert silmək mümkün deyil.");

            var examExpertSubProfessions = await _examExpertSubProfessionRepository
                        .GetAllAsync(x => x.ExamId == examId && expertIds.Contains(x.ExpertId));

            var expertsToRemove = exam.Experts.Where(e => expertIds.Contains(e.Id)).ToList();
            foreach (var expert in expertsToRemove)
            {
                exam.Experts.Remove(expert);
            }
            _examExpertSubProfessionRepository.RemoveRange(examExpertSubProfessions);

            await _examRepository.SaveAsync();
        }

        public async Task RemoveMonitorsFromExamAsync(int examId, List<int> monitorIds)
        {
            var exam = await _examRepository.GetTrackedByIdAsync(examId);
            if (exam == null) throw new ArgumentException("İmtahan tapılmadı");

            // İmtahan günü keçibsə silmə əməliyyatına icazə verilmir
            // (Bu metod həm Monitor, həm HeadMonitor, həm də Worker silmə üçün istifadə olunur)
            if (exam.ExamDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException(
                    "İmtahan günü keçdiyi üçün rəhbər/baş rəhbər/işçi silmək mümkün deyil.");

            var monitorsToRemove = exam.Monitors.Where(m => monitorIds.Contains(m.Id)).ToList();
            foreach (var monitor in monitorsToRemove)
            {
                exam.Monitors.Remove(monitor);
            }

            await _examRepository.SaveAsync();
        }

        public async Task RemoveRepresentativesFromExamAsync(int examId, List<int> representativeIds)
        {
            var exam = await _examRepository.GetTrackedByIdAsync(examId);
            if (exam == null) throw new ArgumentException("İmtahan tapılmadı");

            if (exam.ExamDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException(
                    "İmtahan günü keçdiyi üçün nümayəndə silmək mümkün deyil.");

            var representativesToRemove = exam.Representatives.Where(m => representativeIds.Contains(m.Id)).ToList();
            foreach (var representative in representativesToRemove)
            {
                exam.Representatives.Remove(representative);
            }

            await _examRepository.SaveAsync();
        }
        public async Task<byte[]> GetExamDataForExport(DateOnly selectedDate, int sectionId)
        {
            var exams = _context.Exams
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.WorkerTypeNavigation)
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.Contracts)
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.MonitorLogs)
                .Include(e => e.Experts)
                    .ThenInclude(em => em.Contracts)
                .Include(e => e.ExamDegrees)
                .Include(e => e.Section)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Representatives)
                .Where(e => e.ExamDate == selectedDate && e.SectionId == sectionId)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Exam Data");

            worksheet.Cell(1, 1).Value = "V_NUM";
            worksheet.Cell(1, 2).Value = "RAYON";
            worksheet.Cell(1, 3).Value = "SOY";
            worksheet.Cell(1, 4).Value = "ADI";
            worksheet.Cell(1, 5).Value = "BABA";
            worksheet.Cell(1, 6).Value = "BINA";
            worksheet.Cell(1, 7).Value = "IMT_KOD";
            worksheet.Cell(1, 8).Value = "IL";
            worksheet.Cell(1, 9).Value = "IMT_GUN";
            worksheet.Cell(1, 10).Value = "IMT_AY";
            worksheet.Cell(1, 11).Value = "imt_vezife";
            worksheet.Cell(1, 12).Value = "muqavile";
            worksheet.Cell(1, 13).Value = "MuqavileNo";
            worksheet.Cell(1, 14).Value = "cins";
            worksheet.Cell(1, 15).Value = "SERIYA_P";
            worksheet.Cell(1, 16).Value = "NUM_POSP";
            worksheet.Cell(1, 17).Value = "TELEFON";
            worksheet.Cell(1, 18).Value = "sv_pinkod";
            worksheet.Cell(1, 19).Value = "VOEN";
            worksheet.Cell(1, 20).Value = "Hesablashma";
            worksheet.Cell(1, 21).Value = "rekvizit";
            worksheet.Cell(1, 22).Value = "SSN";
            worksheet.Cell(1, 23).Value = "Bank_Filialı";
            worksheet.Cell(1, 24).Value = "Bank_Filial_Kodu";
            worksheet.Cell(1, 25).Value = "Novbe";

            int row = 2;
            foreach (var exam in exams)
            {
                var monitors = exam.Monitors
                                   .Where(m => !m.MonitorLogs.Any(log => log.ExamId == exam.Id && log.Kind == 0));
                foreach (var monitor in monitors)
                {
                    worksheet.Cell(row, 1).Value = monitor.VNum;
                    worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                    worksheet.Cell(row, 3).Value = monitor.Surname;
                    worksheet.Cell(row, 4).Value = monitor.Name;
                    worksheet.Cell(row, 5).Value = monitor.Fname;
                    worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                    if (exam.ExamDegrees.Select(ed => ed.DegreeId).FirstOrDefault() == 2)
                    {
                        worksheet.Cell(row, 7).Value = "36";
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = exam.Section?.SectCode.ToString();
                    }
                    worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                    worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                    worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();
                    if (monitor.Role == 2 && exam.SectionId != 2)
                    {
                        worksheet.Cell(row, 11).Value = "1.2";
                    }
                    else if (monitor.Role == 1 && exam.SectionId != 2)
                    {
                        worksheet.Cell(row, 11).Value = "3.1";
                    }
                    else if (monitor.WorkerType == 1)
                    {
                        worksheet.Cell(row, 11).Value = "30";
                    }
                    else if (monitor.WorkerType == 2)
                    {
                        worksheet.Cell(row, 11).Value = "29";
                    }
                    else if (monitor.WorkerType == 3)
                    {
                        worksheet.Cell(row, 11).Value = "30.1";
                    }
                    else if (monitor.Role == 1 && exam.SectionId == 2 && (exam.EndTime.Value - exam.StartTime.Value).Hours == 6)
                    {
                        worksheet.Cell(row, 11).Value = "92";
                    }
                    else if (monitor.Role == 1 && exam.SectionId == 2 && (exam.EndTime.Value - exam.StartTime.Value).TotalHours == 4.5)
                    {
                        worksheet.Cell(row, 11).Value = "93";
                    }
                    else if (monitor.Role == 1 && exam.SectionId == 2)
                    {
                        worksheet.Cell(row, 11).Value = "3.1";
                    }
                    else if (monitor.Role == 2 && exam.SectionId == 2 && (exam.EndTime.Value - exam.StartTime.Value).Hours == 6)
                    {
                        worksheet.Cell(row, 11).Value = "90";
                    }
                    else if (monitor.Role == 2 && exam.SectionId == 2 && (exam.EndTime.Value - exam.StartTime.Value).TotalHours == 4.5)
                    {
                        worksheet.Cell(row, 11).Value = "91";
                    }

                    else if (monitor.Role == 2 && exam.SectionId == 2)
                    {
                        worksheet.Cell(row, 11).Value = "1.2";
                    }

                    var latestContract = monitor.Contracts
                        .OrderByDescending(c => c.Id)
                        .FirstOrDefault();

                    worksheet.Cell(row, 12).Value = latestContract?.Date.ToString("dd.MM.yyyy") ?? "";
                    worksheet.Cell(row, 13).Value = latestContract?.Number ?? "";
                    worksheet.Cell(row, 14).Value = monitor.Gender.ToString();
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        worksheet.Cell(row, 15).Value = monitor.Serial.StartsWith("AA") || monitor.Serial.Length == 7
                                                        ? "AA"
                                                        : (monitor.Serial.Length == 8 || monitor.Serial.StartsWith("AZE") ? "AZE" : null);
                    }
                    else
                    {
                        worksheet.Cell(row, 15).Value = "";
                    }
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        string onlyNumbers = Regex.Replace(monitor.Serial, @"\D", "");

                        if (!string.IsNullOrEmpty(onlyNumbers))
                            worksheet.Cell(row, 16).Value = onlyNumbers;
                        else
                            worksheet.Cell(row, 16).Value = "";
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = "";
                    }
                    worksheet.Cell(row, 17).Value = monitor.TelIs;
                    worksheet.Cell(row, 18).Value = monitor.FinCode;
                    worksheet.Cell(row, 19).Value = monitor.Voen;
                    worksheet.Cell(row, 20).Value = monitor.HesablashmaH;
                    worksheet.Cell(row, 21).Value = monitor.Rekvizit;
                    worksheet.Cell(row, 22).Value = monitor.SSN;
                    worksheet.Cell(row, 23).Value = monitor.BankFilial;
                    worksheet.Cell(row, 24).Value = monitor.BankFilialCode;
                    worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                    row++;
                }
                if (exam.SectionId != 1)
                {
                    foreach (var expert in exam.Experts)
                    {
                        worksheet.Cell(row, 1).Value = "";
                        worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                        worksheet.Cell(row, 3).Value = expert.Surname;
                        worksheet.Cell(row, 4).Value = expert.Name;
                        worksheet.Cell(row, 5).Value = expert.Fname;
                        worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                        if (exam.ExamDegrees.Select(ed => ed.DegreeId).FirstOrDefault() == 2)
                        {
                            worksheet.Cell(row, 7).Value = "36";
                        }
                        else
                        {
                            worksheet.Cell(row, 7).Value = exam.Section?.SectCode.ToString();
                        }
                        worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                        worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                        worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();
                        if (expert.Kons == false)
                        {
                            worksheet.Cell(row, 11).Value = "6";
                        }
                        else if (expert.Kons == true)
                        {
                            worksheet.Cell(row, 11).Value = "6.1";
                        }
                        var latestContract = expert.Contracts
                            .OrderByDescending(c => c.Id)
                            .FirstOrDefault();

                        worksheet.Cell(row, 12).Value = latestContract?.Date.ToString("dd.MM.yyyy") ?? "";
                        worksheet.Cell(row, 13).Value = latestContract?.Number ?? "";
                        worksheet.Cell(row, 14).Value = expert.Gender.ToString();
                        if (!string.IsNullOrEmpty(expert?.Serial))
                        {
                            worksheet.Cell(row, 15).Value = expert.Serial.StartsWith("AA") || expert.Serial.Length == 7
                                                            ? "AA"
                                                            : (expert.Serial.Length == 8 || expert.Serial.StartsWith("AZE") ? "AZE" : null);

                        }
                        else
                        {
                            worksheet.Cell(row, 15).Value = "";
                        }
                        if (!string.IsNullOrEmpty(expert?.Serial))
                        {
                            string onlyNumbers = Regex.Replace(expert.Serial, @"\D", "");

                            if (!string.IsNullOrEmpty(onlyNumbers))
                                worksheet.Cell(row, 16).Value = onlyNumbers;
                            else
                                worksheet.Cell(row, 16).Value = "";
                        }
                        else
                        {
                            worksheet.Cell(row, 16).Value = "";
                        }
                        worksheet.Cell(row, 17).Value = expert.TelIs;
                        worksheet.Cell(row, 18).Value = expert.FinCode;
                        worksheet.Cell(row, 19).Value = expert.Voen;
                        worksheet.Cell(row, 20).Value = expert.HesablashmaH;
                        worksheet.Cell(row, 21).Value = expert.Rekvizit;
                        worksheet.Cell(row, 22).Value = expert.SSN;
                        worksheet.Cell(row, 23).Value = expert.BankFilial;
                        worksheet.Cell(row, 24).Value = expert.BankFilialCode;
                        worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                        row++;
                    }
                }
                foreach (var monitor in exam.Representatives)
                {
                    worksheet.Cell(row, 1).Value = "";
                    worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                    worksheet.Cell(row, 3).Value = monitor.Surname;
                    worksheet.Cell(row, 4).Value = monitor.Name;
                    worksheet.Cell(row, 5).Value = monitor.Fname;
                    worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                    if (exam.ExamDegrees.Select(ed => ed.DegreeId).FirstOrDefault() == 2)
                    {
                        worksheet.Cell(row, 7).Value = "36";
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = exam.Section?.SectCode.ToString();
                    }
                    worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                    worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                    worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();
                    worksheet.Cell(row, 11).Value = "";

                    worksheet.Cell(row, 12).Value = "";
                    worksheet.Cell(row, 13).Value = "";
                    worksheet.Cell(row, 14).Value = monitor.Gender.ToString();
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        worksheet.Cell(row, 15).Value = monitor.Serial.StartsWith("AA") || monitor.Serial.Length == 7
                                                        ? "AA"
                                                        : (monitor.Serial.Length == 8 || monitor.Serial.StartsWith("AZE") ? "AZE" : null);
                    }
                    else
                    {
                        worksheet.Cell(row, 15).Value = "";
                    }
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        string onlyNumbers = Regex.Replace(monitor.Serial, @"\D", "");

                        if (!string.IsNullOrEmpty(onlyNumbers))
                            worksheet.Cell(row, 16).Value = onlyNumbers;
                        else
                            worksheet.Cell(row, 16).Value = "";
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = "";
                    }
                    worksheet.Cell(row, 17).Value = monitor.Tel;
                    worksheet.Cell(row, 18).Value = monitor.FinCode;
                    worksheet.Cell(row, 19).Value = "";
                    worksheet.Cell(row, 20).Value = "";
                    worksheet.Cell(row, 21).Value = "";
                    worksheet.Cell(row, 22).Value = "";
                    worksheet.Cell(row, 23).Value = "";
                    worksheet.Cell(row, 24).Value = "";
                    worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                    row++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> GetExamDataForFoodAndWater(DateOnly startDate, DateOnly endDate, int sectionId)
        {
            var exams = _context.Exams
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.WorkerTypeNavigation)
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.Contracts)
                .Include(e => e.Monitors)
                    .ThenInclude(em => em.MonitorLogs)
                .Include(e => e.Experts)
                    .ThenInclude(em => em.Contracts)
                .Include(e => e.ExamDegrees)
                .Include(e => e.Section)
                .Include(e => e.ExamBuilding)
                .Include(e => e.Representatives)
                .Where(e => e.ExamDate >= startDate && e.ExamDate <= endDate && e.SectionId == sectionId)
                .ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Exam Data");

            worksheet.Cell(1, 1).Value = "V_NUM";
            worksheet.Cell(1, 2).Value = "RAYON";
            worksheet.Cell(1, 3).Value = "SOY";
            worksheet.Cell(1, 4).Value = "ADI";
            worksheet.Cell(1, 5).Value = "BABA";
            worksheet.Cell(1, 6).Value = "BINA";
            worksheet.Cell(1, 7).Value = "IMT_KOD";
            worksheet.Cell(1, 8).Value = "IL";
            worksheet.Cell(1, 9).Value = "IMT_GUN";
            worksheet.Cell(1, 10).Value = "IMT_AY";
            worksheet.Cell(1, 11).Value = "imt_vezife";
            worksheet.Cell(1, 12).Value = "muqavile";
            worksheet.Cell(1, 13).Value = "MuqavileNo";
            worksheet.Cell(1, 14).Value = "cins";
            worksheet.Cell(1, 15).Value = "SERIYA_P";
            worksheet.Cell(1, 16).Value = "NUM_POSP";
            worksheet.Cell(1, 17).Value = "TELEFON";
            worksheet.Cell(1, 18).Value = "sv_pinkod";
            worksheet.Cell(1, 19).Value = "VOEN";
            worksheet.Cell(1, 20).Value = "Hesablashma";
            worksheet.Cell(1, 21).Value = "rekvizit";
            worksheet.Cell(1, 22).Value = "SSN";
            worksheet.Cell(1, 23).Value = "Bank_Filialı";
            worksheet.Cell(1, 24).Value = "Bank_Filial_Kodu";
            worksheet.Cell(1, 25).Value = "Novbe";

            int row = 2;
            foreach (var exam in exams)
            {
                var monitors = exam.Monitors
                                   .Where(m => !m.MonitorLogs.Any(log => log.ExamId == exam.Id && log.Kind == 0));
                foreach (var monitor in monitors)
                {
                    worksheet.Cell(row, 1).Value = monitor.VNum;
                    worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                    worksheet.Cell(row, 3).Value = monitor.Surname;
                    worksheet.Cell(row, 4).Value = monitor.Name;
                    worksheet.Cell(row, 5).Value = monitor.Fname;
                    worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                    worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                    worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                    worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();
                    worksheet.Cell(row, 11).Value = monitor.Role switch
                    {
                        1 => "İmtahan rəhbəri",
                        2 => "Nəzarətçi",
                        4 => "Könüllü",
                        _ => ""
                    };

                    var latestContract = monitor.Contracts
                        .OrderByDescending(c => c.Id)
                        .FirstOrDefault();

                    worksheet.Cell(row, 12).Value = latestContract?.Date.ToString("dd.MM.yyyy") ?? "";
                    worksheet.Cell(row, 13).Value = latestContract?.Number ?? "";
                    worksheet.Cell(row, 14).Value = monitor.Gender.ToString();
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        worksheet.Cell(row, 15).Value = monitor.Serial.StartsWith("AA") || monitor.Serial.Length == 7
                                                        ? "AA"
                                                        : (monitor.Serial.Length == 8 || monitor.Serial.StartsWith("AZE") ? "AZE" : null);
                    }
                    else
                    {
                        worksheet.Cell(row, 15).Value = "";
                    }
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        string onlyNumbers = Regex.Replace(monitor.Serial, @"\D", "");

                        if (!string.IsNullOrEmpty(onlyNumbers))
                            worksheet.Cell(row, 16).Value = onlyNumbers;
                        else
                            worksheet.Cell(row, 16).Value = "";
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = "";
                    }
                    worksheet.Cell(row, 17).Value = monitor.TelIs;
                    worksheet.Cell(row, 18).Value = monitor.FinCode;
                    worksheet.Cell(row, 19).Value = monitor.Voen;
                    worksheet.Cell(row, 20).Value = monitor.HesablashmaH;
                    worksheet.Cell(row, 21).Value = monitor.Rekvizit;
                    worksheet.Cell(row, 22).Value = monitor.SSN;
                    worksheet.Cell(row, 23).Value = monitor.BankFilial;
                    worksheet.Cell(row, 24).Value = monitor.BankFilialCode;
                    worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                    row++;
                }
                foreach (var expert in exam.Experts)
                {
                    worksheet.Cell(row, 1).Value = "";
                    worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                    worksheet.Cell(row, 3).Value = expert.Surname;
                    worksheet.Cell(row, 4).Value = expert.Name;
                    worksheet.Cell(row, 5).Value = expert.Fname;
                    worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                    if (exam.ExamDegrees.Select(ed => ed.DegreeId).FirstOrDefault() == 2)
                    {
                        worksheet.Cell(row, 7).Value = "36";
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = exam.Section?.SectCode.ToString();
                    }
                    worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                    worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                    worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();

                    worksheet.Cell(row, 11).Value = (bool)expert.Kons ? "Konsertmeyster" : "Ekspert";

                    var latestContract = expert.Contracts
                            .OrderByDescending(c => c.Id)
                            .FirstOrDefault();

                    worksheet.Cell(row, 12).Value = latestContract?.Date.ToString("dd.MM.yyyy") ?? "";
                    worksheet.Cell(row, 13).Value = latestContract?.Number ?? "";
                    worksheet.Cell(row, 14).Value = expert.Gender.ToString();
                    if (!string.IsNullOrEmpty(expert?.Serial))
                    {
                        worksheet.Cell(row, 15).Value = expert.Serial.StartsWith("AA") || expert.Serial.Length == 7
                                                        ? "AA"
                                                        : (expert.Serial.Length == 8 || expert.Serial.StartsWith("AZE") ? "AZE" : null);

                    }
                    else
                    {
                        worksheet.Cell(row, 15).Value = "";
                    }
                    if (!string.IsNullOrEmpty(expert?.Serial))
                    {
                        string onlyNumbers = Regex.Replace(expert.Serial, @"\D", "");

                        if (!string.IsNullOrEmpty(onlyNumbers))
                            worksheet.Cell(row, 16).Value = onlyNumbers;
                        else
                            worksheet.Cell(row, 16).Value = "";
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = "";
                    }
                    worksheet.Cell(row, 17).Value = expert.TelIs;
                    worksheet.Cell(row, 18).Value = expert.FinCode;
                    worksheet.Cell(row, 19).Value = expert.Voen;
                    worksheet.Cell(row, 20).Value = expert.HesablashmaH;
                    worksheet.Cell(row, 21).Value = expert.Rekvizit;
                    worksheet.Cell(row, 22).Value = expert.SSN;
                    worksheet.Cell(row, 23).Value = expert.BankFilial;
                    worksheet.Cell(row, 24).Value = expert.BankFilialCode;
                    worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                    row++;
                }

                foreach (var monitor in exam.Representatives)
                {
                    worksheet.Cell(row, 1).Value = "";
                    worksheet.Cell(row, 2).Value = exam.DistrictId.ToString();
                    worksheet.Cell(row, 3).Value = monitor.Surname;
                    worksheet.Cell(row, 4).Value = monitor.Name;
                    worksheet.Cell(row, 5).Value = monitor.Fname;
                    worksheet.Cell(row, 6).Value = exam.ExamBuilding.Code;
                    if (exam.ExamDegrees.Select(ed => ed.DegreeId).FirstOrDefault() == 2)
                    {
                        worksheet.Cell(row, 7).Value = "36";
                    }
                    else
                    {
                        worksheet.Cell(row, 7).Value = exam.Section?.SectCode.ToString();
                    }
                    worksheet.Cell(row, 8).Value = exam.ExamDate.Year.ToString();
                    worksheet.Cell(row, 9).Value = exam.ExamDate.Day.ToString();
                    worksheet.Cell(row, 10).Value = exam.ExamDate.Month.ToString();
                    worksheet.Cell(row, 11).Value = monitor.Type switch
                    {
                        1 => "DİM nümayəndəsi",
                        2 => "GİN nümayəndəsi",
                        _ => ""
                    };


                    worksheet.Cell(row, 12).Value = "";
                    worksheet.Cell(row, 13).Value = "";
                    worksheet.Cell(row, 14).Value = monitor.Gender.ToString();
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        worksheet.Cell(row, 15).Value = monitor.Serial.StartsWith("AA") || monitor.Serial.Length == 7
                                                        ? "AA"
                                                        : (monitor.Serial.Length == 8 || monitor.Serial.StartsWith("AZE") ? "AZE" : null);
                    }
                    else
                    {
                        worksheet.Cell(row, 15).Value = "";
                    }
                    if (!string.IsNullOrEmpty(monitor?.Serial))
                    {
                        string onlyNumbers = Regex.Replace(monitor.Serial, @"\D", "");

                        if (!string.IsNullOrEmpty(onlyNumbers))
                            worksheet.Cell(row, 16).Value = onlyNumbers;
                        else
                            worksheet.Cell(row, 16).Value = "";
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = "";
                    }
                    worksheet.Cell(row, 17).Value = monitor.Tel;
                    worksheet.Cell(row, 18).Value = monitor.FinCode;
                    worksheet.Cell(row, 19).Value = "";
                    worksheet.Cell(row, 20).Value = "";
                    worksheet.Cell(row, 21).Value = "";
                    worksheet.Cell(row, 22).Value = "";
                    worksheet.Cell(row, 23).Value = "";
                    worksheet.Cell(row, 24).Value = "";
                    worksheet.Cell(row, 25).Value = exam.Shift.ToString();
                    row++;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private string GetMonitorRoleValue(Monitor monitor, Exam exam)
        {
            if (monitor.Role == 2 && exam.SectionId != 2) return "1.2";
            if (monitor.Role == 1 && exam.SectionId != 2) return "3.1";
            if (monitor.WorkerType == 1) return "30";
            if (monitor.WorkerType == 2) return "29";
            if (monitor.WorkerType == 3) return "30.1";
            return "";
        }

        public async Task AddExamAsyncForAssesment(CreateExamViewModelForAssesment exam)
        {
            await _examRepository.AddAsyncForAssesment(exam);
        }

        public Task AddExamAsyncForAppeal(CreateExamViewModel exam)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> ChangeExpertAsync(ChangeExpertViewModel model)
        {
            var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == model.ExamId);
            if (exam == null) return false;

            var currentAssignment = await _context.ExamExpertSubProfessions
                .FirstOrDefaultAsync(x => x.ExamId == model.ExamId && x.ExpertId == model.CurrentExpertId);
            if (currentAssignment == null) return false;

            int subProfessionId = (int)currentAssignment.SubProfessionId;

            _context.ExamExpertSubProfessions.Remove(currentAssignment);

            var examExpertRecord = await _context.ExamExperts
                .FirstOrDefaultAsync(x => x.ExamId == model.ExamId && x.ExpertId == model.CurrentExpertId);
            if (examExpertRecord != null)
            {
                _context.ExamExperts.Remove(examExpertRecord);
            }

            var currentExpert = await _context.Experts.FindAsync(model.CurrentExpertId);
            var newExpert = await _context.Experts.FindAsync(model.NewExpertId);
            if (currentExpert == null || newExpert == null) return false;

            var newSubProfessionAssignment = new ExamExpertSubProfession
            {
                ExamId = model.ExamId,
                ExpertId = model.NewExpertId,
                SubProfessionId = subProfessionId,
                RoomId = currentAssignment.RoomId,
                FederationId = currentAssignment.FederationId,
                IsAttended = 0
            };
            await _context.ExamExpertSubProfessions.AddAsync(newSubProfessionAssignment);

            var newExamExpert = new ExamExpert
            {
                ExamId = model.ExamId,
                ExpertId = model.NewExpertId
            };
            await _context.ExamExperts.AddAsync(newExamExpert);


            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<byte[]> ExportFoodAndWaterRangeAsync(DateOnly start, DateOnly end, int? sectionId, int examBuildingId)
        {
            var query = _context.Exams
                .Where(e => e.ExamDate >= start && e.ExamDate <= end);

            if (sectionId.HasValue)
                query = query.Where(e => e.SectionId == sectionId.Value);

            query = query.Where(e => e.ExamBuldingId == examBuildingId);

            var exams = await query
                .Include(e => e.ExamBuilding)
                .Include(e => e.District)
                .Include(e => e.ExamMonitors).ThenInclude(em => em.Monitors)
                .Include(e => e.ExamExpertSubProfessions).ThenInclude(es => es.Expert)
                .Include(e => e.ExamExperts).ThenInclude(ee => ee.Experts)
                .Include(e => e.ExamRepresentatives).ThenInclude(ee => ee.Representative)
                .OrderBy(e => e.ExamDate)
                .ToListAsync();
            var allRows = new List<(RowEntry row, int examId, DateTime examDate)>();
            var culture = new CultureInfo("az-Latn-AZ");

            foreach (var exam in exams)
            {
                DateTime examDateTime = exam.ExamDate.ToDateTime(System.TimeOnly.MinValue);
                string rayon = exam.District?.Name ?? "";
                string binaKodu = exam.ExamBuilding?.Code ?? exam.ExamBuilding?.Id.ToString() ?? "";

                foreach (var es in exam.ExamExpertSubProfessions ?? Enumerable.Empty<ExamExpertSubProfession>())
                {
                    var expert = es.Expert;
                    if (expert == null) continue;

                    var r = new RowEntry
                    {
                        Rayon = rayon,
                        BinaKodu = binaKodu,
                        Soyad = expert.Surname ?? "",
                        Ad = expert.Name ?? "",
                        Ata = expert.Fname ?? "",
                        Il = examDateTime.ToString("yyyy", CultureInfo.InvariantCulture),
                        Gun = examDateTime.ToString("dd", CultureInfo.InvariantCulture),
                        Ay = examDateTime.ToString("MMMM", culture),
                        FinCode = GetPersonFinCode(expert.FinCode),
                        Vezife = (expert.Kons == true) ? "Konsertmeyster" : "Ekspert"
                    };
                    allRows.Add((r, exam.Id, examDateTime));
                }

                foreach (var ee in exam.ExamExperts ?? Enumerable.Empty<ExamExpert>())
                {
                    var expert = ee.Experts;
                    if (expert == null) continue;

                    var fin = GetPersonFinCode(expert.FinCode);
                    if (allRows.Any(x => x.examId == exam.Id && x.row.Soyad == expert.Surname && x.row.Ad == expert.Name && x.row.FinCode == fin))
                        continue;

                    var r = new RowEntry
                    {
                        Rayon = rayon,
                        BinaKodu = binaKodu,
                        Soyad = expert.Surname ?? "",
                        Ad = expert.Name ?? "",
                        Ata = expert.Fname ?? "",
                        Il = examDateTime.ToString("yyyy", CultureInfo.InvariantCulture),
                        Gun = examDateTime.ToString("dd", CultureInfo.InvariantCulture),
                        Ay = examDateTime.ToString("MMMM", culture),
                        FinCode = fin,
                        Vezife = (expert.Kons == true) ? "Konsertmeyster" : "Ekspert"
                    };
                    allRows.Add((r, exam.Id, examDateTime));
                }

                foreach (var em in exam.ExamMonitors ?? Enumerable.Empty<ExamMonitor>())
                {
                    var monitor = em.Monitors;
                    if (monitor == null) continue;

                    var r = new RowEntry
                    {
                        Rayon = rayon,
                        BinaKodu = binaKodu,
                        Soyad = monitor.Surname ?? "",
                        Ad = monitor.Name ?? "",
                        Ata = monitor.Fname ?? "",
                        Il = examDateTime.ToString("yyyy", CultureInfo.InvariantCulture),
                        Gun = examDateTime.ToString("dd", CultureInfo.InvariantCulture),
                        Ay = examDateTime.ToString("MMMM", culture),
                        FinCode = GetPersonFinCode(monitor.FinCode),
                        Vezife = MapMonitorRole(monitor.Role, monitor.RoleNavigation?.ToString())
                    };
                    allRows.Add((r, exam.Id, examDateTime));
                }
                foreach (var em in exam.ExamRepresentatives ?? Enumerable.Empty<ExamRepresentative>())
                {
                    var representative = em.Representative;
                    if (representative == null) continue;


                    var r = new RowEntry
                    {
                        Rayon = rayon,
                        BinaKodu = binaKodu,
                        Soyad = representative.Surname ?? "",
                        Ad = representative.Name ?? "",
                        Ata = representative.Fname ?? "",
                        Il = examDateTime.ToString("yyyy", CultureInfo.InvariantCulture),
                        Gun = examDateTime.ToString("dd", CultureInfo.InvariantCulture),
                        Ay = examDateTime.ToString("MMMM", culture),
                        FinCode = GetPersonFinCode(representative.FinCode),
                        Vezife = MapRepresentativeRole((byte?)representative.Type, representative.Type.ToString())
                    };
                    allRows.Add((r, exam.Id, examDateTime));
                }
            }

            using var mem = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(mem, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                var main = doc.AddMainDocumentPart();
                main.Document = new Document();
                var body = main.Document.AppendChild(new Body());

                if (!allRows.Any())
                {
                    body.AppendChild(new Paragraph(new Run(new Text($"Seçilmiş tarix aralığında ({start} - {end}) imtahan tapılmadı."))));
                    main.Document.Save();
                    return mem.ToArray();
                }

                var table = new Table();

                TableProperties tblProps = new TableProperties(
                    new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto },
                    new TableBorders(
                        new TopBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new BottomBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new LeftBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new RightBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new InsideHorizontalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                        new InsideVerticalBorder { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                    )
                );
                table.AppendChild(tblProps);

                var headerRow = new TableRow();
                headerRow.AppendChild(new TableRowProperties(new TableHeader()));
                var headers = new[] { "Rayon", "Bina kodu", "Soyad", "Ad", "Ata", "İl", "Gün", "Ay", "Fin kodu", "Vəzifə" };
                foreach (var h in headers)
                {
                    var tc = new TableCell();
                    var p = new Paragraph(new Run(new RunProperties(new Bold()), new Text(h ?? "")));
                    tc.Append(p);
                    tc.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                    headerRow.Append(tc);
                }
                table.AppendChild(headerRow);

                foreach (var item in allRows)
                {
                    var r = item.row;
                    var tr = new TableRow();

                    tr.AppendChild(new TableRowProperties(new CantSplit()));

                    tr.Append(CreateTableCell(r.Rayon));
                    tr.Append(CreateTableCell(r.BinaKodu));
                    tr.Append(CreateTableCell(r.Soyad));
                    tr.Append(CreateTableCell(r.Ad));
                    tr.Append(CreateTableCell(r.Ata));
                    tr.Append(CreateTableCell(r.Il));
                    tr.Append(CreateTableCell(r.Gun));
                    tr.Append(CreateTableCell(r.Ay));
                    tr.Append(CreateTableCell(r.FinCode));
                    tr.Append(CreateTableCell(r.Vezife));

                    table.AppendChild(tr);
                }

                body.AppendChild(table);

                main.Document.Save();
            }

            return mem.ToArray();
        }


        private static TableRow CreateTableRow(string[] cells, bool isHeader = false)
        {
            var tr = new TableRow();
            foreach (var text in cells)
            {
                var tc = new TableCell();
                Paragraph p;
                if (isHeader)
                {
                    p = new Paragraph(new Run(new RunProperties(new Bold()), new Text(text ?? "")));
                }
                else
                {
                    p = new Paragraph(new Run(new Text(text ?? "")));
                }
                tc.Append(p);
                tc.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
                tr.Append(tc);
            }
            return tr;
        }

        private static string GetPersonFinCode(string? finCode) => string.IsNullOrWhiteSpace(finCode) ? "" : finCode!;

        private static string MapMonitorRole(byte? roleId, string? roleNameFromNav)
        {
            if (roleId.HasValue)
            {
                return roleId.Value switch
                {
                    1 => "İmtahan rəhbəri",
                    2 => "Nəzarətçi",
                    4 => "Könüllü",
                    _ => roleNameFromNav ?? $"Role_{roleId.Value}"
                };
            }
            return roleNameFromNav ?? "Monitor";
        }
        private static string MapRepresentativeRole(byte? roleId, string? roleNameFromNav)
        {
            if (roleId.HasValue)
            {
                return roleId.Value switch
                {
                    1 => "DİM nümayəndəsi",
                    2 => "GİN nümayəndəsi",
                    _ => roleNameFromNav ?? $"Role_{roleId.Value}"
                };
            }
            return roleNameFromNav ?? "Nümayəndə";
        }
        private static TableCell CreateTableCell(string text)
        {
            var tc = new TableCell();
            var p = new Paragraph(new Run(new Text(text ?? "")));

            p.ParagraphProperties = new ParagraphProperties(new KeepLines());

            tc.Append(p);
            tc.Append(new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }));
            return tc;
        }

        private class RowEntry
        {
            public string Rayon { get; set; } = "";
            public string BinaKodu { get; set; } = "";
            public string Soyad { get; set; } = "";
            public string Ad { get; set; } = "";
            public string Ata { get; set; } = "";
            public string Il { get; set; } = "";
            public string Gun { get; set; } = "";
            public string Ay { get; set; } = "";
            public string FinCode { get; set; } = "";
            public string Vezife { get; set; } = "";
        }
        public async Task<IEnumerable<ExamExportForFoodViewModel>> GetExamsForExportAsync()
        {
            var exams = await _examRepository.GetExamsForExportAsync();
            return exams.Select(e => new ExamExportForFoodViewModel
            {
                Id = e.Id,
                ExamBuildingName = e.ExamBuilding?.Name ?? "Bilinmeyen Bina",
                ExamDate = e.ExamDate,
                Food = e.Food,
                Water = e.Water
            });
        }
        public async Task<byte[]> GenerateExamWordReportAsync()
        {
            var exams = await GetExamsForExportAsync();

            using (var stream = new MemoryStream())
            {
                // Boş Word dokümanı oluştur
                using (var document = DocX.Create(stream))
                {
                    // Başlık ekle
                    document.InsertParagraph("İmtahan hesabatı")
                            .FontSize(20)
                            .Bold()
                            .Alignment = Alignment.center;

                    document.InsertParagraph("");

                    // Tablo oluştur
                    var table = document.AddTable(exams.Count() + 1, 5);
                    table.Design = TableDesign.TableGrid;
                    table.Alignment = Alignment.center;
                    table.AutoFit = AutoFit.Contents;

                    // Başlık satırı
                    table.Rows[0].Cells[0].Paragraphs[0].Append("№").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("İmtahan binası").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("İmtahan tarixi").Bold();
                    table.Rows[0].Cells[3].Paragraphs[0].Append("Verilən yemək").Bold();
                    table.Rows[0].Cells[4].Paragraphs[0].Append("Verilən su").Bold();

                    // Veri satırları
                    int rowIndex = 1;
                    foreach (var exam in exams)
                    {
                        table.Rows[rowIndex].Cells[0].Paragraphs[0].Append(exam.Id.ToString());
                        table.Rows[rowIndex].Cells[1].Paragraphs[0].Append(exam.ExamBuildingName);
                        table.Rows[rowIndex].Cells[2].Paragraphs[0].Append(exam.ExamDate.ToString("yyyy-MM-dd"));
                        table.Rows[rowIndex].Cells[3].Paragraphs[0].Append(exam.Food?.ToString() ?? "0");
                        table.Rows[rowIndex].Cells[4].Paragraphs[0].Append(exam.Water?.ToString() ?? "0");
                        rowIndex++;
                    }

                    document.InsertTable(table);
                    document.Save();
                }

                return stream.ToArray();
            }
        }
        public async Task<byte[]> ExportFoodWaterSimpleReportAsync(DateOnly start, DateOnly end)
        {
            var examsQuery = _context.Exams
                .Include(e => e.ExamBuilding)
                .Where(e => e.ExamDate >= start && e.ExamDate <= end && (e.Food > 0 || e.Water > 0));

            var exams = await examsQuery
                .OrderBy(e => e.ExamDate)
                .ThenBy(e => e.Id)
                .ToListAsync();

            var examData = exams.Select(e => new ExamExportForFoodViewModel
            {
                Id = e.Id,
                ExamBuildingName = e.ExamBuilding?.Name ?? "Bilinmeyen Bina",
                ExamDate = e.ExamDate,
                Food = e.Food ?? 0,
                Water = e.Water ?? 0
            }).ToList();

            using var stream = new MemoryStream();
            using (var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = mainPart.Document.AppendChild(new Body());

                // Başlık
                var titleParagraph = new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                    new Run(
                        new RunProperties(new Bold(), new FontSize { Val = "36" }),
                        new Text("İmtahan - Yemək/su hesabatı")
                    )
                );
                body.AppendChild(titleParagraph);

                // Tarih bilgisi
                var dateParagraph = new Paragraph(
                    new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
                    new Run(
                        new RunProperties(new FontSize { Val = "24" }),
                        new Text($"Tarix: {start} - {end}")
                    )
                );
                body.AppendChild(dateParagraph);
                body.AppendChild(new Paragraph());

                // Tablo oluştur
                var table = new Table();

                // Tablo özellikleri
                var tblProps = new TableProperties(
                    new TableWidth { Width = "100%", Type = TableWidthUnitValues.Pct },
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 6 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 6 }
                    )
                );
                table.AppendChild(tblProps);

                // Başlık satırı
                var headerRow = new TableRow();
                var headers = new[] { "№", "İmtahan binası", "İmtahan tarixi", "Verilən yemək", "Verilən su" };

                foreach (var header in headers)
                {
                    var cell = new TableCell(
                        new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }),
                        new Paragraph(
                            new Run(
                                new RunProperties(new Bold()),
                                new Text(header)
                            )
                        )
                    );
                    headerRow.AppendChild(cell);
                }
                table.AppendChild(headerRow);

                // Veri satırları
                for (int i = 0; i < examData.Count; i++)
                {
                    var exam = examData[i];
                    var row = new TableRow();

                    var cells = new[]
                              {
                                  (i + 1).ToString(), // sıra numarası
                                  exam.ExamBuildingName,
                                  exam.ExamDate.ToString("dd.MM.yyyy"),
                                  exam.Food.ToString(),
                                  exam.Water.ToString()
                              };

                    foreach (var cellText in cells)
                    {
                        var cell = new TableCell(
                            new TableCellProperties(new TableCellWidth { Type = TableWidthUnitValues.Auto }),
                            new Paragraph(new Run(new Text(cellText)))
                        );
                        row.AppendChild(cell);
                    }

                    table.AppendChild(row);
                }

                body.AppendChild(table);
                mainPart.Document.Save();
            }

            return stream.ToArray();
        }
        private static void AppendSignatureHeader(TableRow headerRow, byte? shift, int? sectionId)
        {
            if (shift == 2 && sectionId != 2)
            {
                headerRow.Append(CreateTableCell("İmza / I növbə", true, 2000));
                headerRow.Append(CreateTableCell("İmza / II növbə", true, 2000));
            }
            else
            {
                headerRow.Append(CreateTableCell("İmza", true, 2000));
            }
        }

        private static void AppendSignatureCells(TableRow row, byte? shift, int? sectionId)
        {
            row.Append(CreateTableCell("", false, 2000));
            if (shift == 2 && sectionId != 2)
            {
                row.Append(CreateTableCell("", false, 2000));
            }
        }
        private static string NormalizeSubProfessionName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            if (name.StartsWith("Sekundomer", StringComparison.OrdinalIgnoreCase))
                return "Sekundomer";

            return name;
        }

    }
}
