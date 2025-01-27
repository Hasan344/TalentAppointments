using ForQab.DataAccess.Models;

namespace ForQab.Service
{
    public interface IRepresentativeService
    {
        Task<DimRepresentative> GetRepresentativeByIdAsync(int id);
        Task<IEnumerable<DimRepresentative>> GetAllRepresentativesAsync();
        Task AddRepresentativeAsync(DimRepresentative entity);
        Task UpdateRepresentativeAsync(DimRepresentative entity);
        Task DeleteRepresentativeAsync(int id);
    }
}