using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;

namespace ForQab.Repository
{
    public interface IExamRepository : IRepository<Exam>
    {
        public Task AddAsync(CreateExamViewModel examViewModel);
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        public Task<int> GetAvailableExpertsCountAsync(int sectionId, int[] selectedSubProfessions);
        public Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds);
        public Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId);
    }
}
