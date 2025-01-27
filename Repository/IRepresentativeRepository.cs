using ForQab.DataAccess.Models;

namespace ForQab.Repository
{
    public interface IRepresentativeRepository : IBaseRepository<DimRepresentative>
    {
        Task<IEnumerable<DimRepresentative>> GetAllAsync();
    }
}
