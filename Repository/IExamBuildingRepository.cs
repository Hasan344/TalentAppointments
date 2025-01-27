using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository;

public interface IExamBuildingRepository : IBaseRepository<ExamBuilding>
{
    Task<List<ExamBuilding>> GetAllAsync(int? sectionId, Expression<Func<ExamBuilding, bool>> exp = null, params string[] includes);
}