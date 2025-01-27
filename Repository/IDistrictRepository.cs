using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository;

public interface IDistrictRepository : IBaseRepository<District>
{
    Task<List<District>> GetAllAsync();
}
