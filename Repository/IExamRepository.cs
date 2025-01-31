using ForQab.DataAccess.Models;

namespace ForQab.Repository
{
    public interface IExamRepository : IRepository<Exam>
    {
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task<IEnumerable<SubProfession>> GetSubProfessionsBySectionIdAsync(int? sectionId);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        public Task<int> GetAvailableExpertsCountAsync(int sectionId, int[] selectedSubProfessions);
    }
}
