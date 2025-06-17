using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface ISubjectRepository : IBaseRepository<Subject>
    {
        Task<List<Subject>> GetAllAsync(int? sectionId, Expression<Func<Subject, bool>> exp = null, params string[] includes);
    }
}
