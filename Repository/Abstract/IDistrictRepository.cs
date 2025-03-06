using ForQab.DataAccess.Models;
using System.Linq.Expressions;

namespace ForQab.Repository.Abstract;

public interface IDistrictRepository : IBaseRepository<District>
{
    Task<List<District>> GetAllAsync();
}
