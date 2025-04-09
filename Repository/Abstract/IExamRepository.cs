using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace ForQab.Repository.Abstract
{
    public interface IExamRepository : IRepository<Exam>
    {
        public Task AddAsync(CreateExamViewModel examViewModel); 
        public Task AddAsyncForAssesment(CreateExamViewModelForAssesment examViewModel);
       // public Task AddAsyncForAppeal(CreateExamViewModelForAssesment examViewModel);
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId, int? roomId);
        Task AssignExpertsForMXToExamAsync(AssignExpertForMXToExamViewModel viewModel);
        Task AssignMonitorsForMXToExamAsync(AssignMonitorForMXToExamViewModel viewModel);
        Task AssignWorkersForMXToExamAsync(AssignWorkerForMXToExamViewModel viewModel);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate, int? roomId);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int? genderId, DateOnly? maxDate);
        Task AssignWorkersToExamAsync(int examId);
        Task AssignVolunteersToExamAsync(int examId);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId, int type);
        Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds);
        Task<List<DimRepresentative>> GetAvailableRepresentativesAsync();
        Task<List<DimRepresentative>> GetAvailableMinistryRepresentativesAsync();
        Task AssignMinistryRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds);
        Task<List<DataAccess.Models.Monitor>> GetAvailableWorkersAsync(int buildingId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        public Task<int> GetAvailableExpertsCountAsync(int sectionId, int[] selectedSubProfessions);
        public Task<int> GetAvailableMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate);
        public Task<int> GetAvailableHeadMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate);
        public Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds);
        public Task UpdateExamAsync(EditExamViewModelForAssesment exam);
        public Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId);
        Task<MemoryStream> ExportExamScheduleToWord();
        Task AddMonitorLogAsync(MonitorLog log);
        Task AddExpertLogAsync(ExpertLog logs);
        Task<List<int>> GetMonitorsWithLogsAsync(List<int> monitorIds);
        Task<List<int>> GetExpertsWithLogsAsync(List<int> expertIds);
        List<Expert> GetExpertsByExam(int examId);
        Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId); 
        Task<Exam> GetExamWithExpertsAndSubProfessionsAsync(int examId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveAsync();
        Task<Exam> GetExamWithMonitorsAsync(int examId);
        Task<Exam> GetExamWithRepresentativeAsync(int examId);
        Task<byte[]> ExportExamMonitorsToWordAsync(int examId);
        //Task<Exam> GetExamWithExpertsByIdAsync(int examId);
        //Task<List<Expert>> GetExpertsByIdsAsync(int[] expertIds);
    }
}
