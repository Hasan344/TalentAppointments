using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface ICommissionRepository : IBaseRepository<Commission>
    {
        public Task<List<Commission>> GetAllAsync(int? sectionId, int? role, Expression<Func<Commission, bool>> exp = null, params string[] includes);
    }
}
