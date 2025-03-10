using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Models.ViewModels;
using ForQab.Presentation.ViewModels;

namespace ForQab.Service.Abstract
{
    public interface IExamService
    {
        Task<Exam?> GetExamByIdAsync(int id);
        Task<IEnumerable<Exam>> GetAllExamsAsync();
        Task AddExamAsync(CreateExamViewModel exam);
        Task UpdateExamAsync(Exam exam);
        Task DeleteExamAsync(int id);
        Task<ExamDetailsViewModel> GetExamDetailsAsync(int examId);
        Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions, int federationId);
        Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignRandomHeadMonitorsToExamAsync(int examId, int numberOfMonitors, int genderId, DateOnly maxDate);
        Task AssignWorkersToExamAsync(int examId, List<int> selectedWorkerIds);
        Task<IEnumerable<Exam>> GetExamsBySectionIdAsync(int? sectionId);
        Task AssignRepresentativesToExamAsync(int examId, List<int> selectedRepresentativeIds);
        Task<List<DimRepresentative>> GetAvailableRepresentativesAsync();
        Task<List<DataAccess.Models.Monitor>> GetAvailableWorkersAsync(int buildingId);
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
        Task<ChangeExpertViewModel> GetChangeExpertViewModelAsync(int examId, int expertId);
        Task<bool> ChangeExpertAsync(int examId, int currentExpertId, int newExpertId);
        Task<ChangeMonitorViewModel> GetChangeMonitorViewModelAsync(int examId, int monitorId, int role);
        Task<ChangeRepresentativeViewModel> GetChangeRepresentativeViewModelAsync(int examId, int representativeId);
        Task<bool> ChangeMonitorAsync(ChangeMonitorViewModel model);
        Task<bool> ChangeRepresentativeAsync(ChangeRepresentativeViewModel model);
        Task<CreateExamViewModel> PrepareCreateExamViewModelAsync(int? sectionId);
        Task PopulateViewBagsAsync(int? sectionId, dynamic viewBag);
        Task<EditExamViewModel> PrepareEditExamViewModelAsync(int id, int? sectionId);
        //Task<bool> UpdateExpertsAsync(int examId, int[] selectedExpertIds);
        Task<AssignExpertToExamViewModel> PrepareAssignExpertsViewModelAsync(Exam exam);
        Task<byte[]> ExportExamMonitorsToWordAsync(int examId);

    }
}
