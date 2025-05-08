using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface IExamExpertSubProfessionRepository
    {
        Task<List<ExamExpertSubProfession>> GetSubProfessionsByExpertAsync(int examId, int expertId);
        Task<List<ExamExpertSubProfession>> GetSubProfessionByExamAndExpertAsync(int examId, int expertId);
        Task RemoveSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions);
        Task AddSubProfessionsAsync(List<ExamExpertSubProfession> subProfessions);
        Task<int> GetSubProfessionIdByExpertAsync(int examId, int expertId); 
        Task AddSubProfessionsAsync(IEnumerable<ExamExpertSubProfession> subProfessions);
        Task RemoveByExpertAsync(int examId, int expertId);
        Task<List<ExamExpertSubProfession>> GetAllAsync(Expression<Func<ExamExpertSubProfession, bool>> predicate);
        void RemoveRange(IEnumerable<ExamExpertSubProfession> entities); 
        Task<bool> IsExpertAssignedToExamAsync(int examId, int expertId);
    }
}
