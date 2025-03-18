using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract
{
    public interface IFederationService
    {
        Task<Profession> GetFederationByIdAsync(int id);
        Task<IEnumerable<Profession>> GetAllFederationsAsync(int? sectionId);
        Task AddFederationAsync(Profession entity);
        Task UpdateFederationAsync(Profession entity);
        Task DeleteFederationAsync(int id);
    }
}
