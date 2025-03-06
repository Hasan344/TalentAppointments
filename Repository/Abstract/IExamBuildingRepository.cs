using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract;

public interface IExamBuildingRepository : IBaseRepository<ExamBuilding>
{
    Task<List<ExamBuilding>> GetAllAsync(int? sectionId, Expression<Func<ExamBuilding, bool>> exp = null, params string[] includes);
    Task<IEnumerable<ExamBuilding>> GetAllAsync();
    Task<IEnumerable<ExamBuilding>> GetBySectionIdAsync(int sectionId);
}