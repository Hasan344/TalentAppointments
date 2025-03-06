using ForQab.DataAccess.Models;

namespace ForQab.Repository.Abstract
{
    public interface IExamExpertSubProfessionRepository
    {
        Task<List<ExamExpertSubProfession>> GetSubProfessionsByExpertAsync(int examId, int expertId);
        Task<List<ExamExpertSubProfession>> GetSubProfessionByExamAndExpertAsync(int examId, int expertId);
        Task RemoveSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions);
        Task AddSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions);
        Task<int> GetSubProfessionIdByExpertAsync(int examId, int expertId);
    }
}
