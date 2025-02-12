using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;

namespace ForQab.Service
{
    public interface IExamService
    {
        Task<Exam?> GetExamByIdAsync(int id);
        Task<IEnumerable<Exam>> GetAllExamsAsync();
        Task AddExamAsync(CreateExamViewModel exam);
        Task UpdateExamAsync(Exam exam);
        Task DeleteExamAsync(int id);
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubprofessionsBySectionIdAsync(int? sectionId);
        public Task<bool> AssignExpertsAsync(AssignExpertToExamViewModel model);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        public Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds);
        public Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId); 
        Task AddMonitorLogAsync(WriteMonitorLogViewModel model);
    }
}
