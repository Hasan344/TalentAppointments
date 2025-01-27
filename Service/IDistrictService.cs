using ForQab.DataAccess.Models;
using ForQab.Repository;

namespace ForQab.Service;

public interface IDistrictService
{
    Task<District> GetDistrictByIdAsync(int id);
    Task<IEnumerable<District>> GetAllDistrictsAsync();
    Task AddDistrictAsync(District entity);
    Task UpdateDistrictAsync(District entity);
    Task DeleteDistrictAsync(int id);
}