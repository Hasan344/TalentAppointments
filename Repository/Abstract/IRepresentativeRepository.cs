using ForQab.DataAccess.Models;

namespace ForQab.Repository.Abstract
{
    public interface IRepresentativeRepository : IBaseRepository<DimRepresentative>
    {
        Task<IEnumerable<DimRepresentative>> GetAllAsync();
        Task<List<DimRepresentative>> GetAvailableRepresentativeAsync(List<int> selectedRepresentativeList);
    }
}
