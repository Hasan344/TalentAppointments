using ForQab.DataAccess.Models;

namespace ForQab.Repository.Abstract
{
    public interface ISectionRepository
    {
        Task<IEnumerable<Section>> GetAllAsync();
        Task<IEnumerable<Section>> GetByIdAsync(int sectionId);
    }
}
