using ForQab.Data_Access.ViewModel;
using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;

namespace ForQab.Repository
{
    public interface IExamRepository : IRepository<Exam>
    {
        public Task AddAsync(CreateExamViewModel examViewModel);
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        public Task<int> GetAvailableExpertsCountAsync(int sectionId, int[] selectedSubProfessions);
        public Task<int> GetAvailableMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate);
        public Task<int> GetAvailableHeadMonitorsCountAsync(int sectionId, int genderId, DateOnly maxDate);
        public Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds);
        public Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId);
        Task AddMonitorLogAsync(MonitorLog log);
        Task AddExpertLogAsync(ExpertLog logs);
        Task<List<int>> GetMonitorsWithLogsAsync(List<int> monitorIds);
        Task<List<int>> GetExpertsWithLogsAsync(List<int> expertIds);
        List<Expert> GetExpertsByExam(int examId);
        Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId);

    }
}
