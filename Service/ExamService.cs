using ForQab.DataAccess.Models;
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

        public async Task AddExamAsync(Exam exam)
        {
            await _examRepository.AddAsync(exam);
        }

        public async Task AssignRandomExpertsToExamAsync(int examId, int numberOfExperts, int[]? selectedSubProfessions)
        {
            await _examRepository.AssignRandomExpertsToExamAsync(examId, numberOfExperts,  selectedSubProfessions);
        }

        public async Task AssignRandomMonitorsToExamAsync(int examId, int numberOfMonitors)
        {
            await _examRepository.AssignRandomMonitorsToExamAsync(examId, numberOfMonitors);
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
        
    }
}
