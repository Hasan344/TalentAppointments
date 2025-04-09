using ForQab.DataAccess.Models;

namespace ForQab.Service.Abstract
{
    public interface IMinistryRepresentativeService
    {
        Task<DimRepresentative> GetRepresentativeByIdAsync(int id);
        Task<IEnumerable<DimRepresentative>> GetAllRepresentativesAsync();
        Task AddRepresentativeAsync(DimRepresentative entity);
        Task UpdateRepresentativeAsync(DimRepresentative entity);
        Task DeleteRepresentativeAsync(int id);
    }
}