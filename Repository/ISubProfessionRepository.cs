using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository
{
    public interface ISubProfessionRepository : IBaseRepository<SubProfession>
    {
        Task<List<SubProfession>> GetAllAsync(int? sectionId, Expression<Func<SubProfession, bool>> exp = null, params string[] includes);
    }
}
