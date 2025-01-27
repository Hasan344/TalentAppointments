using ForQab.DataAccess.Models;
using Monitor = ForQab.DataAccess.Models.Monitor;
namespace ForQab.Service
{
    public interface IVolunteerService
    {
        Task<Monitor> GetByIdAsync(int id);
        Task<IEnumerable<Monitor>> GetAllAsync(int? sectionId);
        Task AddAsync(Monitor entity);
        Task UpdateAsync(Monitor entity);
        Task DeleteAsync(int id);
        Task<IEnumerable<Section>> GetSectionsAsync(int? sectionId);
        Task BulkAddAsync(IEnumerable<Monitor> monitors);
    }
}
