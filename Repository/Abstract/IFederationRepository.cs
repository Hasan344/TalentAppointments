using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract
{
    public interface IFederationRepository : IBaseRepository<Profession>
    {
        Task<List<Profession>> GetAllAsync(int? sectionId, Expression<Func<Profession, bool>> exp = null, params string[] includes);
    }
}
