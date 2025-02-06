using ForQab.DataAccess.Models;
using ForQab.DataAccess.ViewModel.Exam;
using ForQab.Presentation.ViewModels;
using ForQab.Repository;

namespace ForQab.Service
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;

        public ExamService(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public async Task AddExamAsync(CreateExamViewModel exam)
        {
            await _examRepository.AddAsync(exam);
        }

        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions)
        {
            await _examRepository.AssignRandomExpertsToExamAsync(examId, numberOfExperts,  selectedSubProfessions);
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
                    model.ExamId, assignment.NumberOfExperts, assignment.SelectedSubProfessions);
            }

            return true;
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

        public async Task UpdateExamAsync(EditExamViewModel exam, int[] commissionIds)
        {
            await _examRepository.UpdateExamAsync(exam, commissionIds);
        }

        public async Task<IEnumerable<Commission>> GetCommissionsAsync(int? sectionId)
        {
           return await _examRepository.GetCommissionsAsync(sectionId);
        }
    }
}
