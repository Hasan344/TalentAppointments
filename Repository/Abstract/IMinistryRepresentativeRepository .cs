using ForQab.DataAccess.Models;

namespace ForQab.Repository.Abstract
{
    public interface IMinistryRepresentativeRepository : IBaseRepository<DimRepresentative>
    {
        Task<IEnumerable<DimRepresentative>> GetAllAsync();
        Task<List<DimRepresentative>> GetAvailableRepresentativeAsync(List<int> selectedRepresentativeList);
        Task<IEnumerable<DimRepresentative>> GetAllArchivedAsync();
        Task BulkArchiveAsync(List<int> ids, string archiveReason);
    }
}
