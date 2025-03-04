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
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomWorkersToExamAsync(int examId, int numberOfMonitors, byte workerType);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds);
        Task<List<DimRepresentative>> GetAvailableRepresentativesAsync();
        Task<IEnumerable<SubProfession>> GetSubprofessionsBySectionIdAsync(int? sectionId);
        public Task<bool> AssignExpertsAsync(AssignExpertToExamViewModel model);
        public Task<int?> GetSectionIdByExamIdAsync(int examId);
        Task<MemoryStream> ExportExamScheduleToWord();
        public Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds, int[] degreeIds);
        public Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId);
        Task AddMonitorLogAsync(WriteMonitorLogViewModel model);
        Task AddExpertLogAsync(WriteExpertLogsViewModel model); 
        Task<List<int>> GetMonitorsWithLogsAsync(List<int> monitorIds);
        Task<List<int>> GetExpertsWithLogsAsync(List<int> expertIds);
        List<Expert> GetExpertsByExam(int examId);
        Task<List<ExamExpertSubProfession>> GetExpertSubProfessionsByExamIdAsync(int examId);

    }
}
